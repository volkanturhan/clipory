using System.Windows;
using ClipStack.Services;

namespace ClipStack;

/// <summary>
/// Shows the clipboard history. For now it is a simple always-visible list;
/// a later step turns it into the popup that the global hotkey brings up.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(ClipboardHistory history)
    {
        InitializeComponent();

        // Bind the list to the live history so new clips appear automatically.
        DataContext = history.Items;
    }
}
