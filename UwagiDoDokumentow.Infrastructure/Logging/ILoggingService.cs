namespace UwagiDoDokumentow.Infrastructure.Logging;

/// <summary>
/// Techniczny log błędów (debug_log.txt) — oddzielny od logu aktywności biznesowej.
/// Limit rozmiaru pliku, najnowsze wpisy na górze.
/// </summary>
public interface ILoggingService
{
    void LogError(string message, Exception? exception = null);
    void LogInfo(string message);
}
