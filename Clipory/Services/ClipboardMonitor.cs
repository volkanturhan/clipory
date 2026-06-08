using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// With WinForms enabled (for the tray icon) "Clipboard" is ambiguous; we want
// the WPF clipboard API here.
using Clipboard = System.Windows.Clipboard;

namespace Clipory.Services;

/// <summary>
/// Watches the Windows clipboard and raises <see cref="TextCaptured"/> whenever
/// new text is copied anywhere on the system.
///
/// Windows notifies listeners of clipboard changes by posting a window message,
/// so we need a window to receive it. Clipory has no visible window of its own
/// while idle, so this class spins up a <em>message-only</em> window: an
/// invisible window that exists purely to receive messages. We register it with
/// <c>AddClipboardFormatListener</c> and translate each <c>WM_CLIPBOARDUPDATE</c>
/// into a <see cref="TextCaptured"/> event.
/// </summary>
public sealed class ClipboardMonitor : IDisposable
{
    // Posted to every clipboard-format listener when the clipboard changes.
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    // Special parent handle that turns a new window into a message-only window.
    private static readonly IntPtr HWND_MESSAGE = new(-3);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private readonly HwndSource _source;
    private bool _disposed;

    /// <summary>Raised with the copied text each time the clipboard changes.</summary>
    public event Action<string>? TextCaptured;

    public ClipboardMonitor()
    {
        // Build the invisible message-only window and start listening on it.
        var parameters = new HwndSourceParameters("CliporyClipboardMonitor")
        {
            ParentWindow = HWND_MESSAGE,
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        if (!AddClipboardFormatListener(_source.Handle))
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                "Failed to register the clipboard listener.");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            CaptureClipboardText();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void CaptureClipboardText()
    {
        // The app that just wrote to the clipboard may still hold a lock on it,
        // so reading can briefly fail. We ignore those failures rather than let
        // an exception tear down the listener.
        try
        {
            if (!Clipboard.ContainsText())
                return;

            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text))
                TextCaptured?.Invoke(text);
        }
        catch (COMException)
        {
            // Clipboard was momentarily unavailable; skip this notification.
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        RemoveClipboardFormatListener(_source.Handle);
        _source.RemoveHook(WndProc);
        _source.Dispose();
    }
}
