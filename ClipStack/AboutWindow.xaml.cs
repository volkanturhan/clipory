using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using ClipStack.Services;

// Disambiguate from System.Windows.Localization (pulled in via System.Windows).
using Localization = ClipStack.Services.Localization;

namespace ClipStack;

/// <summary>
/// A small "About" dialog: icon, name, version, author, project link and
/// licence. Its localized strings follow the app language.
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"{Localization.Instance["AboutVersion"]} {version?.ToString(3)}";
    }

    private void OnNavigate(object sender, RequestNavigateEventArgs e)
    {
        // Open the link in the user's default browser.
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
