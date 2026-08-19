using CommunityToolkit.Mvvm.ComponentModel;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Stan okna "O programie" — wersja aplikacji, autor i stos technologiczny.
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    private string versionText = string.Empty;

    [ObservableProperty]
    private string authorText = "Autor: Marcin Żurawicz";

    [ObservableProperty]
    private string techStackText = "C# (.NET 10) • WPF • EF Core • SQLite • QuestPDF";
}
