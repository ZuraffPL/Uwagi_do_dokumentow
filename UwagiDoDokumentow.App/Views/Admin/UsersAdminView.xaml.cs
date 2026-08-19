using System.Windows;
using UwagiDoDokumentow.App.ViewModels;

namespace UwagiDoDokumentow.App.Views.Admin;

/// <summary>
/// Panel administracyjny użytkowników (tylko dla IsAdmin).
/// </summary>
public partial class UsersAdminView : Window
{
    private readonly UsersAdminViewModel _viewModel;

    public UsersAdminView(UsersAdminViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }

    private async void AddUserButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NewPassword = NewPasswordBox.Password;
        await _viewModel.AddUserCommand.ExecuteAsync(null);
        NewPasswordBox.Clear();
    }

    private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ResetPasswordCommand.ExecuteAsync(ResetPasswordBox.Password);
        ResetPasswordBox.Clear();
    }
}
