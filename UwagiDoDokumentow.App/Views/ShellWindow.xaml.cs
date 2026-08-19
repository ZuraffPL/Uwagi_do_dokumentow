using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UwagiDoDokumentow.App.ViewModels;
using UwagiDoDokumentow.Application.Interfaces;

namespace UwagiDoDokumentow.App.Views;

/// <summary>
/// Główne okno aplikacji po zalogowaniu — menu, pasek stanu i lista uwag jako treść domyślna.
/// </summary>
public partial class ShellWindow : Window
{
    private bool _isLoggingOut;

    public ShellWindow(ShellViewModel viewModel, NotesListView notesListView, IBackupService backupService, ICurrentUserService currentUser)
    {
        InitializeComponent();
        DataContext = viewModel;
        ContentHost.Content = notesListView;

        viewModel.OpenUsersAdminRequested += (_, _) => OpenChildWindow<Views.Admin.UsersAdminView>();
        viewModel.OpenDocumentTypesAdminRequested += (_, _) => OpenChildWindow<Views.Admin.DocumentTypesAdminView>();
        viewModel.OpenActivityLogRequested += (_, _) => OpenChildWindow<Views.Admin.ActivityLogView>();
        viewModel.ExportDatabaseRequested += (_, _) => ExportDatabase(backupService, currentUser);
        viewModel.ImportDatabaseRequested += (_, _) => ImportDatabase(backupService, currentUser);
        viewModel.OpenAboutRequested += (_, _) => OpenChildWindow<Views.AboutWindow>();
        viewModel.OpenHelpRequested += (_, _) => OpenChildWindow<Views.HelpWindow>();
        viewModel.LogoutRequested += (_, _) =>
        {
            _isLoggingOut = true;
            App.RestartToLogin();
            Close();
        };

        Closed += (_, _) =>
        {
            if (!_isLoggingOut)
            {
                System.Windows.Application.Current.Shutdown();
            }
        };
    }

    private void OpenChildWindow<TWindow>() where TWindow : Window
    {
        var window = App.Services.GetRequiredService<TWindow>();
        window.Owner = this;
        window.ShowDialog();
    }

    private async void ExportDatabase(IBackupService backupService, ICurrentUserService currentUser)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Eksportuj bazę danych",
            Filter = "Archiwum ZIP (*.zip)|*.zip",
            FileName = $"UwagiDoDokumentow_backup_{DateTime.Now:yyyyMMdd_HHmm}.zip"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            Cursor = Cursors.Wait;
            await backupService.ExportAsync(dialog.FileName, currentUser.Current!.Id);
            Cursor = Cursors.Arrow;
            MessageBox.Show(this, "Eksport zakończony pomyślnie.", "Uwagi do dokumentów", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Arrow;
            MessageBox.Show(this, $"Eksport nie powiódł się: {ex.Message}", "Uwagi do dokumentów", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ImportDatabase(IBackupService backupService, ICurrentUserService currentUser)
    {
        var warn = MessageBox.Show(this,
            "Import NADPISZE całą bazę danych i wszystkie załączniki. Upewnij się, że WSZYSCY " +
            "użytkownicy zamknęli aplikację przed kontynuowaniem — w przeciwnym razie dane mogą zostać uszkodzone.\n\n" +
            "Czy na pewno chcesz kontynuować?",
            "Uwagi do dokumentów — Import bazy danych", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (warn != MessageBoxResult.Yes)
        {
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Importuj bazę danych",
            Filter = "Archiwum ZIP (*.zip)|*.zip"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            Cursor = Cursors.Wait;
            await backupService.ImportAsync(dialog.FileName, currentUser.Current!.Id);
            MessageBox.Show(this,
                "Import zakończony pomyślnie. Aplikacja zostanie teraz zamknięta — uruchom ją ponownie.",
                "Uwagi do dokumentów", MessageBoxButton.OK, MessageBoxImage.Information);
            _isLoggingOut = true;
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Arrow;
            MessageBox.Show(this, $"Import nie powiódł się: {ex.Message}", "Uwagi do dokumentów", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
