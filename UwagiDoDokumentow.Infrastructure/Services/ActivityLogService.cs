using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Domain.Enums;
using UwagiDoDokumentow.Infrastructure.Persistence;

namespace UwagiDoDokumentow.Infrastructure.Services;

/// <summary>
/// Zapis wpisów do logu aktywności biznesowej (activity_log).
/// </summary>
public class ActivityLogService : IActivityLogService
{
    private readonly NotesDbContext _dbContext;

    public ActivityLogService(NotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(int userId, ActivityActionType actionType, string? entityType = null, int? entityId = null, string? details = null, CancellationToken ct = default)
    {
        _dbContext.ActivityLog.Add(new ActivityLogEntry
        {
            UserId = userId,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.Now
        });

        await _dbContext.SaveChangesAsync(ct);
    }
}
