namespace UwagiDoDokumentow.Application.DTO;

/// <summary>
/// Metadane załącznika prezentowane w UI.
/// </summary>
public class AttachmentDto
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string Extension { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string UploadedByDisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
