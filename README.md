# Uwagi do dokumentów

Aplikacja desktopowa (Windows 10 x64) dla wielu użytkowników firmowych, będąca
centralnym repozytorium kontekstu do dokumentów obiegających w firmie (FO, PZ,
WZ, SO itd.). Dla każdego "problematycznego" dokumentu użytkownik zapisuje: kto
zlecił, kiedy, dlaczego i w jakich okolicznościach dokument powstał, oraz
podpina dowody (skany, screeny, maile, pliki). Koniec z szukaniem uzasadnień w
mailach, komunikatorach i segregatorach pół roku po fakcie.

Aplikacja jest **wieloużytkownikowa** — każdy wpis ma autora, a dostęp do
dodawania/edycji/usuwania jest kontrolowany uprawnieniami i rejestrowany w
logu aktywności.

## Spis treści

- [Funkcjonalności](#funkcjonalności)
- [Stack technologiczny](#stack-technologiczny)
- [Architektura i struktura projektu](#architektura-i-struktura-projektu)
- [Model danych](#model-danych)
- [Uruchamianie w trybie deweloperskim](#uruchamianie-w-trybie-deweloperskim)
- [Budowanie instalatora](#budowanie-instalatora)
- [Instalacja (użytkownik końcowy)](#instalacja-użytkownik-końcowy)
- [Licencja](#licencja)

## Funkcjonalności

- **Uwagi do dokumentów** — pełny rekord: data, symbol i numer dokumentu, kto
  zlecił, tytuł, treść (kontekst/przyczyna), tagi, status archiwum.
- **Załączniki** — dowolna liczba plików na wpis: obrazy/skany (jpg, png,
  webp), dokumenty (pdf, txt, rtf, doc, docx, odt, ods, odp), archiwa (zip,
  rar). Miniatury podglądu dla obrazów, otwieranie domyślną aplikacją systemową.
- **Wyszukiwanie i filtrowanie** — po numerze/symbolu dokumentu, zakresie dat,
  autorze, zlecającym, frazie w tytule/treści, obecności załączników, statusie
  archiwum (aktywne/zarchiwizowane).
- **Archiwizacja** — checkbox w liście, menu kontekstowe (PPM) lub skrót
  klawiszowy, bez utraty historii wpisu.
- **Historia zmian rekordu** — podgląd, co i kiedy zostało zmienione w danym
  wpisie (na podstawie logu aktywności), z wizualnym wyróżnieniem
  edytowanych rekordów na liście.
- **Wydruk i eksport PDF** — pojedynczego rekordu (z listą załączników) oraz
  zestawień wielu zaznaczonych rekordów, generowane code-first przez QuestPDF.
- **Użytkownicy i uprawnienia** — logowanie (username + hasło, hasła hashowane
  BCrypt), flagi uprawnień per użytkownik (dodawanie/edycja/usuwanie),
  konto administratora do zarządzania użytkownikami i słownikiem symboli
  dokumentów. Konta nigdy nie są fizycznie usuwane bez spełnienia warunków
  bezpieczeństwa (brak powiązanych rekordów) — domyślnie tylko dezaktywowane.
- **Log aktywności** — kto, co i kiedy zrobił w danych (logowania,
  tworzenie/edycja/usuwanie, druk, eksport, import), przeglądany z poziomu
  panelu administracyjnego.
- **Backup / restore** — eksport spójnej migawki bazy (SQLite `VACUUM INTO`) +
  załączników do archiwum zip, oraz import z powrotem do aplikacji.
- **Panel administracyjny** — zarządzanie użytkownikami, słownikiem symboli
  dokumentów (`document_types`) i przegląd logu aktywności — dostępne tylko
  dla kont z flagą administratora.
- **Ekran "O programie" i "Pomoc"** — wersja aplikacji, autor, stos
  technologiczny oraz statyczna instrukcja obsługi dostępne z poziomu menu.

## Stack technologiczny

- **Język:** C# (.NET 10), aplikacja wyłącznie pod Windows 10 x64
- **UI:** WPF (Windows Presentation Foundation) + XAML
- **Wzorzec:** MVVM przez `CommunityToolkit.Mvvm` (`ObservableObject`,
  `[ObservableProperty]`, `[RelayCommand]`)
- **Baza danych:** SQLite 3 przez EF Core (`Microsoft.EntityFrameworkCore.Sqlite`),
  tryb WAL (Write-Ahead Logging) — obsługuje pracę wielu użytkowników
  równolegle na wspólnym zasobie sieciowym
- **DI / Hosting:** `Microsoft.Extensions.Hosting` +
  `Microsoft.Extensions.DependencyInjection`
- **PDF:** `QuestPDF` — generowanie PDF pojedynczego rekordu i zestawień,
  code-first, bez pośredniego renderu HTML
- **Hasła:** `BCrypt.Net-Next` (hash + sól w jednym stringu)
- **Instalator:** Inno Setup 6 (self-contained, .NET runtime dołączony —
  komputer docelowy nie wymaga osobnej instalacji .NET)

## Architektura i struktura projektu

Układ warstwowy: **View → ViewModel → Application Service → Repository/DbContext**.
UI nie zna szczegółów SQLite ani systemu plików; ViewModel nie wie nic o EF Core.

```
UwagiDoDokumentow.slnx
│
├─ UwagiDoDokumentow.App              WPF (.NET 10) — XAML, ViewModel-e, nawigacja
│  ├─ Views/                          okna (Login, Shell, NotesList, NoteEditor,
│  │                                  NoteDetails, NoteHistory, About, Help,
│  │                                  Admin/ — Users, DocumentTypes, ActivityLog)
│  ├─ ViewModels/
│  ├─ Converters/                     konwertery WPF (miniatury, widoczność itd.)
│  ├─ Services/                       CurrentUserService, UiDispatcher
│  └─ Styles/                         style i szablony kontrolek (AppStyles.xaml)
│
├─ UwagiDoDokumentow.Domain           encje, enumy, value objects — bez zależności
│  ├─ Entities/                       DocumentNote, NoteAttachment, DocumentType,
│  │                                  User, ActivityLogEntry
│  ├─ Enums/                          ActivityActionType, AttachmentKind
│  └─ ValueObjects/                   DocumentNumber, NoteSearchFilter
│
├─ UwagiDoDokumentow.Application      interfejsy serwisów + DTO (bez EF/WPF)
│  ├─ Interfaces/                     INotesService, IUserService, IPrintService,
│  │                                  IActivityLogService, ICurrentUserService...
│  └─ DTO/                            NoteListItemDto, NoteDetailsDto, NoteEditDto...
│
└─ UwagiDoDokumentow.Infrastructure    implementacje, EF Core, SQLite, storage, PDF
   ├─ Persistence/                    NotesDbContext, Configurations, Migrations
   ├─ Services/                       NotesService, UserService, ActivityLogService,
   │                                  ActivityLogReaderService, DocumentTypesService,
   │                                  BackupService
   ├─ Storage/                        LocalAttachmentStorage
   ├─ Security/                       PasswordHasher (BCrypt)
   ├─ Printing/                       QuestPdfNoteRenderer
   ├─ Logging/                        LoggingService (log techniczny)
   └─ AppPaths.cs                     ścieżki danych aplikacji (baza, załączniki)
```

> Konkretne implementacje serwisów leżą w `Infrastructure/Services/` (nie w
> `Application/Services/`), bo wymagają bezpośredniego dostępu do
> `NotesDbContext`, a `Application` celowo nie referencuje `Infrastructure`.
> Interfejsy pozostają w `Application/Interfaces/`.

## Model danych

Główne tabele SQLite (`snake_case`):

| Tabela | Opis |
|---|---|
| `document_notes` | Główny rekord — uwaga do dokumentu (data, symbol, numer, kto zlecił, treść, tagi, archiwum) |
| `note_attachments` | Załączniki powiązane z uwagą (ścieżka na dysku, metadane, rozmiar) |
| `document_types` | Słownik symboli dokumentów (FO, PZ, WZ, SO...), rozszerzalny z panelu administracyjnego |
| `users` | Konta użytkowników, hash hasła, flagi uprawnień (`can_add`/`can_edit`/`can_delete`/`is_admin`) |
| `activity_log` | Log biznesowy: kto/co/kiedy zrobił w danych (login, create, update, delete, print, export, import) |

Pliki załączników **nie** są trzymane w bazie jako BLOB — tylko metadane i
ścieżka względna; same pliki leżą na dysku (`Data/attachments/{rok}/{miesiąc}/{guid}.{ext}`).

## Uruchamianie w trybie deweloperskim

```powershell
# Budowanie + uruchomienie (skrypt w katalogu głównym repo)
.\uruchom.bat
```

`uruchom.bat` buduje projekt (`dotnet build`) i uruchamia plik `.exe`
bezpośrednio z `bin\Debug\net10.0-windows\` — **nie** przez `dotnet run`, bo to
nie działa poprawnie z WPF pack URI.

Migracje EF Core:

```powershell
dotnet ef migrations add <Nazwa> --project UwagiDoDokumentow.Infrastructure --startup-project UwagiDoDokumentow.App
dotnet ef database update --project UwagiDoDokumentow.Infrastructure --startup-project UwagiDoDokumentow.App
```

## Budowanie instalatora

Wymaga zainstalowanego [Inno Setup 6](https://jrsoftware.org/isdl.php).

```powershell
.\Zbuduj-Installer.bat
```

Skrypt (`Publish-App.ps1`) publikuje aplikację jako self-contained (win-x64),
odczytuje wersję z `UwagiDoDokumentow.App.csproj` i kompiluje instalator
`UwagiDoDokumentow_Setup_{wersja}.exe` w katalogu głównym repozytorium.

`Czysc-Buildy.bat` czyści katalogi `bin`/`obj`/`publish` (z zachowaniem
folderu `Data` z bazą i załącznikami, jeśli istnieje w `publish`).

## Instalacja (użytkownik końcowy)

Gotowe instalatory dostępne są w zakładce
[Releases](../../releases) repozytorium. Instalator jest self-contained —
nie wymaga osobnej instalacji .NET na komputerze docelowym i nie wymaga
uprawnień administratora.

Przy pierwszym uruchomieniu tworzone jest konto administratora (login
`admin`), z hasłem pokazanym jednorazowo w oknie komunikatu — zalecana zmiana
hasła po pierwszym zalogowaniu.

## Licencja

Projekt jest udostępniony na licencji [MIT](LICENSE).
