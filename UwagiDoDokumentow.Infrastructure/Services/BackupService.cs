using System.IO.Compression;
using Microsoft.Data.Sqlite;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Enums;

namespace UwagiDoDokumentow.Infrastructure.Services;

/// <summary>
/// Eksport/import całej bazy danych (SQLite + załączniki) do/z pojedynczego archiwum ZIP.
/// Eksport korzysta z `VACUUM INTO`, dzięki czemu tworzy spójną migawkę bez blokowania bazy
/// dla innych, aktualnie pracujących użytkowników. Import jest operacją niszczącą i wymaga,
/// aby w danym momencie nikt inny nie korzystał z bazy na wspólnym zasobie sieciowym.
/// </summary>
public class BackupService : IBackupService
{
    private const string DatabaseEntryName = "notes.db";
    private const string AttachmentsEntryPrefix = "attachments";

    private readonly IActivityLogService _activityLog;

    public BackupService(IActivityLogService activityLog)
    {
        _activityLog = activityLog;
    }

    public async Task ExportAsync(string destinationZipFilePath, int performedByUserId, CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();
        var tempDbCopy = Path.Combine(AppPaths.TempDirectory, $"notes_export_{Guid.NewGuid():N}.db");

        try
        {
            await using (var connection = new SqliteConnection($"Data Source={AppPaths.DatabaseFilePath};Mode=ReadOnly"))
            {
                await connection.OpenAsync(ct);
                await using var command = connection.CreateCommand();
                command.CommandText = "VACUUM INTO $path;";
                command.Parameters.AddWithValue("$path", tempDbCopy);
                await command.ExecuteNonQueryAsync(ct);
            }

            if (File.Exists(destinationZipFilePath))
            {
                File.Delete(destinationZipFilePath);
            }

            using var archive = ZipFile.Open(destinationZipFilePath, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(tempDbCopy, DatabaseEntryName);

            if (Directory.Exists(AppPaths.AttachmentsDirectory))
            {
                foreach (var file in Directory.EnumerateFiles(AppPaths.AttachmentsDirectory, "*", SearchOption.AllDirectories))
                {
                    var relative = $"{AttachmentsEntryPrefix}/{Path.GetRelativePath(AppPaths.AttachmentsDirectory, file).Replace('\\', '/')}";
                    archive.CreateEntryFromFile(file, relative);
                }
            }
        }
        finally
        {
            if (File.Exists(tempDbCopy))
            {
                File.Delete(tempDbCopy);
            }
        }

        await _activityLog.LogAsync(performedByUserId, ActivityActionType.Export, details: destinationZipFilePath, ct: ct);
    }

    public async Task ImportAsync(string sourceZipFilePath, int performedByUserId, CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();
        var tempExtractDir = Path.Combine(AppPaths.TempDirectory, $"import_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempExtractDir);

        try
        {
            ZipFile.ExtractToDirectory(sourceZipFilePath, tempExtractDir);

            var importedDb = Path.Combine(tempExtractDir, DatabaseEntryName);
            if (!File.Exists(importedDb))
            {
                throw new InvalidOperationException("Wybrany plik nie zawiera poprawnej kopii bazy danych (brak notes.db).");
            }

            // Zwolnienie puli połączeń SQLite w tym procesie, żeby uwolnić uchwyt pliku bazy
            // przed jego nadpisaniem. Nie chroni to przed innymi procesami/użytkownikami
            // mającymi otwartą aplikację — o to musi zadbać operator przed importem.
            SqliteConnection.ClearAllPools();

            File.Copy(importedDb, AppPaths.DatabaseFilePath, overwrite: true);

            var importedAttachments = Path.Combine(tempExtractDir, AttachmentsEntryPrefix);
            if (Directory.Exists(importedAttachments))
            {
                if (Directory.Exists(AppPaths.AttachmentsDirectory))
                {
                    Directory.Delete(AppPaths.AttachmentsDirectory, recursive: true);
                }

                CopyDirectory(importedAttachments, AppPaths.AttachmentsDirectory);
            }
        }
        finally
        {
            if (Directory.Exists(tempExtractDir))
            {
                Directory.Delete(tempExtractDir, recursive: true);
            }
        }

        // Zapis do logu po imporcie odbywa się już na nowej (zaimportowanej) bazie danych.
        await _activityLog.LogAsync(performedByUserId, ActivityActionType.Import, details: sourceZipFilePath, ct: ct);
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);
            var destPath = Path.Combine(destDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            File.Copy(file, destPath, overwrite: true);
        }
    }
}
