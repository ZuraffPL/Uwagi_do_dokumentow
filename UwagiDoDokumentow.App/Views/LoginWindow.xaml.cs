using System.Windows;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Ekran logowania. Brak zalogowania = brak dostępu do danych aplikacji.
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.LoginSucceeded += (_, _) =>
        {
            DialogResult = true;
            Close();
        };
    }
}
