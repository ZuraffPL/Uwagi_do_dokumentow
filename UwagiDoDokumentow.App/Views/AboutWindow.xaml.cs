using System.Windows;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Okno "O programie" — wyświetla wersję aplikacji, autora i stos technologiczny.
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow(AboutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        var appVersion = GetType().Assembly.GetName().Version;
        viewModel.VersionText = appVersion is null
            ? "Wersja 1.0.0"
            : $"Wersja {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) => Close();
}
