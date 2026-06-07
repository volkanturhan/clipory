using System.Windows;
using ClipStack.Services;

namespace ClipStack;

/// <summary>
/// Application entry point. Wires together the long-lived pieces of ClipStack:
/// the in-memory history and the clipboard monitor that feeds it.
/// </summary>
public partial class App : Application
{
    private ClipboardHistory _history = null!;
    private ClipboardMonitor _monitor = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _history = new ClipboardHistory();

        // Every captured clip flows into the history store.
        _monitor = new ClipboardMonitor();
        _monitor.TextCaptured += _history.Add;

        // For now we show the history in a plain window so we can watch capture
        // working. A later step turns this into a tray app with a popup.
        var window = new MainWindow(_history);
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _monitor?.Dispose();
        base.OnExit(e);
    }
}
