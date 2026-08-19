namespace UwagiDoDokumentow.Application.DTO;

/// <summary>
/// Wiersz listy uwag — dane potrzebne do wyświetlenia w gridzie/liście.
/// </summary>
public class NoteListItemDto
{
    public int Id { get; set; }
    public DateTime DocumentDate { get; set; }
    public string DocumentSymbol { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string OrderedBy { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Tags { get; set; }
    public bool IsArchived { get; set; }
    public bool WasModified { get; set; }
    public int AttachmentsCount { get; set; }
    public string CreatedByDisplayName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
