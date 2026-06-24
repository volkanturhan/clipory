using System.IO;
using System.Text.Json;

namespace clipory.Services;

/// <summary>
/// Persists small user preferences (language and colour theme) as JSON under
/// %APPDATA%\clipory. Best-effort, like <see cref="HistoryStorage"/>: failures
/// fall back to defaults rather than throwing.
/// </summary>
public sealed class SettingsStore
{
    // Theme is nullable so older settings files (which only had a language) still
    // load; a missing value just falls back to the default.
    private sealed record Data(string Language, string? Theme = null);

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public SettingsStore()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "clipory");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");
    }

    /// <summary>Loads the saved language, defaulting to English.</summary>
    public AppLanguage LoadLanguage()
    {
        var data = Read();
        return data is not null && Enum.TryParse<AppLanguage>(data.Language, out var language)
            ? language
            : AppLanguage.English;
    }

    /// <summary>Loads the saved theme, defaulting to System.</summary>
    public AppTheme LoadTheme()
    {
        var data = Read();
        return data?.Theme is not null && Enum.TryParse<AppTheme>(data.Theme, out var theme)
            ? theme
            : AppTheme.System;
    }

    /// <summary>Saves the chosen language, preserving the stored theme.</summary>
    public void SaveLanguage(AppLanguage language)
    {
        var current = Read();
        Write(new Data(language.ToString(), current?.Theme));
    }

    /// <summary>Saves the chosen theme, preserving the stored language.</summary>
    public void SaveTheme(AppTheme theme)
    {
        var current = Read();
        Write(new Data(current?.Language ?? AppLanguage.English.ToString(), theme.ToString()));
    }

    // Reads and parses the settings file, or null if it is missing/unreadable.
    private Data? Read()
    {
        try
        {
            return File.Exists(_filePath)
                ? JsonSerializer.Deserialize<Data>(File.ReadAllText(_filePath))
                : null;
        }
        catch
        {
            return null;
        }
    }

    private void Write(Data data)
    {
        try
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(data, JsonOptions));
        }
        catch
        {
            // Best-effort; a lost preference is not worth crashing over.
        }
    }
}
