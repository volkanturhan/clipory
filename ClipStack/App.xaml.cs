using System.Windows;
using ClipStack.Services;

// Enabling WinForms (for the tray icon) pulls System.Windows.Forms.Application
// into scope too, so spell out that "Application" here means the WPF one.
using Application = System.Windows.Application;

namespace ClipStack;

/// <summary>
/// Application entry point. Wires together the long-lived pieces of ClipStack
/// and runs it as a tray application: there is no window on startup, the app
/// lives in the system tray, and it only exits when the user chooses "Quit".
/// </summary>
public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private ClipboardHistory _history = null!;
    private ClipboardMonitor _monitor = null!;
    private TrayIcon _tray = null!;
    private MainWindow _window = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Only one ClipStack should watch the clipboard at a time. If another
        // instance already holds the mutex, bow out quietly.
        _singleInstanceMutex = new Mutex(initiallyOwned: true,
            @"Local\ClipStack.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);

        // No visible window means closing the WPF window must not end the app;
        // shutdown is driven explicitly from the tray's Quit command.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _history = new ClipboardHistory();

        // Every captured clip flows into the history store.
        _monitor = new ClipboardMonitor();
        _monitor.TextCaptured += _history.Add;

        // The window is created once and reused; closing it just hides it back
        // into the tray (see MainWindow.OnClosing).
        _window = new MainWindow(_history);

        _tray = new TrayIcon();
        _tray.OpenRequested += ShowWindow;
        _tray.QuitRequested += Shutdown;
    }

    /// <summary>Brings the history window to the foreground, restoring it if hidden.</summary>
    private void ShowWindow()
    {
        _window.Show();

        if (_window.WindowState == WindowState.Minimized)
            _window.WindowState = WindowState.Normal;

        _window.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _monitor?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
