using System.Windows;
using System.Windows.Threading;
using clipory.Models;
using clipory.Services;

// Enabling WinForms (for the tray icon) pulls the System.Windows.Forms versions
// of these types into scope too, so spell out that we mean the WPF ones.
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Localization = clipory.Services.Localization;

namespace clipory;

/// <summary>
/// Application entry point. Wires together the long-lived pieces of clipory
/// and runs it as a tray application: there is no window on startup, the app
/// lives in the system tray, and it only exits when the user chooses "Quit".
///
/// The core flow: press Ctrl+Shift+V → the popup appears over the current app →
/// pick a clip → it is pasted straight back into that app.
/// </summary>
public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private SettingsStore _settings = null!;
    private HistoryStorage _storage = null!;
    private ClipboardHistory _history = null!;
    private ClipboardMonitor _monitor = null!;
    private HotkeyService _hotkey = null!;
    private TrayIcon _tray = null!;
    private MainWindow _window = null!;
    private AboutWindow? _aboutWindow;

    private UpdateService _updates = null!;
    // Periodically re-checks for updates so a long-running instance still notices.
    private DispatcherTimer? _updateTimer;
    // The newer release found by the background check, awaiting the user's nod.
    private UpdateService.AvailableUpdate? _pendingUpdate;

    // The window that was focused when the popup was summoned, so a chosen clip
    // can be pasted back into it. IntPtr.Zero means "don't paste, just copy".
    private IntPtr _pasteTarget;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Only one clipory should watch the clipboard at a time. If another
        // instance already holds the mutex, bow out quietly.
        _singleInstanceMutex = new Mutex(initiallyOwned: true,
            @"Local\clipory.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);

        // No visible window means closing the WPF window must not end the app;
        // shutdown is driven explicitly from the tray's Quit command.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Apply the saved language before any UI is built, then persist changes.
        _settings = new SettingsStore();
        Localization.Instance.Language = _settings.LoadLanguage();
        Localization.Instance.LanguageChanged +=
            () => _settings.SaveLanguage(Localization.Instance.Language);

        // Apply the saved colour theme before any window is built, then persist.
        ThemeService.Apply(_settings.LoadTheme());
        ThemeService.Changed += () => _settings.SaveTheme(ThemeService.Theme);

        // Restore the saved history, then keep persisting it as it changes.
        _storage = new HistoryStorage();
        _history = new ClipboardHistory();
        _history.Initialize(_storage.Load());
        _history.Changed += () => _storage.Save(_history.Items);

        // Every captured clip flows into the history store.
        _monitor = new ClipboardMonitor();
        _monitor.TextCaptured += _history.Add;

        // The popup is created once and reused; it hides instead of closing.
        _window = new MainWindow(_history);
        _window.EntryChosen += OnEntryChosen;

        // Ctrl+Shift+V summons the popup over whatever app the user is in.
        _hotkey = new HotkeyService();
        _hotkey.Pressed += ShowPopupForPaste;

        _tray = new TrayIcon();
        _tray.OpenRequested += ShowPopupForBrowsing;
        _tray.ClearHistoryRequested += _history.ClearUnpinned;
        _tray.AboutRequested += ShowAbout;
        _tray.UpdateRequested += InstallPendingUpdate;
        _tray.CheckUpdateRequested += () => _ = CheckForUpdateAsync(announceWhenCurrent: true);
        _tray.QuitRequested += Shutdown;

        // Quietly ask GitHub whether a newer clipory exists; if so the tray will
        // offer it. Fire-and-forget so a slow network never delays startup.
        _updates = new UpdateService();
        _ = CheckForUpdateAsync(announceWhenCurrent: false);

        // Re-check every few hours so an instance left running for days still
        // notices a new release without needing a restart.
        _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromHours(6) };
        _updateTimer.Tick += (_, _) => _ = CheckForUpdateAsync(announceWhenCurrent: false);
        _updateTimer.Start();

        // Lets the tray's "Open" be reproduced from the command line.
        if (e.Args.Contains("--open"))
            ShowPopupForBrowsing();
    }

    /// <summary>
    /// Background check for a newer release. The await resumes on the UI thread,
    /// so touching the tray here is safe. Silent on failure by design.
    /// </summary>
    private async Task CheckForUpdateAsync(bool announceWhenCurrent)
    {
        _pendingUpdate = await _updates.CheckForUpdateAsync();
        if (_pendingUpdate is not null)
            _tray.ShowUpdateAvailable(_pendingUpdate.Version.ToString(3));
        else if (announceWhenCurrent)
            _tray.ShowUpToDate();   // give feedback only for a manual check
    }

    /// <summary>
    /// Downloads and launches the installer for the pending update, then quits so
    /// it can replace clipory's files. Tells the user if the download fails.
    /// </summary>
    private async void InstallPendingUpdate()
    {
        if (_pendingUpdate is null)
            return;

        try
        {
            await _updates.DownloadAndLaunchInstallerAsync(_pendingUpdate);
            Shutdown();
        }
        catch
        {
            MessageBox.Show(Localization.Instance["UpdateFailed"], "clipory",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>Shows the About window, reusing it if already open.</summary>
    private void ShowAbout()
    {
        if (_aboutWindow is not null)
        {
            _aboutWindow.Activate();
            return;
        }

        _aboutWindow = new AboutWindow();
        _aboutWindow.Closed += (_, _) => _aboutWindow = null;
        _aboutWindow.Show();
    }

    /// <summary>Hotkey flow: remember the active app so we can paste back into it.</summary>
    private void ShowPopupForPaste()
    {
        _pasteTarget = ForegroundPaste.CaptureForegroundWindow();
        _window.ShowAsPopup();
    }

    /// <summary>
    /// Tray flow: just browse the history. There is no meaningful app to paste
    /// into, so a chosen clip is only copied to the clipboard.
    /// </summary>
    private void ShowPopupForBrowsing()
    {
        _pasteTarget = IntPtr.Zero;
        _window.ShowAsPopup();
    }

    private void OnEntryChosen(ClipboardEntry entry)
    {
        _window.Hide();

        // Put the chosen text on the clipboard so it is ready to paste anywhere.
        Clipboard.SetText(entry.Text);

        // Paste it back into the originating app once the popup has gone away,
        // so focus has already returned before we send the keystroke.
        var target = _pasteTarget;
        Dispatcher.BeginInvoke(() => ForegroundPaste.PasteInto(target),
            System.Windows.Threading.DispatcherPriority.Background);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _hotkey?.Dispose();
        _monitor?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
