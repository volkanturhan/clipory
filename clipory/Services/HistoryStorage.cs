using System.IO;
using System.Text.Json;
using clipory.Models;

namespace clipory.Services;

/// <summary>
/// Persists the clipboard history to disk so it survives restarts, storing it
/// as JSON under %APPDATA%\clipory. All operations are best-effort: a missing
/// or corrupt file simply yields an empty history, and a failed save is
/// swallowed rather than allowed to crash the app.
/// </summary>
public sealed class HistoryStorage
{
    // The shape we actually serialise — a plain, versionable record rather than
    // the live model type.
    private sealed record StoredEntry(string Text, DateTime CopiedAt, bool IsPinned);

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public HistoryStorage()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "clipory");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "history.json");
    }

    /// <summary>Loads the saved history, or an empty list if there is none.</summary>
    public IReadOnlyList<ClipboardEntry> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return Array.Empty<ClipboardEntry>();

            var stored = JsonSerializer.Deserialize<List<StoredEntry>>(File.ReadAllText(_filePath));
            if (stored is null)
                return Array.Empty<ClipboardEntry>();

            return stored
                .Select(s => new ClipboardEntry(s.Text, s.CopiedAt, s.IsPinned))
                .ToList();
        }
        catch
        {
            // Corrupt or unreadable file: start fresh rather than fail to launch.
            return Array.Empty<ClipboardEntry>();
        }
    }

    /// <summary>Writes the current history to disk.</summary>
    public void Save(IEnumerable<ClipboardEntry> entries)
    {
        try
        {
            var stored = entries
                .Select(e => new StoredEntry(e.Text, e.CopiedAt, e.IsPinned))
                .ToList();
            File.WriteAllText(_filePath, JsonSerializer.Serialize(stored, JsonOptions));
        }
        catch
        {
            // Best-effort persistence; losing a save is preferable to crashing.
        }
    }
}
