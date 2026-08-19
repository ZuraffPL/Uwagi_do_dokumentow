using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Domain.ValueObjects;

namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Serwis aplikacyjny obsługujący CRUD uwag do dokumentów oraz wyszukiwanie/filtrowanie.
/// ViewModele korzystają wyłącznie z tego interfejsu.
/// </summary>
public interface INotesService
{
    Task<List<NoteListItemDto>> SearchAsync(NoteSearchFilter filter, CancellationToken ct = default);
    Task<NoteDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<NoteEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(NoteEditDto note, CancellationToken ct = default);
    Task UpdateAsync(NoteEditDto note, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task SetArchivedAsync(int id, bool isArchived, CancellationToken ct = default);

    Task<AttachmentDto> AddAttachmentAsync(int noteId, string sourceFilePath, CancellationToken ct = default);
    Task RemoveAttachmentAsync(int attachmentId, CancellationToken ct = default);
}
