namespace UwagiDoDokumentow.Domain.Entities;

/// <summary>
/// Słownik symboli dokumentów (np. FO, PZ, WZ, SO). Lista jest zamknięta,
/// ale rozszerzalna przez panel administracyjny.
/// </summary>
public class DocumentType
{
    public string Symbol { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
