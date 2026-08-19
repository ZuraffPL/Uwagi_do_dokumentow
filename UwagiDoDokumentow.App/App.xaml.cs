using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using UwagiDoDokumentow.App.Services;
using UwagiDoDokumentow.App.ViewModels;
using UwagiDoDokumentow.App.Views;
using UwagiDoDokumentow.App.Views.Admin;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Infrastructure;
using UwagiDoDokumentow.Infrastructure.Logging;
using UwagiDoDokumentow.Infrastructure.Persistence;
using UwagiDoDokumentow.Infrastructure.Printing;
using UwagiDoDokumentow.Infrastructure.Security;
using UwagiDoDokumentow.Infrastructure.Services;
using UwagiDoDokumentow.Infrastructure.Storage;

namespace UwagiDoDokumentow.App;

/// <summary>
/// Punkt startowy aplikacji: konfiguruje DI (Generic Host), pokazuje splash screen podczas
/// inicjalizacji (migracje EF Core, tryb WAL, seed danych startowych), a następnie
/// prowadzi przez ekran logowania do głównego okna aplikacji.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        QuestPDF.Settings.License = LicenseType.Community;

        _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => ConfigureServices(services))
            .Build();

        await _host.StartAsync();
        Services = _host.Services;

        var splashViewModel = Services.GetRequiredService<SplashViewModel>();
        var appVersion = GetType().Assembly.GetName().Version;
        splashViewModel.VersionText = appVersion is null ? "Wersja 1.0.0" : $"Wersja {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
        var splash = new SplashWindow(splashViewModel);
        splash.Show();

        try
        {
            await InitializeApplicationDataAsync(splashViewModel);
        }
        catch (Exception ex)
        {
            Services.GetRequiredService<ILoggingService>().LogError("Błąd inicjalizacji aplikacji.", ex);
            MessageBox.Show(
                $"Nie udało się uruchomić aplikacji: {ex.Message}",
                "Uwagi do dokumentów", MessageBoxButton.OK, MessageBoxImage.Error);
            splash.Close();
            Shutdown(-1);
            return;
        }

        splash.Close();
        ShowLoginFlow();
    }

    private static async Task InitializeApplicationDataAsync(SplashViewModel status)
    {
        status.StatusText = "Przygotowywanie katalogów danych…";
        AppPaths.EnsureDirectoriesExist();

        status.StatusText = "Aktualizowanie bazy danych…";
        var dbContext = Services.GetRequiredService<NotesDbContext>();
        await dbContext.Database.MigrateAsync();
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");

        status.StatusText = "Sprawdzanie konta administratora…";
        if (!await dbContext.Users.AnyAsync())
        {
            dbContext.Users.Add(new User
            {
                Username = "admin",
                DisplayName = "Administrator",
                PasswordHash = PasswordHasher.Hash("admin123"),
                IsAdmin = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await dbContext.SaveChangesAsync();

            MessageBox.Show(
                "Utworzono domyślne konto administratora:\nLogin: admin\nHasło: admin123\n\n" +
                "Zaloguj się i jak najszybciej zmień hasło w panelu administracyjnym.",
                "Uwagi do dokumentów — pierwsze uruchomienie",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        status.StatusText = "Gotowe.";
    }

    private void ShowLoginFlow()
    {
        var login = Services.GetRequiredService<LoginWindow>();
        var result = login.ShowDialog();

        if (result != true)
        {
            Shutdown();
            return;
        }

        var shell = Services.GetRequiredService<ShellWindow>();
        MainWindow = shell;
        shell.Show();
    }

    /// <summary>Wywoływane po wylogowaniu — ponownie pokazuje ekran logowania.</summary>
    public static void RestartToLogin() => ((App)Current).ShowLoginFlow();

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var connectionString = $"Data Source={AppPaths.DatabaseFilePath}";
        services.AddDbContext<NotesDbContext>(
            options => options.UseSqlite(connectionString),
            ServiceLifetime.Singleton);

        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IAttachmentStorage, LocalAttachmentStorage>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IActivityLogService, ActivityLogService>();
        services.AddSingleton<IActivityLogReaderService, ActivityLogReaderService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IDocumentTypesService, DocumentTypesService>();
        services.AddSingleton<INotesService, NotesService>();
        services.AddSingleton<IPrintService, QuestPdfNoteRenderer>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<UiDispatcher>();

        services.AddTransient<SplashViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<NotesListViewModel>();
        services.AddTransient<NoteEditorViewModel>();
        services.AddTransient<NoteDetailsViewModel>();
        services.AddTransient<NoteHistoryViewModel>();
        services.AddTransient<UsersAdminViewModel>();
        services.AddTransient<DocumentTypesAdminViewModel>();
        services.AddTransient<ActivityLogViewModel>();
        services.AddTransient<AboutViewModel>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<ShellWindow>();
        services.AddTransient<NotesListView>();
        services.AddTransient<NoteEditorWindow>();
        services.AddTransient<NoteDetailsWindow>();
        services.AddTransient<NoteHistoryWindow>();
        services.AddTransient<UsersAdminView>();
        services.AddTransient<DocumentTypesAdminView>();
        services.AddTransient<ActivityLogView>();
        services.AddTransient<AboutWindow>();
        services.AddTransient<HelpWindow>();
    }
}

