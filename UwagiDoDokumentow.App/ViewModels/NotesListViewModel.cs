using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Domain.ValueObjects;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Lista uwag do dokumentów z filtrowaniem/wyszukiwaniem. Widoczność akcji
/// Dodaj/Edytuj/Usuń zależy od uprawnień bieżącego użytkownika.
/// </summary>
public partial class NotesListViewModel : ObservableObject
{
    private readonly INotesService _notesService;
    private readonly IDocumentTypesService _documentTypesService;
    private readonly IPrintService _printService;
    private readonly ICurrentUserService _currentUser;

    public NotesListViewModel(
        INotesService notesService,
        IDocumentTypesService documentTypesService,
        IPrintService printService,
        ICurrentUserService currentUser)
    {
        _notesService = notesService;
        _documentTypesService = documentTypesService;
        _printService = printService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Pseudo-symbol reprezentujący brak filtra ("(Wszystkie)") — dzięki temu użytkownik
    /// może w ComboBoxie wrócić ze stanu "wybrany konkretny symbol" do stanu neutralnego.
    /// </summary>
    public const string AllSymbolsFilterValue = "";

    public ObservableCollection<NoteListItemDto> Notes { get; } = new();
    public ObservableCollection<DocumentType> DocumentTypes { get; } = new();

    [ObservableProperty]
    private string? phraseFilter;

    [ObservableProperty]
    private string? symbolFilter = AllSymbolsFilterValue;

    [ObservableProperty]
    private DateTime? dateFromFilter;

    [ObservableProperty]
    private DateTime? dateToFilter;

    [ObservableProperty]
    private bool onlyWithAttachmentsFilter;

    [ObservableProperty]
    private bool showArchivedFilter;

    [ObservableProperty]
    private NoteListItemDto? selectedNote;

    [ObservableProperty]
    private bool isBusy;

    public bool CanAdd => _currentUser.Current?.CanAdd ?? false;
    public bool CanEdit => _currentUser.Current?.CanEdit ?? false;
    public bool CanDelete => _currentUser.Current?.CanDelete ?? false;

    public event EventHandler? AddRequested;
    public event EventHandler<int>? EditRequested;
    public event EventHandler<int>? DetailsRequested;
    public event EventHandler<int>? HistoryRequested;

    public async Task InitializeAsync()
    {
        var types = await _documentTypesService.GetAllAsync(onlyActive: true);
        DocumentTypes.Clear();
        DocumentTypes.Add(new DocumentType { Symbol = AllSymbolsFilterValue, Description = "(Wszystkie)" });
        foreach (var type in types)
        {
            DocumentTypes.Add(type);
        }

        await SearchAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsBusy = true;
        try
        {
            var filter = new NoteSearchFilter
            {
                Phrase = string.IsNullOrWhiteSpace(PhraseFilter) ? null : PhraseFilter,
                DocumentSymbol = string.IsNullOrWhiteSpace(SymbolFilter) ? null : SymbolFilter,
                DocumentDateFrom = DateFromFilter,
                DocumentDateTo = DateToFilter,
                OnlyWithAttachments = OnlyWithAttachmentsFilter ? true : null,
                IsArchived = ShowArchivedFilter ? null : false
            };

            var results = await _notesService.SearchAsync(filter);
            Notes.Clear();
            foreach (var note in results)
            {
                Notes.Add(note);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Add() => AddRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit()
    {
        if (SelectedNote is not null)
        {
            EditRequested?.Invoke(this, SelectedNote.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void ShowDetails()
    {
        if (SelectedNote is not null)
        {
            DetailsRequested?.Invoke(this, SelectedNote.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteAsync()
    {
        if (SelectedNote is null)
        {
            return;
        }

        await _notesService.DeleteAsync(SelectedNote.Id);
        await SearchAsync();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task PrintAsync()
    {
        if (SelectedNote is null)
        {
            return;
        }

        var pdfPath = await _printService.GenerateNotePdfAsync(SelectedNote.Id);
        _printService.OpenPreview(pdfPath);
    }

    [RelayCommand(CanExecute = nameof(CanToggleArchive))]
    private async Task ToggleArchiveAsync()
    {
        if (SelectedNote is null)
        {
            return;
        }

        await _notesService.SetArchivedAsync(SelectedNote.Id, !SelectedNote.IsArchived);
        await SearchAsync();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void ShowHistory()
    {
        if (SelectedNote is not null)
        {
            HistoryRequested?.Invoke(this, SelectedNote.Id);
        }
    }

    private bool HasSelection => SelectedNote is not null;
    private bool CanToggleArchive => SelectedNote is not null && CanEdit;

    partial void OnSelectedNoteChanged(NoteListItemDto? value)
    {
        EditCommand.NotifyCanExecuteChanged();
        ShowDetailsCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        PrintCommand.NotifyCanExecuteChanged();
        ToggleArchiveCommand.NotifyCanExecuteChanged();
        ShowHistoryCommand.NotifyCanExecuteChanged();
    }
}
