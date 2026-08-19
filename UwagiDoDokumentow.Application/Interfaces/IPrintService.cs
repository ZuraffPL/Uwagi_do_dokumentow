namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Generowanie i drukowanie/eksport PDF pojedynczego rekordu oraz zestawień.
/// </summary>
public interface IPrintService
{
    Task<string> GenerateNotePdfAsync(int noteId, CancellationToken ct = default);
    Task<string> GenerateNotesListPdfAsync(IEnumerable<int> noteIds, CancellationToken ct = default);
    void OpenPreview(string pdfFilePath);
    void Print(string pdfFilePath);
}
