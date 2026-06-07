using System.Collections.ObjectModel;
using ClipStack.Models;

namespace ClipStack.Services;

/// <summary>
/// Keeps the most recently copied text items in memory, newest first.
///
/// The store enforces three rules that make the history feel natural to use:
///   1. The newest item is always at the top of the list.
///   2. Re-copying text that is already in the history moves it back to the
///      top instead of creating a duplicate.
///   3. Only the last <see cref="Capacity"/> items are kept; older ones drop off.
///
/// <see cref="Items"/> is an <see cref="ObservableCollection{T}"/> so the UI can
/// bind to it directly and update as clips come and go. All mutation happens on
/// the UI thread (the clipboard listener runs on the window's message loop), so
/// no extra synchronisation is needed.
/// </summary>
public sealed class ClipboardHistory
{
    private readonly ObservableCollection<ClipboardEntry> _items = new();

    public ClipboardHistory(int capacity = DefaultCapacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity),
                "History capacity must be at least 1.");

        Capacity = capacity;
        Items = new ReadOnlyObservableCollection<ClipboardEntry>(_items);
    }

    /// <summary>Number of items kept by default when no capacity is supplied.</summary>
    public const int DefaultCapacity = 50;

    /// <summary>Maximum number of items retained before the oldest is dropped.</summary>
    public int Capacity { get; }

    /// <summary>The history, newest first, exposed read-only for data binding.</summary>
    public ReadOnlyObservableCollection<ClipboardEntry> Items { get; }

    /// <summary>
    /// Records newly copied text. Blank text is ignored. If the same text is
    /// already in the history it is promoted to the top rather than duplicated.
    /// </summary>
    public void Add(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Promote an existing identical clip to the top instead of duplicating it.
        var existingIndex = IndexOf(text);
        if (existingIndex >= 0)
        {
            if (existingIndex != 0)
                _items.Move(existingIndex, 0);
            return;
        }

        _items.Insert(0, new ClipboardEntry(text, DateTime.Now));

        // Trim from the tail so we never exceed the configured capacity.
        while (_items.Count > Capacity)
            _items.RemoveAt(_items.Count - 1);
    }

    /// <summary>Removes every item from the history.</summary>
    public void Clear() => _items.Clear();

    private int IndexOf(string text)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i].Text, text, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }
}
