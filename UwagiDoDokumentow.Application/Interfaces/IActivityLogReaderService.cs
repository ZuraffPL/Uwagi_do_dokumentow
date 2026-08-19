using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Odczyt wpisów z logu aktywności biznesowej — do ekranu administracyjnego.
/// </summary>
public interface IActivityLogReaderService
{
    Task<List<ActivityLogEntry>> GetRecentAsync(int take = 200, CancellationToken ct = default);

    /// <summary>Historia zmian pojedynczej encji (np. DocumentNote o danym id), od najnowszych.</summary>
    Task<List<ActivityLogEntry>> GetForEntityAsync(string entityType, int entityId, CancellationToken ct = default);
}
