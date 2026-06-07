using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ClipStack.Models;
using ClipStack.Services;

// WinForms is enabled for the tray icon, so disambiguate the input event args
// in favour of the WPF ones this window actually uses.
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ClipStack;

/// <summary>
/// The quick-pick popup. It shows the clipboard history, lets the user filter
/// it by typing, and raises <see cref="EntryChosen"/> when an item is picked
/// (by Enter or double-click). The application decides what "chosen" means —
/// typically: copy it and paste it back into the previous app.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ICollectionView _view;
    private string _searchText = string.Empty;

    // True only during the show sequence. Forcing the window to the foreground
    // can fire a transient Deactivated; while showing we ignore it so the popup
    // does not immediately hide itself.
    private bool _isShowing;

    /// <summary>Raised when the user picks an entry from the list.</summary>
    public event Action<ClipboardEntry>? EntryChosen;

    public MainWindow(ClipboardHistory history)
    {
        InitializeComponent();

        // A collection view lets us filter the live history by the search text
        // without touching the underlying store.
        _view = CollectionViewSource.GetDefaultView(history.Items);
        _view.Filter = MatchesSearch;
        HistoryList.ItemsSource = _view;
    }

    /// <summary>
    /// Shows the popup at the mouse cursor, cleared and ready for typing with
    /// the first item pre-selected so Enter pastes immediately.
    /// </summary>
    public void ShowAsPopup()
    {
        _isShowing = true;

        SearchBox.Text = string.Empty; // triggers a refresh via TextChanged
        PositionAtCursor();

        Show();

        // A global hotkey fires while another app is in front, so a plain
        // Activate() is often refused. Force ourselves to the foreground.
        WindowActivator.ForceToForeground(new WindowInteropHelper(this).Handle);
        Activate();

        SelectFirst();
        SearchBox.Focus();

        // Re-enable click-away-to-close once the show has fully settled.
        Dispatcher.BeginInvoke(() => _isShowing = false, DispatcherPriority.Background);
    }

    private bool MatchesSearch(object item)
    {
        if (_searchText.Length == 0)
            return true;

        return item is ClipboardEntry entry
            && entry.Text.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = SearchBox.Text;
        _view.Refresh();
        SelectFirst();
    }

    private void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            // Let the arrow keys drive the list while the caret stays in the box.
            case Key.Down:
                MoveSelection(+1);
                e.Handled = true;
                break;
            case Key.Up:
                MoveSelection(-1);
                e.Handled = true;
                break;
            case Key.Enter:
                ChooseSelected();
                e.Handled = true;
                break;
            case Key.Escape:
                Hide();
                e.Handled = true;
                break;
        }
    }

    private void OnListDoubleClick(object sender, MouseButtonEventArgs e) => ChooseSelected();

    // Clicking outside the popup dismisses it, the way a launcher menu would —
    // but not during the show sequence, which can briefly deactivate us.
    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (!_isShowing)
            Hide();
    }

    private void SelectFirst()
    {
        HistoryList.SelectedIndex = HistoryList.Items.Count > 0 ? 0 : -1;
        ScrollToSelection();
    }

    private void MoveSelection(int delta)
    {
        var count = HistoryList.Items.Count;
        if (count == 0)
            return;

        var next = HistoryList.SelectedIndex + delta;
        HistoryList.SelectedIndex = Math.Clamp(next, 0, count - 1);
        ScrollToSelection();
    }

    private void ScrollToSelection()
    {
        if (HistoryList.SelectedItem is not null)
            HistoryList.ScrollIntoView(HistoryList.SelectedItem);
    }

    private void ChooseSelected()
    {
        if (HistoryList.SelectedItem is ClipboardEntry entry)
            EntryChosen?.Invoke(entry);
    }

    /// <summary>
    /// Places the popup near the mouse cursor, kept fully inside the working
    /// area of whichever monitor the cursor is on. Cursor and screen bounds are
    /// in physical pixels, so we scale them to WPF's device-independent units.
    /// </summary>
    private void PositionAtCursor()
    {
        var cursor = System.Windows.Forms.Cursor.Position;
        var work = System.Windows.Forms.Screen.FromPoint(cursor).WorkingArea;

        using var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
        var scaleX = graphics.DpiX / 96.0;
        var scaleY = graphics.DpiY / 96.0;

        var widthPx = Width * scaleX;
        var heightPx = Height * scaleY;

        var x = Math.Clamp(cursor.X, work.Left, Math.Max(work.Left, work.Right - (int)widthPx));
        var y = Math.Clamp(cursor.Y, work.Top, Math.Max(work.Top, work.Bottom - (int)heightPx));

        Left = x / scaleX;
        Top = y / scaleY;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Closing (e.g. Alt+F4) just hides the popup; the app keeps running and
        // is shut down from the tray's Quit command.
        e.Cancel = true;
        Hide();

        base.OnClosing(e);
    }
}
