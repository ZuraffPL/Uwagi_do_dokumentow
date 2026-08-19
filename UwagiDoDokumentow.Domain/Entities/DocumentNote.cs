namespace UwagiDoDokumentow.Domain.Entities;

/// <summary>
/// Główny rekord aplikacji — uwaga do dokumentu: kto zlecił, kiedy, dlaczego
/// i w jakich okolicznościach dokument powstał.
/// </summary>
public class DocumentNote
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
    public int CreatedByUserId { get; set; }
    public int UpdatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DocumentType? DocumentType { get; set; }
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
    public ICollection<NoteAttachment> Attachments { get; set; } = new List<NoteAttachment>();
}
