using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using clipory.Models;
using clipory.Services;

// WinForms is enabled for the tray icon, so disambiguate the input event args
// in favour of the WPF ones this window actually uses.
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace clipory;

/// <summary>
/// The quick-pick popup. It shows the clipboard history, lets the user filter
/// it by typing, and raises <see cref="EntryChosen"/> when an item is picked
/// (by Enter or double-click). The application decides what "chosen" means —
/// typically: copy it and paste it back into the previous app.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ClipboardHistory _history;
    private readonly ICollectionView _view;
    private string _searchText = string.Empty;

    // True only during the show sequence. Forcing the window to the foreground
    // can fire a transient Deactivated; while showing we ignore it so the popup
    // does not immediately hide itself.
    private bool _isShowing;

    // The moving end (focus) and fixed end (anchor) of a keyboard range
    // selection driven by Shift+↑/↓. They collapse together on a single select.
    private int _focusIndex = -1;
    private int _anchorIndex = -1;

    /// <summary>Raised when the user picks an entry from the list.</summary>
    public event Action<ClipboardEntry>? EntryChosen;

    public MainWindow(ClipboardHistory history)
    {
        InitializeComponent();

        _history = history;

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
        // Ctrl+P pins or unpins the selected clip.
        if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
        {
            PinSelected();
            e.Handled = true;
            return;
        }

        switch (e.Key)
        {
            // Arrows drive the list while the caret stays in the box; holding
            // Shift extends a contiguous range instead of moving a single pick.
            case Key.Down:
                MoveSelection(+1, extend: Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                e.Handled = true;
                break;
            case Key.Up:
                MoveSelection(-1, extend: Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                e.Handled = true;
                break;
            case Key.Enter:
                ChooseSelected();
                e.Handled = true;
                break;
            case Key.Delete:
                DeleteSelected();
                e.Handled = true;
                break;
            case Key.Escape:
                Hide();
                e.Handled = true;
                break;
        }
    }

    private void OnListDoubleClick(object sender, MouseButtonEventArgs e) => ChooseSelected();

    // Context-menu actions. The menu inherits the row's data context, so the
    // sender's DataContext is the entry that was right-clicked.
    private void OnPinClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ClipboardEntry entry })
            _history.TogglePin(entry);
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: ClipboardEntry clicked })
            return;

        // If the right-clicked row is part of a multi-selection, clear the whole
        // selection; otherwise just remove the one that was clicked.
        if (HistoryList.SelectedItems.Count > 1 && HistoryList.SelectedItems.Contains(clicked))
            DeleteSelected();
        else
            _history.Remove(clicked);
    }

    private void PinSelected()
    {
        if (HistoryList.SelectedItem is ClipboardEntry entry)
            _history.TogglePin(entry);
    }

    private void DeleteSelected()
    {
        // Snapshot first: removing entries mutates the live SelectedItems list.
        var selected = HistoryList.SelectedItems.Cast<ClipboardEntry>().ToList();
        if (selected.Count == 0)
            return;

        // Remember where the (top of the) removed block was, to re-anchor after.
        var landing = Math.Min(_anchorIndex < 0 ? HistoryList.SelectedIndex : _anchorIndex,
                               _focusIndex < 0 ? HistoryList.SelectedIndex : _focusIndex);

        foreach (var entry in selected)
            _history.Remove(entry);

        // Keep a sensible neighbour selected where the block used to be.
        if (HistoryList.Items.Count > 0)
            SelectSingle(Math.Clamp(landing, 0, HistoryList.Items.Count - 1));
        else
            SelectFirst();
    }

    // Clicking outside the popup dismisses it, the way a launcher menu would —
    // but not during the show sequence, which can briefly deactivate us.
    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (!_isShowing)
            Hide();
    }

    private void SelectFirst()
    {
        if (HistoryList.Items.Count > 0)
        {
            SelectSingle(0);
        }
        else
        {
            _anchorIndex = _focusIndex = -1;
            HistoryList.SelectedIndex = -1;
        }
    }

    private void MoveSelection(int delta, bool extend)
    {
        var count = HistoryList.Items.Count;
        if (count == 0)
            return;

        if (!extend)
        {
            // Plain move: collapse to one item and walk it up or down.
            var from = HistoryList.SelectedIndex < 0 ? 0 : HistoryList.SelectedIndex;
            SelectSingle(Math.Clamp(from + delta, 0, count - 1));
            return;
        }

        // Extending: anchor on the current single selection unless we are already
        // mid-range (then keep growing from the tracked focus end).
        if (HistoryList.SelectedItems.Count <= 1)
            _anchorIndex = _focusIndex = HistoryList.SelectedIndex < 0 ? 0 : HistoryList.SelectedIndex;

        _focusIndex = Math.Clamp(_focusIndex + delta, 0, count - 1);
        SelectRange(_anchorIndex, _focusIndex);
        ScrollIntoViewAt(_focusIndex);
    }

    // Selects exactly one row and resets the range ends to it.
    private void SelectSingle(int index)
    {
        _anchorIndex = _focusIndex = index;
        HistoryList.SelectedItems.Clear();
        HistoryList.SelectedIndex = index;
        ScrollIntoViewAt(index);
    }

    // Selects the inclusive run of rows between two indices.
    private void SelectRange(int a, int b)
    {
        var lo = Math.Min(a, b);
        var hi = Math.Max(a, b);

        HistoryList.SelectedItems.Clear();
        for (var i = lo; i <= hi; i++)
            HistoryList.SelectedItems.Add(HistoryList.Items[i]);
    }

    private void ScrollIntoViewAt(int index)
    {
        if (index >= 0 && index < HistoryList.Items.Count)
            HistoryList.ScrollIntoView(HistoryList.Items[index]);
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
