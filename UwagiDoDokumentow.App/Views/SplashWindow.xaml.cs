using System.Windows;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Ekran powitalny (splash screen) wyświetlany podczas inicjalizacji aplikacji
/// (migracje bazy danych, seed danych startowych, przygotowanie katalogów).
/// </summary>
public partial class SplashWindow : Window
{
    public SplashWindow(SplashViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
