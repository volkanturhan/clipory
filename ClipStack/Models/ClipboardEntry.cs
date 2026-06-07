using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClipStack.Models;

/// <summary>
/// A single item captured from the clipboard.
/// For now ClipStack only tracks text; richer formats (images, files) can be
/// added later without changing how the history store works.
/// </summary>
public sealed class ClipboardEntry : INotifyPropertyChanged
{
    private bool _isPinned;

    public ClipboardEntry(string text, DateTime copiedAt, bool isPinned = false)
    {
        Text = text;
        CopiedAt = copiedAt;
        _isPinned = isPinned;
    }

    /// <summary>The full text that was copied.</summary>
    public string Text { get; }

    /// <summary>When this item was captured.</summary>
    public DateTime CopiedAt { get; }

    /// <summary>
    /// Whether the user has pinned this clip. Pinned clips stay at the top of
    /// the history and are never dropped when the list fills up.
    /// </summary>
    public bool IsPinned
    {
        get => _isPinned;
        set
        {
            if (_isPinned == value)
                return;

            _isPinned = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// A short, single-line version of <see cref="Text"/> for showing in the
    /// history list. Newlines are collapsed and long text is trimmed so every
    /// row stays on one line.
    /// </summary>
    public string Preview
    {
        get
        {
            // Collapse any run of whitespace (including newlines/tabs) into a
            // single space so multi-line clips render as one tidy row.
            var collapsed = string.Join(' ', Text.Split(
                (char[]?)null, StringSplitOptions.RemoveEmptyEntries));

            const int maxLength = 80;
            return collapsed.Length <= maxLength
                ? collapsed
                : collapsed[..maxLength] + "…";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
