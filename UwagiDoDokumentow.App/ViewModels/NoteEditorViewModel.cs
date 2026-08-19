using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Infrastructure;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Formularz dodawania/edycji uwagi do dokumentu wraz z zarządzaniem załącznikami.
/// </summary>
public partial class NoteEditorViewModel : ObservableObject
{
    private readonly INotesService _notesService;
    private readonly IDocumentTypesService _documentTypesService;
    private readonly IAttachmentStorage _attachmentStorage;

    public NoteEditorViewModel(
        INotesService notesService,
        IDocumentTypesService documentTypesService,
        IAttachmentStorage attachmentStorage)
    {
        _notesService = notesService;
        _documentTypesService = documentTypesService;
        _attachmentStorage = attachmentStorage;
    }

    public ObservableCollection<DocumentType> DocumentTypes { get; } = new();
    public ObservableCollection<AttachmentDto> Attachments { get; } = new();

    [ObservableProperty]
    private int noteId;

    [ObservableProperty]
    private DateTime documentDate = DateTime.Today;

    [ObservableProperty]
    private string? documentSymbol;

    [ObservableProperty]
    private string documentNumber = string.Empty;

    [ObservableProperty]
    private string orderedBy = string.Empty;

    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private string content = string.Empty;

    [ObservableProperty]
    private string? tags;

    [ObservableProperty]
    private bool isArchived;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public bool IsNewNote => NoteId == 0;
    public string WindowTitle => IsNewNote ? "Nowa uwaga do dokumentu" : "Edycja uwagi do dokumentu";

    public event EventHandler? SavedSuccessfully;

    public async Task LoadAsync(int? noteIdToEdit)
    {
        var types = await _documentTypesService.GetAllAsync(onlyActive: true);
        DocumentTypes.Clear();
        foreach (var type in types)
        {
            DocumentTypes.Add(type);
        }

        if (noteIdToEdit is int id)
        {
            var note = await _notesService.GetForEditAsync(id)
                ?? throw new InvalidOperationException($"Nie znaleziono uwagi o id {id}.");

            NoteId = note.Id;
            DocumentDate = note.DocumentDate;
            DocumentSymbol = note.DocumentSymbol;
            DocumentNumber = note.DocumentNumber;
            OrderedBy = note.OrderedBy;
            Title = note.Title;
            Content = note.Content;
            Tags = note.Tags;
            IsArchived = note.IsArchived;

            await ReloadAttachmentsAsync();
        }
        else
        {
            NoteId = 0;
        }

        OnPropertyChanged(nameof(IsNewNote));
        OnPropertyChanged(nameof(WindowTitle));
    }

    private async Task ReloadAttachmentsAsync()
    {
        Attachments.Clear();
        var details = await _notesService.GetDetailsAsync(NoteId);
        if (details is not null)
        {
            foreach (var attachment in details.Attachments)
            {
                Attachments.Add(attachment);
            }
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(DocumentSymbol) || string.IsNullOrWhiteSpace(DocumentNumber) ||
            string.IsNullOrWhiteSpace(OrderedBy) || string.IsNullOrWhiteSpace(Content))
        {
            ErrorMessage = "Uzupełnij symbol, numer dokumentu, osobę zlecającą i treść uwagi.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await SaveNoteAsync();
            SavedSuccessfully?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się zapisać: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveNoteAsync()
    {
        var dto = new NoteEditDto
        {
            Id = NoteId,
            DocumentDate = DocumentDate,
            DocumentSymbol = DocumentSymbol!,
            DocumentNumber = DocumentNumber,
            OrderedBy = OrderedBy,
            Title = Title,
            Content = Content,
            Tags = Tags,
            IsArchived = IsArchived
        };

        if (IsNewNote)
        {
            NoteId = await _notesService.CreateAsync(dto);
            OnPropertyChanged(nameof(IsNewNote));
            OnPropertyChanged(nameof(WindowTitle));
        }
        else
        {
            await _notesService.UpdateAsync(dto);
        }
    }

    [RelayCommand]
    private async Task AddAttachmentAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // Załącznik wymaga zapisanej uwagi — jeśli to nowa, nie zapisana jeszcze uwaga, zapisujemy ją najpierw.
            if (IsNewNote)
            {
                if (string.IsNullOrWhiteSpace(DocumentSymbol) || string.IsNullOrWhiteSpace(DocumentNumber) ||
                    string.IsNullOrWhiteSpace(OrderedBy) || string.IsNullOrWhiteSpace(Content))
                {
                    ErrorMessage = "Uzupełnij i zapisz uwagę przed dodaniem załącznika.";
                    return;
                }
                await SaveNoteAsync();
            }

            await _notesService.AddAttachmentAsync(NoteId, filePath);
            await ReloadAttachmentsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się dodać załącznika: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RemoveAttachmentAsync(AttachmentDto? attachment)
    {
        if (attachment is null)
        {
            return;
        }

        await _notesService.RemoveAttachmentAsync(attachment.Id);
        await ReloadAttachmentsAsync();
    }

    [RelayCommand]
    private void OpenAttachment(AttachmentDto? attachment)
    {
        if (attachment is null)
        {
            return;
        }

        var fullPath = Path.Combine(AppPaths.AttachmentsDirectory, attachment.RelativePath);
        Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
    }
}
