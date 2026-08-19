using System.Text;

namespace UwagiDoDokumentow.Infrastructure.Logging;

/// <summary>
/// Implementacja technicznego logu błędów. Plik debug_log.txt, limit 5 MB,
/// najnowsze wpisy na górze pliku.
/// </summary>
public class LoggingService : ILoggingService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly object SyncRoot = new();

    public void LogError(string message, Exception? exception = null)
    {
        var entry = new StringBuilder()
            .Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("] BŁĄD: ")
            .Append(message);

        if (exception is not null)
        {
            entry.AppendLine().Append(exception);
        }

        WriteEntry(entry.ToString());
    }

    public void LogInfo(string message)
    {
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
        WriteEntry(entry);
    }

    private void WriteEntry(string entry)
    {
        // Blok "firewall" — log techniczny nie może sam wywołać kolejnego wyjątku
        // i zapętlić aplikacji, dlatego cichy catch jest tu świadomym wyjątkiem od reguły.
        try
        {
            lock (SyncRoot)
            {
                AppPaths.EnsureDirectoriesExist();
                var path = AppPaths.LogFilePath;
                var existing = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
                var updated = entry + Environment.NewLine + existing;

                if (Encoding.UTF8.GetByteCount(updated) > MaxFileSizeBytes)
                {
                    updated = updated[..(int)MaxFileSizeBytes];
                }

                File.WriteAllText(path, updated, Encoding.UTF8);
            }
        }
        catch
        {
            // Celowo pomijamy błąd — logowanie nie może wywrócić działania aplikacji.
        }
    }
}
