using CommunityToolkit.Mvvm.ComponentModel;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Stan ekranu powitalnego (splash screen) wyświetlanego podczas inicjalizacji aplikacji.
/// </summary>
public partial class SplashViewModel : ObservableObject
{
    [ObservableProperty]
    private string statusText = "Uruchamianie…";

    [ObservableProperty]
    private string versionText = string.Empty;

    [ObservableProperty]
    private string authorText = "Autor: Marcin Żurawicz";

    [ObservableProperty]
    private string techStackText = "C# (.NET 10) • WPF • EF Core • SQLite • QuestPDF";
}
