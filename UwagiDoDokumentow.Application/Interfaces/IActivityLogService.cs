using UwagiDoDokumentow.Domain.Enums;

namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Log aktywności biznesowej (kto/co/kiedy zrobił w danych) — zapis do tabeli activity_log.
/// Oddzielny od technicznego logu błędów (ILoggingService).
/// </summary>
public interface IActivityLogService
{
    Task LogAsync(int userId, ActivityActionType actionType, string? entityType = null, int? entityId = null, string? details = null, CancellationToken ct = default);
}
