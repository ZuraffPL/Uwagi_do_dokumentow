using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UwagiDoDokumentow.Application.Interfaces;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Główne okno aplikacji — pasek menu z dostępem do modułów, w zależności od uprawnień
/// bieżącego użytkownika (ICurrentUserService), oraz obszar treści (domyślnie lista uwag).
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly ICurrentUserService _currentUser;

    public ShellViewModel(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public string CurrentUserDisplayName => _currentUser.Current?.DisplayName ?? string.Empty;
    public bool IsAdmin => _currentUser.Current?.IsAdmin ?? false;

    public string WindowTitle
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionText = version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
            return $"Uwagi do dokumentów — wersja {versionText}";
        }
    }

    public event EventHandler? OpenUsersAdminRequested;
    public event EventHandler? OpenDocumentTypesAdminRequested;
    public event EventHandler? OpenActivityLogRequested;
    public event EventHandler? ExportDatabaseRequested;
    public event EventHandler? ImportDatabaseRequested;
    public event EventHandler? OpenAboutRequested;
    public event EventHandler? OpenHelpRequested;
    public event EventHandler? LogoutRequested;

    [RelayCommand]
    private void OpenUsersAdmin() => OpenUsersAdminRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenDocumentTypesAdmin() => OpenDocumentTypesAdminRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenActivityLog() => OpenActivityLogRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ExportDatabase() => ExportDatabaseRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ImportDatabase() => ImportDatabaseRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenAbout() => OpenAboutRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenHelp() => OpenHelpRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Logout()
    {
        _currentUser.SignOut();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
