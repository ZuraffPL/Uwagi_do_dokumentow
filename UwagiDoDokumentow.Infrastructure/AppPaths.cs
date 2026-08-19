namespace UwagiDoDokumentow.Infrastructure;

/// <summary>
/// Ścieżki danych aplikacji. Aplikacja jest instalowana na wspólnym dysku sieciowym i uruchamiana
/// przez wielu użytkowników jednocześnie — dlatego dane (baza SQLite w trybie WAL + załączniki)
/// domyślnie leżą w katalogu "Data" obok pliku .exe, a nie w profilu lokalnym użytkownika
/// (%LocalAppData% byłoby osobne dla każdego stanowiska i każdego użytkownika, co uniemożliwiłoby
/// współdzielenie kontekstu — całego sensu tej aplikacji).
/// </summary>
public static class AppPaths
{
    /// <summary>
    /// Katalog bazowy danych aplikacji. Można nadpisać zmienną środowiskową
    /// UWAGI_DATA_DIR, np. żeby wskazać inny wspólny zasób sieciowy niż katalog instalacji.
    /// </summary>
    public static string DataRootDirectory
    {
        get
        {
            var overridePath = Environment.GetEnvironmentVariable("UWAGI_DATA_DIR");
            if (!string.IsNullOrWhiteSpace(overridePath))
            {
                return overridePath;
            }

            return Path.Combine(AppContext.BaseDirectory, "Data");
        }
    }

    public static string DatabaseDirectory => Path.Combine(DataRootDirectory, "data");
    public static string DatabaseFilePath => Path.Combine(DatabaseDirectory, "notes.db");
    public static string AttachmentsDirectory => Path.Combine(DataRootDirectory, "attachments");
    public static string LogFilePath => Path.Combine(DataRootDirectory, "debug_log.txt");
    public static string TempDirectory => Path.Combine(DataRootDirectory, "temp");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(DatabaseDirectory);
        Directory.CreateDirectory(AttachmentsDirectory);
        Directory.CreateDirectory(TempDirectory);
    }
}
