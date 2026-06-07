using System.Drawing;
using System.Windows.Forms;

namespace ClipStack.Services;

/// <summary>
/// The system-tray presence for ClipStack. While the app is idle it lives here
/// rather than on the taskbar. Double-clicking the icon or its "Open" menu item
/// raises <see cref="OpenRequested"/>; the "Quit" item raises
/// <see cref="QuitRequested"/>. The app decides what those actions actually do.
///
/// Backed by the WinForms <see cref="NotifyIcon"/>, which ships with the .NET
/// SDK so ClipStack needs no third-party tray library.
/// </summary>
public sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Icon? _icon;

    /// <summary>Raised when the user asks to open the history window.</summary>
    public event Action? OpenRequested;

    /// <summary>Raised when the user asks to quit the application.</summary>
    public event Action? QuitRequested;

    public TrayIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open ClipStack", null, (_, _) => OpenRequested?.Invoke());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => QuitRequested?.Invoke());

        _icon = TryLoadAppIcon();

        _notifyIcon = new NotifyIcon
        {
            // Fall back to a generic icon if ours fails to load — never crash
            // the whole app over a tray icon.
            Icon = _icon ?? SystemIcons.Application,
            Text = "ClipStack",
            Visible = true,
            ContextMenuStrip = menu,
        };

        _notifyIcon.DoubleClick += (_, _) => OpenRequested?.Invoke();
    }

    /// <summary>
    /// Loads the bundled ClipStack icon at the system's small-icon size so the
    /// tray gets a crisp frame. Returns null on any failure.
    /// </summary>
    private static Icon? TryLoadAppIcon()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/ClipStack.ico");
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
        // Hide before disposing so the icon disappears immediately instead of
        // lingering in the tray until the user hovers over it.
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon?.Dispose();
    }
}
