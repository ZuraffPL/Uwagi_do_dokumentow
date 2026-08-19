namespace UwagiDoDokumentow.Domain.Entities;

/// <summary>
/// Metadane załącznika podpiętego do uwagi. Sam plik jest przechowywany na dysku,
/// pod nazwą wygenerowaną jako GUID + oryginalne rozszerzenie (nigdy oryginalna nazwa).
/// </summary>
public class NoteAttachment
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string Extension { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public DocumentNote? Note { get; set; }
    public User? UploadedByUser { get; set; }
}
