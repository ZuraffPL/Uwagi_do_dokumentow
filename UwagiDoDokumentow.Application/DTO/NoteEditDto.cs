namespace UwagiDoDokumentow.Application.DTO;

/// <summary>
/// Dane edytowalne uwagi do dokumentu — używane przy tworzeniu/aktualizacji.
/// </summary>
public class NoteEditDto
{
    public int Id { get; set; }
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public string DocumentSymbol { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string OrderedBy { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsArchived { get; set; }
}
