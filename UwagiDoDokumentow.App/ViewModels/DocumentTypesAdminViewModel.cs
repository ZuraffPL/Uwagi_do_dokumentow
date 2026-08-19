using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Panel administracyjny słownika symboli dokumentów (document_types).
/// </summary>
public partial class DocumentTypesAdminViewModel : ObservableObject
{
    private readonly IDocumentTypesService _documentTypesService;

    public DocumentTypesAdminViewModel(IDocumentTypesService documentTypesService)
    {
        _documentTypesService = documentTypesService;
    }

    public ObservableCollection<DocumentType> DocumentTypes { get; } = new();

    [ObservableProperty]
    private DocumentType? selectedType;

    [ObservableProperty]
    private string newSymbol = string.Empty;

    [ObservableProperty]
    private string? newDescription;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public async Task InitializeAsync() => await ReloadAsync();

    private async Task ReloadAsync()
    {
        var types = await _documentTypesService.GetAllAsync();
        DocumentTypes.Clear();
        foreach (var type in types)
        {
            DocumentTypes.Add(type);
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSymbol))
        {
            ErrorMessage = "Podaj symbol dokumentu.";
            return;
        }

        try
        {
            await _documentTypesService.AddAsync(NewSymbol.Trim().ToUpperInvariant(), NewDescription);
            NewSymbol = string.Empty;
            NewDescription = string.Empty;
            ErrorMessage = string.Empty;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync()
    {
        if (SelectedType is null)
        {
            return;
        }

        await _documentTypesService.SetActiveAsync(SelectedType.Symbol, !SelectedType.IsActive);
        await ReloadAsync();
    }
}
