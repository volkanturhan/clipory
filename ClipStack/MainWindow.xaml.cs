using System.ComponentModel;
using System.Windows;
using ClipStack.Services;

namespace ClipStack;

/// <summary>
/// Shows the clipboard history. For now it is opened from the tray; a later
/// step adds a global hotkey and turns it into a quick-pick popup.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(ClipboardHistory history)
    {
        InitializeComponent();

        // Bind the list to the live history so new clips appear automatically.
        DataContext = history.Items;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Closing the window just tucks it back into the tray; the app keeps
        // running. Real shutdown happens from the tray's Quit command.
        e.Cancel = true;
        Hide();

        base.OnClosing(e);
    }
}
