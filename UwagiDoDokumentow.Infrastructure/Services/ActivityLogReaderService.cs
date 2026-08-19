using Microsoft.EntityFrameworkCore;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Infrastructure.Persistence;

namespace UwagiDoDokumentow.Infrastructure.Services;

/// <summary>
/// Odczyt wpisów logu aktywności biznesowej — do ekranu administracyjnego.
/// </summary>
public class ActivityLogReaderService : IActivityLogReaderService
{
    private readonly NotesDbContext _dbContext;

    public ActivityLogReaderService(NotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ActivityLogEntry>> GetRecentAsync(int take = 200, CancellationToken ct = default) =>
        await _dbContext.ActivityLog
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync(ct);

    public async Task<List<ActivityLogEntry>> GetForEntityAsync(string entityType, int entityId, CancellationToken ct = default) =>
        await _dbContext.ActivityLog
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
}
