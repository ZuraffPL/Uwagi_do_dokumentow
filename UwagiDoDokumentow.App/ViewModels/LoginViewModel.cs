using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UwagiDoDokumentow.Application.Interfaces;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Logowanie użytkownika. Brak zalogowania = brak dostępu do danych.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public LoginViewModel(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public event EventHandler? LoginSucceeded;

    [RelayCommand]
    private async Task LoginAsync(object? passwordBoxParameter)
    {
        // PasswordBox.Password celowo nie jest bindowalne przez WPF (względy bezpieczeństwa),
        // dlatego kontrolka jest przekazywana jako CommandParameter zamiast trzymać hasło w polu ViewModelu.
        var password = (passwordBoxParameter as PasswordBox)?.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Podaj nazwę użytkownika i hasło.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var user = await _userService.AuthenticateAsync(Username.Trim(), password);
            if (user is null)
            {
                ErrorMessage = "Nieprawidłowa nazwa użytkownika lub hasło.";
                return;
            }

            _currentUserService.SignIn(user);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
