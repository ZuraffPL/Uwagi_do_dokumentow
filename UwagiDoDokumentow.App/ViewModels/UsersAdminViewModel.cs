using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Application.Interfaces;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Panel administracyjny użytkowników: dodawanie, zmiana uprawnień, dezaktywacja, reset hasła,
/// usuwanie kont nieaktywnych. Dostępny tylko dla użytkowników z flagą IsAdmin.
/// </summary>
public partial class UsersAdminViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    public UsersAdminViewModel(IUserService userService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    public ObservableCollection<UserDto> Users { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteUserCommand))]
    private UserDto? selectedUser;

    [ObservableProperty]
    private string newUsername = string.Empty;

    [ObservableProperty]
    private string newDisplayName = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public async Task InitializeAsync()
    {
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var users = await _userService.GetAllAsync();
        Users.Clear();
        foreach (var user in users)
        {
            Users.Add(user);
        }
    }

    [RelayCommand]
    private async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewDisplayName) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "Uzupełnij login, nazwę wyświetlaną i hasło.";
            return;
        }

        try
        {
            await _userService.CreateAsync(NewUsername.Trim(), NewDisplayName.Trim(), NewPassword, isAdmin: false, canAdd: true, canEdit: false, canDelete: false);
            NewUsername = string.Empty;
            NewDisplayName = string.Empty;
            NewPassword = string.Empty;
            ErrorMessage = string.Empty;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task SavePermissionsAsync()
    {
        if (SelectedUser is null)
        {
            return;
        }

        await _userService.UpdatePermissionsAsync(SelectedUser.Id, SelectedUser.IsAdmin, SelectedUser.CanAdd, SelectedUser.CanEdit, SelectedUser.CanDelete);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task ToggleActiveAsync()
    {
        if (SelectedUser is null)
        {
            return;
        }

        await _userService.SetActiveAsync(SelectedUser.Id, !SelectedUser.IsActive);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(string? newPasswordValue)
    {
        if (SelectedUser is null || string.IsNullOrWhiteSpace(newPasswordValue))
        {
            return;
        }

        await _userService.ResetPasswordAsync(SelectedUser.Id, newPasswordValue);
    }

    private bool CanDeleteUser() => SelectedUser is not null && !SelectedUser.IsActive;

    [RelayCommand(CanExecute = nameof(CanDeleteUser))]
    private async Task DeleteUserAsync()
    {
        if (SelectedUser is null)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Czy na pewno chcesz trwale usunąć użytkownika „{SelectedUser.Username}”? Tej operacji nie można cofnąć.",
            "Uwagi do dokumentów", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var performedBy = _currentUser.Current?.Id ?? 0;
            await _userService.DeleteAsync(SelectedUser.Id, performedBy);
            ErrorMessage = string.Empty;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
