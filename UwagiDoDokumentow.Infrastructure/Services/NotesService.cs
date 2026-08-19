using Microsoft.EntityFrameworkCore;
using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Domain.ValueObjects;
using UwagiDoDokumentow.Infrastructure.Persistence;

namespace UwagiDoDokumentow.Infrastructure.Services;

/// <summary>
/// Implementacja CRUD i wyszukiwania uwag do dokumentów. Każda operacja modyfikująca
/// zapisuje wpis w activity_log.
/// </summary>
public class NotesService : INotesService
{
    private readonly NotesDbContext _dbContext;
    private readonly IAttachmentStorage _attachmentStorage;
    private readonly ICurrentUserService _currentUser;
    private readonly IActivityLogService _activityLog;

    public NotesService(
        NotesDbContext dbContext,
        IAttachmentStorage attachmentStorage,
        ICurrentUserService currentUser,
        IActivityLogService activityLog)
    {
        _dbContext = dbContext;
        _attachmentStorage = attachmentStorage;
        _currentUser = currentUser;
        _activityLog = activityLog;
    }

    public async Task<List<NoteListItemDto>> SearchAsync(NoteSearchFilter filter, CancellationToken ct = default)
    {
        var query = _dbContext.DocumentNotes
            .Include(n => n.Attachments)
            .Include(n => n.CreatedByUser)
            .AsQueryable();

        if (filter.Id is not null)
        {
            query = query.Where(n => n.Id == filter.Id);
        }
        if (!string.IsNullOrWhiteSpace(filter.DocumentSymbol))
        {
            query = query.Where(n => n.DocumentSymbol == filter.DocumentSymbol);
        }
        if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
        {
            query = query.Where(n => n.DocumentNumber.Contains(filter.DocumentNumber));
        }
        if (filter.DocumentDateFrom is not null)
        {
            query = query.Where(n => n.DocumentDate >= filter.DocumentDateFrom);
        }
        if (filter.DocumentDateTo is not null)
        {
            query = query.Where(n => n.DocumentDate <= filter.DocumentDateTo);
        }
        if (filter.CreatedByUserId is not null)
        {
            query = query.Where(n => n.CreatedByUserId == filter.CreatedByUserId);
        }
        if (!string.IsNullOrWhiteSpace(filter.OrderedBy))
        {
            query = query.Where(n => n.OrderedBy.Contains(filter.OrderedBy));
        }
        if (!string.IsNullOrWhiteSpace(filter.Phrase))
        {
            query = query.Where(n => n.Title!.Contains(filter.Phrase) || n.Content.Contains(filter.Phrase));
        }
        if (filter.OnlyWithAttachments == true)
        {
            query = query.Where(n => n.Attachments.Count > 0);
        }
        if (filter.IsArchived is not null)
        {
            query = query.Where(n => n.IsArchived == filter.IsArchived);
        }

        var notes = await query
            .OrderByDescending(n => n.UpdatedAt)
            .Select(n => new NoteListItemDto
            {
                Id = n.Id,
                DocumentDate = n.DocumentDate,
                DocumentSymbol = n.DocumentSymbol,
                DocumentNumber = n.DocumentNumber,
                OrderedBy = n.OrderedBy,
                Title = n.Title,
                Tags = n.Tags,
                IsArchived = n.IsArchived,
                WasModified = n.UpdatedAt != n.CreatedAt,
                AttachmentsCount = n.Attachments.Count,
                CreatedByDisplayName = n.CreatedByUser!.DisplayName,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync(ct);

        return notes;
    }

    public async Task<NoteDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var note = await _dbContext.DocumentNotes
            .Include(n => n.Attachments).ThenInclude(a => a.UploadedByUser)
            .Include(n => n.CreatedByUser)
            .Include(n => n.UpdatedByUser)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

        if (note is null)
        {
            return null;
        }

        return new NoteDetailsDto
        {
            Id = note.Id,
            DocumentDate = note.DocumentDate,
            DocumentSymbol = note.DocumentSymbol,
            DocumentNumber = note.DocumentNumber,
            OrderedBy = note.OrderedBy,
            Title = note.Title,
            Content = note.Content,
            Tags = note.Tags,
            IsArchived = note.IsArchived,
            CreatedByDisplayName = note.CreatedByUser?.DisplayName ?? string.Empty,
            UpdatedByDisplayName = note.UpdatedByUser?.DisplayName ?? string.Empty,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            Attachments = note.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                NoteId = a.NoteId,
                OriginalFileName = a.OriginalFileName,
                StoredFileName = a.StoredFileName,
                RelativePath = a.RelativePath,
                ContentType = a.ContentType,
                Extension = a.Extension,
                SizeBytes = a.SizeBytes,
                UploadedByDisplayName = a.UploadedByUser?.DisplayName ?? string.Empty,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    public async Task<NoteEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var note = await _dbContext.DocumentNotes.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (note is null)
        {
            return null;
        }

        return new NoteEditDto
        {
            Id = note.Id,
            DocumentDate = note.DocumentDate,
            DocumentSymbol = note.DocumentSymbol,
            DocumentNumber = note.DocumentNumber,
            OrderedBy = note.OrderedBy,
            Title = note.Title,
            Content = note.Content,
            Tags = note.Tags,
            IsArchived = note.IsArchived
        };
    }

    public async Task<int> CreateAsync(NoteEditDto note, CancellationToken ct = default)
    {
        var userId = RequireCurrentUserId();
        var now = DateTime.Now;

        var entity = new DocumentNote
        {
            DocumentDate = note.DocumentDate,
            DocumentSymbol = note.DocumentSymbol,
            DocumentNumber = note.DocumentNumber,
            OrderedBy = note.OrderedBy,
            Title = note.Title,
            Content = note.Content,
            Tags = note.Tags,
            IsArchived = note.IsArchived,
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.DocumentNotes.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        await _activityLog.LogAsync(userId, Domain.Enums.ActivityActionType.Create, nameof(DocumentNote), entity.Id, ct: ct);

        return entity.Id;
    }

    public async Task UpdateAsync(NoteEditDto note, CancellationToken ct = default)
    {
        var userId = RequireCurrentUserId();

        var entity = await _dbContext.DocumentNotes.FirstOrDefaultAsync(n => n.Id == note.Id, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono uwagi o id {note.Id}.");

        var changedFields = new List<string>();
        if (entity.DocumentDate != note.DocumentDate) changedFields.Add("datę dokumentu");
        if (entity.DocumentSymbol != note.DocumentSymbol) changedFields.Add("symbol");
        if (entity.DocumentNumber != note.DocumentNumber) changedFields.Add("numer dokumentu");
        if (entity.OrderedBy != note.OrderedBy) changedFields.Add("kto zlecił");
        if (entity.Title != note.Title) changedFields.Add("tytuł");
        if (entity.Content != note.Content) changedFields.Add("treść");
        if (entity.Tags != note.Tags) changedFields.Add("tagi");
        if (entity.IsArchived != note.IsArchived) changedFields.Add("status archiwizacji");

        entity.DocumentDate = note.DocumentDate;
        entity.DocumentSymbol = note.DocumentSymbol;
        entity.DocumentNumber = note.DocumentNumber;
        entity.OrderedBy = note.OrderedBy;
        entity.Title = note.Title;
        entity.Content = note.Content;
        entity.Tags = note.Tags;
        entity.IsArchived = note.IsArchived;
        entity.UpdatedByUserId = userId;
        entity.UpdatedAt = DateTime.Now;

        await _dbContext.SaveChangesAsync(ct);

        var details = changedFields.Count > 0
            ? "Zmieniono: " + string.Join(", ", changedFields)
            : "Zapisano bez zmian";

        await _activityLog.LogAsync(userId, Domain.Enums.ActivityActionType.Update, nameof(DocumentNote), entity.Id, details: details, ct: ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var userId = RequireCurrentUserId();

        var entity = await _dbContext.DocumentNotes
            .Include(n => n.Attachments)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

        if (entity is null)
        {
            return;
        }

        foreach (var attachment in entity.Attachments)
        {
            _attachmentStorage.Delete(attachment);
        }

        _dbContext.DocumentNotes.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);

        await _activityLog.LogAsync(userId, Domain.Enums.ActivityActionType.Delete, nameof(DocumentNote), id, ct: ct);
    }

    public async Task SetArchivedAsync(int id, bool isArchived, CancellationToken ct = default)
    {
        var userId = RequireCurrentUserId();

        var entity = await _dbContext.DocumentNotes.FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono uwagi o id {id}.");

        entity.IsArchived = isArchived;
        entity.UpdatedByUserId = userId;
        entity.UpdatedAt = DateTime.Now;

        await _dbContext.SaveChangesAsync(ct);

        await _activityLog.LogAsync(userId, Domain.Enums.ActivityActionType.Update, nameof(DocumentNote), id,
            details: isArchived ? "Zarchiwizowano" : "Przywrócono z archiwum", ct: ct);
    }

    public async Task<AttachmentDto> AddAttachmentAsync(int noteId, string sourceFilePath, CancellationToken ct = default)
    {
        var userId = RequireCurrentUserId();

        var note = await _dbContext.DocumentNotes.FirstOrDefaultAsync(n => n.Id == noteId, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono uwagi o id {noteId}.");

        var attachment = await _attachmentStorage.StoreAsync(sourceFilePath, userId, ct);
        attachment.NoteId = noteId;

        _dbContext.NoteAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(ct);

        await _activityLog.LogAsync(userId, Domain.Enums.ActivityActionType.Update, nameof(NoteAttachment), attachment.Id,
            details: $"Dodano załącznik {attachment.OriginalFileName}", ct: ct);

        return new AttachmentDto
        {
            Id = attachment.Id,
            NoteId = attachment.NoteId,
            OriginalFileName = attachment.OriginalFileName,
            StoredFileName = attachment.StoredFileName,
            RelativePath = attachment.RelativePath,
            ContentType = attachment.ContentType,
            Extension = attachment.Extension,
            SizeBytes = attachment.SizeBytes,
            UploadedByDisplayName = _currentUser.Current?.DisplayName ?? string.Empty,
            CreatedAt = attachment.CreatedAt
        };
    }

    public async Task RemoveAttachmentAsync(int attachmentId, CancellationToken ct = default)
    {
        var userId = RequireCurrentUserId();

        var attachment = await _dbContext.NoteAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (attachment is null)
        {
            return;
        }

        _attachmentStorage.Delete(attachment);
        _dbContext.NoteAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(ct);

        await _activityLog.LogAsync(userId, Domain.Enums.ActivityActionType.Update, nameof(NoteAttachment), attachmentId,
            details: "Usunięto załącznik", ct: ct);
    }

    private int RequireCurrentUserId() =>
        _currentUser.Current?.Id ?? throw new InvalidOperationException("Brak zalogowanego użytkownika.");
}
