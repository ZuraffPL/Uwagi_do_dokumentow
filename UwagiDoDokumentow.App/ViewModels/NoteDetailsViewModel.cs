using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Infrastructure;

namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>
/// Pełny, tylko-do-odczytu widok szczegółów uwagi do dokumentu wraz z załącznikami.
/// </summary>
public partial class NoteDetailsViewModel : ObservableObject
{
    private readonly INotesService _notesService;
    private readonly IPrintService _printService;

    public NoteDetailsViewModel(INotesService notesService, IPrintService printService)
    {
        _notesService = notesService;
        _printService = printService;
    }

    public ObservableCollection<AttachmentDto> Attachments { get; } = new();

    [ObservableProperty]
    private int noteId;

    [ObservableProperty]
    private NoteDetailsDto? note;

    public async Task LoadAsync(int id)
    {
        NoteId = id;
        Note = await _notesService.GetDetailsAsync(id);
        Attachments.Clear();
        if (Note is not null)
        {
            foreach (var attachment in Note.Attachments)
            {
                Attachments.Add(attachment);
            }
        }
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        var pdfPath = await _printService.GenerateNotePdfAsync(NoteId);
        _printService.OpenPreview(pdfPath);
    }

    [RelayCommand]
    private void OpenAttachment(AttachmentDto? attachment)
    {
        if (attachment is null)
        {
            return;
        }

        var fullPath = Path.Combine(AppPaths.AttachmentsDirectory, attachment.RelativePath);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullPath) { UseShellExecute = true });
    }
}
