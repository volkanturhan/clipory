using System.ComponentModel;

namespace clipory.Services;

public enum AppLanguage
{
    English,
    Turkish,
}

/// <summary>
/// The app's tiny translation table and current-language state.
///
/// UI elements bind to the string indexer (e.g. <c>[SearchPlaceholder]</c>)
/// against the shared <see cref="Instance"/>. When <see cref="Language"/>
/// changes we raise the special "Item[]" property change so every bound string
/// re-reads itself, giving a live language switch without rebuilding the UI.
/// Non-WPF consumers (the tray menu) can instead listen to
/// <see cref="LanguageChanged"/>.
/// </summary>
public sealed class Localization : INotifyPropertyChanged
{
    public static Localization Instance { get; } = new();

    private AppLanguage _language = AppLanguage.English;

    private static readonly Dictionary<string, string> English = new()
    {
        ["SearchPlaceholder"] = "Search clips…",
        ["EmptyState"] = "No clips to show",
        ["Hint"] = "↑↓ select · Enter paste · Del remove · right-click to pin · Esc close",
        ["MenuPin"] = "Pin / Unpin",
        ["MenuDelete"] = "Delete",
        ["TrayOpen"] = "Open clipory",
        ["TrayClear"] = "Clear history",
        ["TrayAutostart"] = "Start with Windows",
        ["TrayLanguage"] = "Language",
        ["TrayAbout"] = "About",
        ["TrayQuit"] = "Quit",
        ["AboutDescription"] = "A lightweight clipboard history manager.",
        ["AboutVersion"] = "Version",
        ["AboutClose"] = "Close",
    };

    private static readonly Dictionary<string, string> Turkish = new()
    {
        ["SearchPlaceholder"] = "Kopyalarda ara…",
        ["EmptyState"] = "Gösterilecek kopya yok",
        ["Hint"] = "↑↓ seç · Enter yapıştır · Del sil · sağ tık sabitle · Esc kapat",
        ["MenuPin"] = "Sabitle / Kaldır",
        ["MenuDelete"] = "Sil",
        ["TrayOpen"] = "clipory'yi Aç",
        ["TrayClear"] = "Geçmişi temizle",
        ["TrayAutostart"] = "Windows ile başlat",
        ["TrayLanguage"] = "Dil",
        ["TrayAbout"] = "Hakkında",
        ["TrayQuit"] = "Çıkış",
        ["AboutDescription"] = "Hafif bir pano geçmişi yöneticisi.",
        ["AboutVersion"] = "Sürüm",
        ["AboutClose"] = "Kapat",
    };

    /// <summary>The active language. Changing it refreshes all bound strings.</summary>
    public AppLanguage Language
    {
        get => _language;
        set
        {
            if (_language == value)
                return;

            _language = value;

            // "Item[]" tells WPF that every indexer binding should re-evaluate.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            LanguageChanged?.Invoke();
        }
    }

    /// <summary>The translation for <paramref name="key"/> in the current language.</summary>
    public string this[string key]
    {
        get
        {
            var table = _language == AppLanguage.Turkish ? Turkish : English;
            return table.TryGetValue(key, out var value) ? value : key;
        }
    }

    /// <summary>Raised after the language changes (for non-binding consumers).</summary>
    public event Action? LanguageChanged;

    public event PropertyChangedEventHandler? PropertyChanged;
}
