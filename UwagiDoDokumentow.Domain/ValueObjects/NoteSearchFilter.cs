namespace UwagiDoDokumentow.Domain.ValueObjects;

/// <summary>
/// Zestaw kryteriów wyszukiwania/filtrowania uwag do dokumentów, używany przez ISearchService.
/// </summary>
public class NoteSearchFilter
{
    public int? Id { get; set; }
    public string? DocumentSymbol { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? DocumentDateFrom { get; set; }
    public DateTime? DocumentDateTo { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? OrderedBy { get; set; }
    public string? Phrase { get; set; }
    public bool? OnlyWithAttachments { get; set; }
    public bool? IsArchived { get; set; }
}
