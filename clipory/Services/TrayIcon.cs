using System.Drawing;
using System.Windows.Forms;

namespace clipory.Services;

/// <summary>
/// The system-tray presence for clipory. While the app is idle it lives here
/// rather than on the taskbar. The context menu exposes the app's actions and
/// settings; the events below let the application decide what each one does.
///
/// Menu text follows the app language: the menu is built once and its labels
/// are refreshed whenever <see cref="Localization"/> changes.
///
/// Backed by the WinForms <see cref="NotifyIcon"/>, which ships with the .NET
/// SDK so clipory needs no third-party tray library.
/// </summary>
public sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Icon? _icon;

    // Hidden until an update is found; shown bold at the top of the menu, with
    // its own separator, so it stands out without cluttering the normal menu.
    private readonly ToolStripMenuItem _updateItem = new() { Visible = false };
    private readonly ToolStripSeparator _updateSeparator = new() { Visible = false };
    private string? _updateVersion;

    private readonly ToolStripMenuItem _openItem = new();
    private readonly ToolStripMenuItem _clearItem = new();
    private readonly ToolStripMenuItem _autoStartItem = new() { CheckOnClick = true };
    private readonly ToolStripMenuItem _languageItem = new();
    private readonly ToolStripMenuItem _englishItem = new("English");
    private readonly ToolStripMenuItem _turkishItem = new("Türkçe");
    private readonly ToolStripMenuItem _themeItem = new();
    private readonly ToolStripMenuItem _systemThemeItem = new();
    private readonly ToolStripMenuItem _darkThemeItem = new();
    private readonly ToolStripMenuItem _lightThemeItem = new();
    private readonly ToolStripMenuItem _checkUpdateItem = new();
    private readonly ToolStripMenuItem _aboutItem = new();
    private readonly ToolStripMenuItem _quitItem = new();

    /// <summary>Raised when the user asks to open the history window.</summary>
    public event Action? OpenRequested;

    /// <summary>Raised when the user asks to clear the (unpinned) history.</summary>
    public event Action? ClearHistoryRequested;

    /// <summary>Raised when the user asks to see the About window.</summary>
    public event Action? AboutRequested;

    /// <summary>Raised when the user asks to quit the application.</summary>
    public event Action? QuitRequested;

    /// <summary>Raised when the user accepts the offered update.</summary>
    public event Action? UpdateRequested;

    /// <summary>Raised when the user asks to check for updates now.</summary>
    public event Action? CheckUpdateRequested;

    public TrayIcon()
    {
        // The update entry is drawn bold to read as the call-to-action it is.
        _updateItem.Font = new Font(SystemFonts.MenuFont!, FontStyle.Bold);
        _updateItem.Click += (_, _) => UpdateRequested?.Invoke();

        _openItem.Click += (_, _) => OpenRequested?.Invoke();
        _clearItem.Click += (_, _) => ClearHistoryRequested?.Invoke();
        _autoStartItem.Checked = AutoStart.IsEnabled();
        _autoStartItem.CheckedChanged += (_, _) => AutoStart.SetEnabled(_autoStartItem.Checked);
        _aboutItem.Click += (_, _) => AboutRequested?.Invoke();
        _quitItem.Click += (_, _) => QuitRequested?.Invoke();

        _englishItem.Click += (_, _) => Localization.Instance.Language = AppLanguage.English;
        _turkishItem.Click += (_, _) => Localization.Instance.Language = AppLanguage.Turkish;
        _languageItem.DropDownItems.Add(_englishItem);
        _languageItem.DropDownItems.Add(_turkishItem);

        _systemThemeItem.Click += (_, _) => ThemeService.Apply(AppTheme.System);
        _darkThemeItem.Click += (_, _) => ThemeService.Apply(AppTheme.Dark);
        _lightThemeItem.Click += (_, _) => ThemeService.Apply(AppTheme.Light);
        _themeItem.DropDownItems.Add(_systemThemeItem);
        _themeItem.DropDownItems.Add(_darkThemeItem);
        _themeItem.DropDownItems.Add(_lightThemeItem);

        _checkUpdateItem.Click += (_, _) => CheckUpdateRequested?.Invoke();

        var menu = new ContextMenuStrip();
        menu.Items.AddRange(new ToolStripItem[]
        {
            _updateItem,
            _updateSeparator,
            _openItem,
            _clearItem,
            _autoStartItem,
            _languageItem,
            _themeItem,
            _checkUpdateItem,
            _aboutItem,
            new ToolStripSeparator(),
            _quitItem,
        });

        _icon = TryLoadAppIcon();
        _notifyIcon = new NotifyIcon
        {
            // Fall back to a generic icon if ours fails to load — never crash
            // the whole app over a tray icon.
            Icon = _icon ?? SystemIcons.Application,
            Text = "clipory",
            Visible = true,
            ContextMenuStrip = menu,
        };
        _notifyIcon.DoubleClick += (_, _) => OpenRequested?.Invoke();
        // We only ever raise a balloon for an update, so clicking it means "yes".
        _notifyIcon.BalloonTipClicked += (_, _) => UpdateRequested?.Invoke();

        Localization.Instance.LanguageChanged += ApplyLanguage;
        // Re-tick the active theme entry whenever the theme changes.
        ThemeService.Changed += ApplyLanguage;
        ApplyLanguage();
    }

    // Refresh every menu label from the current language and tick the active
    // language entry.
    private void ApplyLanguage()
    {
        var text = Localization.Instance;
        _openItem.Text = text["TrayOpen"];
        _clearItem.Text = text["TrayClear"];
        _autoStartItem.Text = text["TrayAutostart"];
        _languageItem.Text = text["TrayLanguage"];
        _themeItem.Text = text["TrayTheme"];
        _systemThemeItem.Text = text["ThemeSystem"];
        _darkThemeItem.Text = text["ThemeDark"];
        _lightThemeItem.Text = text["ThemeLight"];
        _checkUpdateItem.Text = text["TrayCheckUpdate"];
        _aboutItem.Text = text["TrayAbout"];
        _quitItem.Text = text["TrayQuit"];

        // Keep the (version-stamped) update label in the current language too.
        if (_updateVersion is not null)
            _updateItem.Text = string.Format(text["TrayUpdate"], _updateVersion);

        _englishItem.Checked = text.Language == AppLanguage.English;
        _turkishItem.Checked = text.Language == AppLanguage.Turkish;

        _systemThemeItem.Checked = ThemeService.Theme == AppTheme.System;
        _darkThemeItem.Checked = ThemeService.Theme == AppTheme.Dark;
        _lightThemeItem.Checked = ThemeService.Theme == AppTheme.Light;
    }

    /// <summary>
    /// Reveals the update entry for <paramref name="version"/> and shows a tray
    /// balloon so the user notices even without opening the menu. Call on the UI
    /// thread once a newer release has been found.
    /// </summary>
    public void ShowUpdateAvailable(string version)
    {
        _updateVersion = version;
        _updateItem.Visible = true;
        _updateSeparator.Visible = true;
        ApplyLanguage();

        var text = Localization.Instance;
        _notifyIcon.BalloonTipTitle = text["UpdateBalloonTitle"];
        _notifyIcon.BalloonTipText = text["UpdateBalloonText"];
        _notifyIcon.ShowBalloonTip(5000);
    }

    /// <summary>
    /// Shows a brief "you're up to date" balloon. Used to give feedback when the
    /// user checks for updates manually and there is nothing newer.
    /// </summary>
    public void ShowUpToDate()
    {
        var text = Localization.Instance;
        _notifyIcon.BalloonTipTitle = text["UpdateBalloonTitle"];
        _notifyIcon.BalloonTipText = text["UpToDate"];
        _notifyIcon.ShowBalloonTip(4000);
    }

    /// <summary>
    /// Loads the bundled clipory icon at the system's small-icon size so the
    /// tray gets a crisp frame. Returns null on any failure.
    /// </summary>
    private static Icon? TryLoadAppIcon()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/clipory.ico");
            using var stream = System.Windows.Application.GetResourceStream(uri).Stream;
            return new Icon(stream, SystemInformation.SmallIconSize);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        Localization.Instance.LanguageChanged -= ApplyLanguage;
        ThemeService.Changed -= ApplyLanguage;

        // Hide before disposing so the icon disappears immediately instead of
        // lingering in the tray until the user hovers over it.
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon?.Dispose();
    }
}
