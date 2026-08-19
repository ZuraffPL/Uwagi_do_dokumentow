namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Eksport/import całej bazy danych (dokumenty, użytkownicy wraz z hasłami, załączniki)
/// do/z pojedynczego archiwum ZIP. Przeznaczone do ręcznych kopii zapasowych i migracji danych.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Tworzy spójną migawkę bazy danych (SQLite VACUUM INTO) razem z katalogiem załączników
    /// i zapisuje je jako jedno archiwum ZIP pod wskazaną ścieżką.
    /// </summary>
    Task ExportAsync(string destinationZipFilePath, int performedByUserId, CancellationToken ct = default);

    /// <summary>
    /// Nadpisuje bieżącą bazę danych i katalog załączników zawartością wskazanego archiwum ZIP
    /// (utworzonego wcześniej przez <see cref="ExportAsync"/>). Operacja niszcząca — wymaga,
    /// aby żaden inny proces/użytkownik nie korzystał w tym momencie z bazy danych.
    /// </summary>
    Task ImportAsync(string sourceZipFilePath, int performedByUserId, CancellationToken ct = default);
}
