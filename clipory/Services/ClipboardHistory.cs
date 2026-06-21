using System.Collections.ObjectModel;
using clipory.Models;

namespace clipory.Services;

/// <summary>
/// Keeps the most recently copied text items in memory. Ordering is "pinned
/// first, then most recent": pinned clips sit at the top and are kept forever,
/// while the unpinned clips below them are newest-first and capped to
/// <see cref="Capacity"/> (older ones drop off).
///
/// Re-copying text that is already in the history moves it to the top of its
/// section instead of creating a duplicate.
///
/// <see cref="Items"/> is observable so the UI can bind to it directly. The
/// <see cref="Changed"/> event fires after any mutation so callers (e.g. the
/// persistence layer) can react. All mutation happens on the UI thread.
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

    /// <summary>Number of unpinned items kept by default.</summary>
    public const int DefaultCapacity = 50;

    /// <summary>Maximum number of unpinned items retained before the oldest drops off.</summary>
    public int Capacity { get; }

    /// <summary>The history, pinned-first then newest-first, exposed read-only for binding.</summary>
    public ReadOnlyObservableCollection<ClipboardEntry> Items { get; }

    /// <summary>Raised after any change to the history (add, pin, remove, clear).</summary>
    public event Action? Changed;

    /// <summary>Replaces the current history with the given entries (used on load).</summary>
    public void Initialize(IEnumerable<ClipboardEntry> entries)
    {
        _items.Clear();
        foreach (var entry in entries)
            _items.Add(entry);

        PinnedFirst();
        TrimUnpinned();
        Changed?.Invoke();
    }

    /// <summary>
    /// Records newly copied text. Blank text is ignored. If the same text is
    /// already in the history it is promoted to the top of its section rather
    /// than duplicated.
    /// </summary>
    public void Add(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var existingIndex = IndexOf(text);
        if (existingIndex >= 0)
        {
            // Promote to the top of its own section (pinned items to index 0,
            // unpinned items to just below the last pinned one).
            var target = _items[existingIndex].IsPinned ? 0 : PinnedCount;
            if (existingIndex != target)
                _items.Move(existingIndex, target);
            Changed?.Invoke();
            return;
        }

        // New clips go to the top of the unpinned section.
        _items.Insert(PinnedCount, new ClipboardEntry(text, DateTime.Now));
        TrimUnpinned();
        Changed?.Invoke();
    }

    /// <summary>Pins or unpins an entry and moves it to the top of its new section.</summary>
    public void TogglePin(ClipboardEntry entry)
    {
        var index = _items.IndexOf(entry);
        if (index < 0)
            return;

        entry.IsPinned = !entry.IsPinned;

        // After the flag flips, PinnedCount reflects the new section sizes, so
        // index 0 is the top of the pinned section and PinnedCount is the top
        // of the unpinned section.
        var target = entry.IsPinned ? 0 : PinnedCount;
        if (index != target)
            _items.Move(index, target);

        TrimUnpinned();
        Changed?.Invoke();
    }

    /// <summary>Removes a single entry from the history.</summary>
    public void Remove(ClipboardEntry entry)
    {
        if (_items.Remove(entry))
            Changed?.Invoke();
    }

    /// <summary>Clears the unpinned history, keeping pinned clips.</summary>
    public void ClearUnpinned()
    {
        for (var i = _items.Count - 1; i >= 0; i--)
        {
            if (!_items[i].IsPinned)
                _items.RemoveAt(i);
        }

        Changed?.Invoke();
    }

    private int PinnedCount
    {
        get
        {
            var count = 0;
            foreach (var item in _items)
            {
                if (item.IsPinned)
                    count++;
            }

            return count;
        }
    }

    private int IndexOf(string text)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i].Text, text, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    // Stable partition so every pinned item precedes every unpinned one while
    // each group keeps its existing relative order.
    private void PinnedFirst()
    {
        var insertAt = 0;
        for (var i = 0; i < _items.Count; i++)
        {
            if (_items[i].IsPinned)
            {
                if (i != insertAt)
                    _items.Move(i, insertAt);
                insertAt++;
            }
        }
    }

    // Keep only the newest Capacity unpinned items; drop older unpinned ones.
    private void TrimUnpinned()
    {
        var unpinnedSeen = 0;
        var i = 0;
        while (i < _items.Count)
        {
            if (!_items[i].IsPinned && ++unpinnedSeen > Capacity)
            {
                _items.RemoveAt(i);
                continue;
            }

            i++;
        }
    }
}
