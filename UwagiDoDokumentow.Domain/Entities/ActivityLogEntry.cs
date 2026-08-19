using UwagiDoDokumentow.Domain.Enums;

namespace UwagiDoDokumentow.Domain.Entities;

/// <summary>
/// Wpis w logu aktywności biznesowej (kto/co/kiedy zrobił w danych).
/// Oddzielny od technicznego logu błędów.
/// </summary>
public class ActivityLogEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ActivityActionType ActionType { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
