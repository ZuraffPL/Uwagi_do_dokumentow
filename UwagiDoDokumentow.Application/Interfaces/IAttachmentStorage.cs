using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Przechowywanie plików załączników na dysku. Same pliki nie trafiają do bazy jako BLOB —
/// tylko metadane, ścieżki są względne wobec katalogu bazowego aplikacji.
/// </summary>
public interface IAttachmentStorage
{
    /// <summary>Waliduje rozszerzenie i rozmiar, kopiuje plik do docelowej lokalizacji i zwraca metadane do zapisania w bazie.</summary>
    Task<NoteAttachment> StoreAsync(string sourceFilePath, int uploadedByUserId, CancellationToken ct = default);

    /// <summary>Usuwa fizyczny plik z dysku (jeśli istnieje).</summary>
    void Delete(NoteAttachment attachment);

    /// <summary>Zwraca pełną ścieżkę do pliku na dysku.</summary>
    string GetFullPath(NoteAttachment attachment);
}
