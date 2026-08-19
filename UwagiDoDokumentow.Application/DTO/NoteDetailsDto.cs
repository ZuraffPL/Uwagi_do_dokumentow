namespace UwagiDoDokumentow.Application.DTO;

/// <summary>
/// Pełny widok szczegółów uwagi do dokumentu, wraz z listą załączników.
/// </summary>
public class NoteDetailsDto
{
    public int Id { get; set; }
    public DateTime DocumentDate { get; set; }
    public string DocumentSymbol { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string OrderedBy { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsArchived { get; set; }
    public string CreatedByDisplayName { get; set; } = string.Empty;
    public string UpdatedByDisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}
