namespace ClipStack.Models;

/// <summary>
/// A single item captured from the clipboard.
/// For now ClipStack only tracks text; richer formats (images, files) can be
/// added later without changing how the history store works.
/// </summary>
public sealed class ClipboardEntry
{
    public ClipboardEntry(string text, DateTime copiedAt)
    {
        Text = text;
        CopiedAt = copiedAt;
    }

    /// <summary>The full text that was copied.</summary>
    public string Text { get; }

    /// <summary>When this item was captured.</summary>
    public DateTime CopiedAt { get; }

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
}
