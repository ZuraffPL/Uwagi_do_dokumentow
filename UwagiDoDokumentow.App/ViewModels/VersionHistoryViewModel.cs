namespace UwagiDoDokumentow.App.ViewModels;

/// <summary>Pojedyncza wersja aplikacji wraz z listą zmian.</summary>
public sealed record VersionHistoryEntry(string Version, string ReleaseDate, IReadOnlyList<string> Changes)
{
    public string Header => $"Wersja {Version} — {ReleaseDate}";
}

/// <summary>
/// Stan okna "Historia wersji" — lista zmian w kolejnych wersjach aplikacji (od najnowszej).
/// Przy wydaniu nowej wersji dopisz wpis na początku listy <see cref="Entries"/>.
/// </summary>
public class VersionHistoryViewModel
{
    public IReadOnlyList<VersionHistoryEntry> Entries { get; } =
    [
        new("1.1.4", "2026-10-27",
        [
            "Dodano pozycję menu „Historia wersji” z listą zmian w kolejnych wersjach aplikacji.",
            "Aktualizacja bibliotek: .NET/EF Core 10.0.14, QuestPDF 2026.9.1, SQLitePCLRaw 3.0.5."
        ]),
        new("1.1.3", "2026-08-31",
        [
            "Naprawa skrótu na pulpicie tworzonego przez instalator — skrót trafia teraz na pulpit bieżącego użytkownika, dzięki czemu działa także bez uprawnień administratora."
        ]),
        new("1.1.2", "2026-08-19",
        [
            "Logowanie użytkowników oraz uprawnienia (dodawanie, edycja, usuwanie, administrator).",
            "Lista uwag do dokumentów z filtrowaniem (symbol, zakres dat, załączniki, archiwum, fraza).",
            "Dodawanie i edycja uwag wraz z załącznikami (skany, zdjęcia, dokumenty, archiwa).",
            "Archiwizacja uwag, historia zmian rekordu oraz podświetlanie edytowanych uwag.",
            "Drukowanie i eksport uwag do PDF.",
            "Panel administracyjny: użytkownicy, symbole dokumentów, log aktywności.",
            "Eksport i import kopii zapasowej bazy danych wraz z załącznikami.",
            "Instrukcja obsługi oraz ekran „O programie”."
        ])
    ];
}
