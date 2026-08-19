using System.Windows;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Okno "Instrukcja obsługi" — statyczny opis funkcjonalności aplikacji.
/// </summary>
public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
