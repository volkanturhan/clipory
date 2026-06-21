using System.Runtime.InteropServices;

namespace clipory.Services;

/// <summary>
/// Pastes a chosen clip into whichever window the user was working in before
/// clipory's popup appeared.
///
/// The flow is: capture the foreground window <em>before</em> showing the popup
/// (showing it would otherwise steal the foreground), then later restore that
/// window and simulate a Ctrl+V keystroke so the text lands where the user
/// expects it.
/// </summary>
public static class ForegroundPaste
{
    private const byte VirtualKeyControl = 0x11;
    private const byte VirtualKeyV = 0x56;
    private const uint KeyEventKeyUp = 0x0002;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    // keybd_event is the simplest reliable way to synthesise a Ctrl+V here.
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    /// <summary>Returns the window that currently has focus.</summary>
    public static IntPtr CaptureForegroundWindow() => GetForegroundWindow();

    /// <summary>
    /// Brings <paramref name="targetWindow"/> back to the front and sends it a
    /// Ctrl+V. Does nothing if the handle is empty (e.g. opened from the tray
    /// rather than over another app), leaving the text on the clipboard.
    /// </summary>
    public static void PasteInto(IntPtr targetWindow)
    {
        if (targetWindow == IntPtr.Zero)
            return;

        SetForegroundWindow(targetWindow);
        SendCtrlV();
    }

    private static void SendCtrlV()
    {
        keybd_event(VirtualKeyControl, 0, 0, UIntPtr.Zero);          // Ctrl down
        keybd_event(VirtualKeyV, 0, 0, UIntPtr.Zero);               // V down
        keybd_event(VirtualKeyV, 0, KeyEventKeyUp, UIntPtr.Zero);   // V up
        keybd_event(VirtualKeyControl, 0, KeyEventKeyUp, UIntPtr.Zero); // Ctrl up
    }
}
