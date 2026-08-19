# CLAUDE.md — Uwagi do dokumentów

## Kontekst projektu
Aplikacja desktopowa (Windows 10 x64) dla wielu użytkowników firmowych, będąca centralnym repozytorium
kontekstu do dokumentów obiegających w firmie (FO, PZ, WZ, SO itd.). Dla każdego "problematycznego"
dokumentu użytkownik zapisuje: kto zlecił, kiedy, dlaczego i w jakich okolicznościach dokument powstał,
oraz podpina dowody (skany, screeny, maile, pliki). Cel: koniec z szukaniem uzasadnień w mailach,
komunikatorach i segregatorach pół roku po fakcie.

Aplikacja jest **wieloużytkownikowa** — każdy wpis ma autora, a dostęp do dodawania/edycji/usuwania
jest kontrolowany uprawnieniami i rejestrowany w logu aktywności. To odróżnia ją od pierwotnego MVP
opisanego w dokumencie projektowym, który świadomie pomijał logowanie na etapie MVP — w tej aplikacji
logowanie i uprawnienia są wymaganiem od pierwszej wersji, nie etapem 2.

## Stack technologiczny
- **Język:** C# (.NET 10), aplikacja wyłącznie pod Windows 10 x64
- **UI Framework:** WPF (Windows Presentation Foundation)
- **Markup UI:** XAML
- **MVVM:** `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`)
- **Baza danych:** SQLite 3 przez EF Core (`Microsoft.EntityFrameworkCore.Sqlite`), tryb **WAL**
  (Write-Ahead Logging) — konieczny, jeśli plik bazy leży na współdzielonym zasobie sieciowym
  i kilku użytkowników pracuje równolegle
- **ORM:** EF Core z migracjami
- **DI / Hosting:** `Microsoft.Extensions.Hosting` + `Microsoft.Extensions.DependencyInjection`
- **PDF:** `QuestPDF` — generowanie PDF pojedynczego rekordu i zestawień, bez pośredniego renderu HTML
- **Hasła:** `BCrypt.Net-Next` (hashowanie + sól w jednym), alternatywa: wbudowany `Rfc2898DeriveBytes`
  (PBKDF2) bez dodatkowej zależności — patrz sekcja *Użytkownicy i uprawnienia*
- **IDE:** VS Code + GitHub Copilot (Claude Sonnet/Opus)

> ⚠️ **Uwaga licencyjna QuestPDF:** od wersji 2023 biblioteka działa na licencji Community/Commercial —
> Community jest darmowa dla firm o przychodzie rocznym poniżej 1 mln USD i dla podmiotów non-profit,
> powyżej tego progu wymagana jest licencja komercyjna. Przed wdrożeniem produkcyjnym zweryfikuj to
> z osobą decyzyjną w firmie.

## Architektura projektu
Układ warstwowy (Clean-ish, bez przesadnego DDD — MVP ma być czytelne i szybkie w rozwoju):

```
UwagiDoDokumentow.sln
│
├─ UwagiDoDokumentow.App              // WPF .NET 10 — XAML, ViewModel-e, nawigacja, bindingi
├─ UwagiDoDokumentow.Domain           // encje, enumy, reguły domenowe (bez zależności od EF/WPF)
├─ UwagiDoDokumentow.Infrastructure   // EF Core, SQLite, repozytoria, storage plików, PDF, hasła
└─ UwagiDoDokumentow.Application      // serwisy aplikacyjne, DTO, filtry, uprawnienia, logi aktywności
```

Przepływ: **View → ViewModel → Application Service → Repository/DbContext**.
UI nie zna szczegółów SQLite ani systemu plików; ViewModel nie wie nic o EF Core.

- Wzorzec **MVVM** obowiązkowy dla WPF
- Dostęp do bazy przez **Service Pattern** — Services implementują interfejsy
  (`INotesService`, `ISearchService`, `IPrintService`, `IUserService`, `IActivityLogService` itd.),
  ViewModele korzystają wyłącznie z interfejsów
- Połączenie z SQLite tylko przez `NotesDbContext` (EF Core DbContext)
- Schemat bazy zarządzany przez **migracje EF Core**, nie ręczny SQL

## Konwencje nazewnicze
- Klasy: `PascalCase` (np. `DocumentNote`, `NotesListViewModel`)
- Właściwości i pola publiczne: `PascalCase`
- Pola prywatne: `_camelCase`
- Pliki XAML: nazwa = nazwa klasy (np. `NoteEditorView.xaml` + `NoteEditorView.xaml.cs`)
- Tabele SQLite: `snake_case` (np. `document_notes`, `note_attachments`, `activity_log`)

## Reguły XAML / WPF
- Logika **tylko** w ViewModelu, nigdy w code-behind (`*.xaml.cs`)
- `INotifyPropertyChanged` przez dziedziczenie po `ObservableObject`
- Bindowania zawsze przez `{Binding PropertyName}` — bez bezpośredniej manipulacji UI z C#
- `ICommand` (`RelayCommand`/`AsyncRelayCommand`) zamiast event handlerów w code-behind
- Widoczność/dostępność przycisków (Dodaj/Edytuj/Usuń) wiązana do uprawnień bieżącego użytkownika
  przez `ICurrentUserService`, nie przez sprawdzanie ról w code-behind
- Style i szablony kontrolek w osobnych plikach ResourceDictionary (`Styles/`)

## Model danych

### document_notes
Główny rekord — uwaga do dokumentu.

```sql
CREATE TABLE document_notes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    document_date TEXT NOT NULL,
    document_symbol TEXT NOT NULL,          -- FK -> document_types.symbol
    document_number TEXT NOT NULL,
    ordered_by TEXT NOT NULL,               -- kto zlecił dokument
    title TEXT NULL,
    content TEXT NOT NULL,                  -- kontekst, sytuacja, przyczyna
    tags TEXT NULL,
    is_archived INTEGER NOT NULL DEFAULT 0,
    created_by_user_id INTEGER NOT NULL,
    updated_by_user_id INTEGER NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    FOREIGN KEY (document_symbol) REFERENCES document_types(symbol),
    FOREIGN KEY (created_by_user_id) REFERENCES users(id),
    FOREIGN KEY (updated_by_user_id) REFERENCES users(id)
);

CREATE INDEX ix_document_notes_document_date ON document_notes(document_date);
CREATE INDEX ix_document_notes_symbol_number ON document_notes(document_symbol, document_number);
CREATE INDEX ix_document_notes_updated_at ON document_notes(updated_at);
CREATE INDEX ix_document_notes_ordered_by ON document_notes(ordered_by);
```

### note_attachments

```sql
CREATE TABLE note_attachments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    note_id INTEGER NOT NULL,
    original_file_name TEXT NOT NULL,
    stored_file_name TEXT NOT NULL,         -- GUID + rozszerzenie, żeby uniknąć kolizji/path traversal
    relative_path TEXT NOT NULL,
    content_type TEXT NULL,
    extension TEXT NOT NULL,
    size_bytes INTEGER NOT NULL,
    uploaded_by_user_id INTEGER NOT NULL,
    created_at TEXT NOT NULL,
    FOREIGN KEY (note_id) REFERENCES document_notes(id) ON DELETE CASCADE,
    FOREIGN KEY (uploaded_by_user_id) REFERENCES users(id)
);

CREATE INDEX ix_note_attachments_note_id ON note_attachments(note_id);
```

### document_types (tabela słownikowa symboli)
> **Odstępstwo od pierwotnej propozycji MVP:** dokument projektowy sugerował wolny tekst dla symbolu
> na start. Ponieważ lista symboli w firmie jest zamknięta, ale ma być rozszerzalna — właściwym
> rozwiązaniem jest tabela słownikowa + ComboBox, nie wolny tekst.

```sql
CREATE TABLE document_types (
    symbol TEXT PRIMARY KEY,                -- np. 'FO', 'PZ', 'SO'
    description TEXT NULL,
    is_active INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL
);
```

Seed startowy: `FO, FI, PZ, PI, DZ, EK, RE, RR, SO, WZ, RO, KZ, IV, MM, M1, M2, UN, KB, KF`.
Dodawanie nowych symboli — tylko przez ekran administracyjny (patrz niżej), nie wolny wpis w formularzu.

### users

```sql
CREATE TABLE users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    password_hash TEXT NOT NULL,            -- BCrypt: hash + sól w jednym stringu
    is_admin INTEGER NOT NULL DEFAULT 0,
    can_add INTEGER NOT NULL DEFAULT 1,
    can_edit INTEGER NOT NULL DEFAULT 0,
    can_delete INTEGER NOT NULL DEFAULT 0,
    is_active INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL,
    last_login_at TEXT NULL
);
```

Uprawnienia jako proste flagi (`can_add`/`can_edit`/`can_delete`) + `is_admin` do zarządzania
użytkownikami i słownikiem symboli. Prostsze niż pełny system ról na skalę tej aplikacji —
jeśli w przyszłości pojawią się różne kombinacje uprawnień per moduł, można to rozbudować do
osobnej tabeli `role_permissions`.

### activity_log

```sql
CREATE TABLE activity_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    action_type TEXT NOT NULL,              -- LOGIN, LOGOUT, CREATE, UPDATE, DELETE, PRINT, EXPORT
    entity_type TEXT NULL,                  -- 'DocumentNote', 'Attachment', 'User' itd.
    entity_id INTEGER NULL,
    details TEXT NULL,
    created_at TEXT NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

CREATE INDEX ix_activity_log_user_id ON activity_log(user_id);
CREATE INDEX ix_activity_log_created_at ON activity_log(created_at);
```

To jest **log biznesowy** (kto/co/kiedy zrobił w danych), oddzielny od technicznego logu błędów
(`debug_log.txt`) — patrz sekcja *Obsługa błędów i logów*. Przeglądany z poziomu aplikacji
(ekran dostępny tylko dla `is_admin`), nie tylko jako plik.

## Użytkownicy i uprawnienia
- Ekran logowania przy starcie aplikacji (username + hasło); brak zalogowania = brak dostępu do danych
- Hasła **nigdy** w plain tekście — `BCrypt.Net-Next` (`BCrypt.HashPassword` / `BCrypt.Verify`) albo
  wbudowany `Rfc2898DeriveBytes` (PBKDF2), jeśli chcemy zminimalizować liczbę zależności
- `ICurrentUserService` — singleton/scoped serwis trzymający zalogowanego użytkownika w sesji,
  używany do: stemplowania `created_by`/`updated_by`, włączania/wyłączania komend w ViewModelach
  wg uprawnień, oraz zapisu do `activity_log`
- Panel administracyjny (tylko `is_admin`): dodawanie/dezaktywacja użytkowników, zmiana uprawnień,
  reset hasła, zarządzanie słownikiem `document_types`, przegląd `activity_log`
- Konto nie jest nigdy fizycznie usuwane z bazy (żeby nie urwać historii autorstwa) — tylko
  dezaktywowane (`is_active = 0`)

> **Do ustalenia z użytkownikiem (zapytaj, jeśli nieprecyzyjne):** czy baza SQLite będzie leżała
> na wspólnym dysku sieciowym dla kilku stanowisk, czy każde stanowisko ma własną kopię z synchronizacją?
> To wpływa na tryb WAL, blokady plików i strategię backupu — jeśli nieokreślone, domyślnie zakładamy
> wspólny zasób sieciowy + WAL, ale to założenie wymaga potwierdzenia przed implementacją warstwy danych.

## Załączniki
Obsługiwane typy: obrazy/skany (`jpg`, `jpeg`, `png`, `webp`), dokumenty (`pdf`, `txt`, `rtf`, `doc`,
`docx`, `odt`, `ods`, `odp`), archiwa (`zip`, `rar`).

- Pliki **nie** trafiają do bazy jako BLOB — tylko metadane + ścieżka względna w `note_attachments`,
  same pliki na dysku:
  ```
  %LocalAppData%\UwagiDoDokumentow\
  ├─ data\notes.db
  └─ attachments\{rok}\{miesiąc}\{guid}.{ext}
  ```
  (albo odpowiednik na dysku sieciowym, jeśli aplikacja jest wieloużytkownikowa na współdzielonym zasobie)
- Nazwa pliku na dysku = GUID + oryginalne rozszerzenie (nigdy oryginalna nazwa) — zapobiega
  path traversal i kolizjom nazw
- **Whitelist rozszerzeń przy uploadzie** — odrzucaj wszystko spoza listy powyżej, w szczególności
  pliki wykonywalne/skrypty (`exe`, `bat`, `ps1`, `js` itd.)
- Limit rozmiaru pojedynczego pliku — proponowany domyślny próg 50 MB, konfigurowalny
- Miniatury tylko dla `jpg`/`png`/`webp`, generowane natywnym WPF `BitmapImage` z `DecodePixelWidth`
  (bez dodatkowej biblioteki typu System.Drawing); dla pozostałych typów — ikona wg rozszerzenia
- Otwieranie załącznika: `Process.Start` z `UseShellExecute = true` (domyślna aplikacja systemowa) —
  archiwa `zip`/`rar` też się tak otwierają, aplikacja nie musi ich rozpakowywać

## Wyszukiwanie i filtrowanie
`ISearchService` obsługuje filtrowanie po:
- indeksie/ID rekordu
- symbolu i numerze dokumentu
- zakresie dat wystawienia
- autorze wpisu (`created_by_user_id`)
- zlecającym (`ordered_by`)
- frazie w tytule/treści
- obecności załączników (tylko z załącznikami)
- statusie archiwum (aktywne / zarchiwizowane)

Na start `LIKE` w EF Core wystarczy. Gdy liczba rekordów urośnie, rozważ SQLite **FTS5** dla
`title`/`content` (Etap 2) — nie wdrażaj tego przedwcześnie.

## Drukowanie i eksport PDF
- `QuestPDF` generuje PDF code-first (bez pośredniego HTML) — dla:
  - pojedynczego rekordu (pełny widok + lista załączników)
  - zestawienia wielu zaznaczonych rekordów (tabela/karty)
- Podgląd: wygeneruj plik tymczasowy, otwórz domyślną przeglądarką PDF systemu (`Process.Start`)
- Druk: albo otwórz PDF z verbem `"print"` (wysyła do drukarki domyślnej bez UI podglądu),
  albo pozwól użytkownikowi wydrukować z poziomu otwartego podglądu — prostsze i mniej awaryjne
  niż własny dialog druku
- Eksport: standardowy dialog "Zapisz jako", kopiuje wygenerowany PDF do wskazanej lokalizacji
- **Rozważona i odrzucona alternatywa:** WebView2 + render HTML do druku (opcja z dokumentu
  projektowego). Odrzucona na rzecz QuestPDF, bo: jedna zależność mniej, pełna kontrola nad
  layoutem PDF z poziomu C# bez pośredniego HTML/CSS, brak zależności od silnika przeglądarki

## Biblioteki — zasady
Nie dodawaj bibliotek bez potrzeby, ale **aktywnie proponuj** nowe paczki NuGet, jeśli upraszczają
kod lub rozwiązują problem lepiej. Przy każdej propozycji podaj: nazwę NuGet, powód użycia,
alternatywę.

Aktualnie planowane:
- `Microsoft.EntityFrameworkCore.Sqlite` — ORM + driver SQLite
- `Microsoft.EntityFrameworkCore.Tools` / `Design` — CLI do migracji
- `CommunityToolkit.Mvvm` — `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`
- `Microsoft.Extensions.Hosting` — `IHost`, lifecycle aplikacji
- `Microsoft.Extensions.DependencyInjection` — kontener DI
- `QuestPDF` — generowanie i eksport PDF (zweryfikuj licencję — patrz uwaga w sekcji Stack)
- `BCrypt.Net-Next` — hashowanie haseł (alternatywa: wbudowany `Rfc2898DeriveBytes`, zero zależności)

Dopuszczalne w przyszłości:
- `Serilog` — logowanie zdarzeń/błędów (zastąpiłoby ręczny `LoggingService.cs`)
- `xUnit` lub `NUnit` — testy jednostkowe

## Obsługa błędów i logów
Dwa **oddzielne** logi o różnym przeznaczeniu:

1. **Log techniczny błędów** (`ILoggingService`) — plik `debug_log.txt`, limit 5 MB, najnowsze na
   górze. Wszystkie operacje I/O i DB owinięte w `try/catch`. Puste `catch { }` zakazane —
   wyjątek: bloki „firewall" chroniące serwis logowania przed nieskończoną pętlą oraz ostatnia
   linia obrony w `LogUnhandledException` — zawsze skomentuj dlaczego.
2. **Log aktywności biznesowej** (`IActivityLogService`) — zapis do tabeli `activity_log`
   w bazie, przeglądalny z poziomu aplikacji przez administratora. Rejestruje logowania,
   tworzenie/edycję/usuwanie rekordów, druk i eksport — nie błędy techniczne.

Użytkownik zawsze widzi komunikat o błędzie (MessageBox lub status bar) — nie cicha awaria.

## Zachowanie AI (GitHub Copilot / Claude)

### Zadawaj pytania doprecyzowujące, gdy:
- Wymaganie biznesowe jest nieprecyzyjne (np. "dodaj filtrowanie" — po czym filtrować?)
- Istnieją dwa równoważne podejścia architektoniczne, a wybór wpłynie na strukturę projektu
- Nie jesteś pewien, czy zmiana dotknie innych modułów
- Schemat bazy danych wymaga decyzji projektowej (relacje, typy danych, indeksy)
- Dotyczy modelu wdrożenia bazy (lokalnie vs. dysk sieciowy) — patrz otwarta kwestia w sekcji
  *Użytkownicy i uprawnienia*

### Proponuj zmiany bibliotek, gdy:
- Aktualna implementacja jest zbyt skomplikowana bez zewnętrznej pomocy
- Istnieje paczka NuGet rozwiązująca problem stabilnie i popularnie
- Zauważysz, że piszemy "ręcznie" coś, co biblioteka robi lepiej
- Zawsze uzasadnij propozycję i podaj alternatywę

### Ogólne zasady pracy:
- Przed większą zmianą opisz plan w punktach i czekaj na akceptację
- Nie refaktoryzuj kodu poza zakresem zadania bez pytania
- Jeśli zadanie jest duże — podziel na kroki i realizuj etapami
- Kod musi się kompilować po każdym kroku
- Komentarze w kodzie po polsku lub angielsku — konsekwentnie
- Każda operacja modyfikująca dane (dodanie/edycja/usunięcie rekordu, logowanie, druk) musi
  zapisać wpis w `activity_log` — nie pomijaj tego przy dodawaniu nowych funkcji

## Struktura folderów (docelowa)

```
UwagiDoDokumentow.App
├─ App.xaml
├─ Views
│  ├─ LoginView.xaml
│  ├─ ShellWindow.xaml
│  ├─ NotesListView.xaml
│  ├─ NoteEditorView.xaml
│  ├─ NoteDetailsView.xaml
│  ├─ PrintPreviewView.xaml
│  ├─ Admin
│  │  ├─ UsersAdminView.xaml
│  │  ├─ DocumentTypesAdminView.xaml
│  │  └─ ActivityLogView.xaml
│  └─ Dialogs
│     ├─ AttachmentPickerDialog.xaml
│     └─ ConfirmDeleteDialog.xaml
├─ ViewModels
│  ├─ LoginViewModel.cs
│  ├─ ShellViewModel.cs
│  ├─ NotesListViewModel.cs
│  ├─ NoteEditorViewModel.cs
│  ├─ NoteDetailsViewModel.cs
│  ├─ PrintPreviewViewModel.cs
│  ├─ UsersAdminViewModel.cs
│  ├─ DocumentTypesAdminViewModel.cs
│  └─ ActivityLogViewModel.cs
├─ Commands
├─ Converters
├─ Styles
├─ Resources
└─ Services
   ├─ NavigationService.cs
   ├─ CurrentUserService.cs
   └─ UiDispatcher.cs

UwagiDoDokumentow.Domain
├─ Entities
│  ├─ DocumentNote.cs
│  ├─ NoteAttachment.cs
│  ├─ DocumentType.cs
│  ├─ User.cs
│  └─ ActivityLogEntry.cs
├─ ValueObjects
│  ├─ DocumentNumber.cs
│  └─ NoteSearchFilter.cs
└─ Enums
   ├─ AttachmentKind.cs
   └─ ActivityActionType.cs

UwagiDoDokumentow.Infrastructure
├─ Persistence
│  ├─ NotesDbContext.cs
│  ├─ Configurations
│  │  ├─ DocumentNoteConfiguration.cs
│  │  ├─ NoteAttachmentConfiguration.cs
│  │  ├─ UserConfiguration.cs
│  │  └─ ActivityLogConfiguration.cs
│  └─ Migrations
├─ Repositories
│  ├─ DocumentNoteRepository.cs
│  ├─ AttachmentRepository.cs
│  ├─ UserRepository.cs
│  └─ ActivityLogRepository.cs
├─ Storage
│  └─ LocalAttachmentStorage.cs
├─ Security
│  └─ PasswordHasher.cs
├─ Printing
│  └─ QuestPdfNoteRenderer.cs
└─ Logging
   └─ LoggingService.cs

UwagiDoDokumentow.Application
├─ DTO
│  ├─ NoteListItemDto.cs
│  ├─ NoteDetailsDto.cs
│  ├─ NoteEditDto.cs
│  ├─ AttachmentDto.cs
│  └─ UserDto.cs
├─ Services
│  ├─ NotesService.cs
│  ├─ SearchService.cs
│  ├─ PrintService.cs
│  ├─ UserService.cs
│  └─ ActivityLogService.cs
└─ Interfaces
   ├─ INotesService.cs
   ├─ IAttachmentStorage.cs
   ├─ IPrintService.cs
   ├─ IUserService.cs
   ├─ ICurrentUserService.cs
   └─ IActivityLogService.cs
```

## Zmiana wersji aplikacji

⚠️ **ZAWSZE przy zmianie wersji aktualizuj OBA miejsca jednocześnie:**

1. `UwagiDoDokumentow.App.csproj` — właściwości `<Version>`, `<AssemblyVersion>`, `<FileVersion>`
2. `Properties/AssemblyInfo.cs` — atrybuty `[AssemblyVersion]`, `[AssemblyFileVersion]`,
   `[AssemblyInformationalVersion]`

**Dlaczego:** jeśli projekt ma `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`, atrybuty
z `AssemblyInfo.cs` mają pierwszeństwo nad `.csproj`. WPF generuje pack URI z wersją z `.csproj`,
ale assembly w runtime ma wersję z `AssemblyInfo.cs` — niezgodność powoduje `FileNotFoundException`
przy starcie aplikacji.

Po każdej zmianie wersji wymagane jest `dotnet clean` + `dotnet build` (pełna rekompilacja).

## Komendy projektu

```bash
# Budowanie projektu
dotnet build

# Uruchomienie (bezpośrednio przez .exe — dotnet run nie działa poprawnie z WPF pack URI)
.\UwagiDoDokumentow.App\bin\Debug\net10.0-windows\UwagiDoDokumentow.App.exe

# Migracje EF Core
dotnet ef migrations add <Nazwa> --project UwagiDoDokumentow.Infrastructure --startup-project UwagiDoDokumentow.App
dotnet ef database update --project UwagiDoDokumentow.Infrastructure --startup-project UwagiDoDokumentow.App

# Testy (gdy dodane)
dotnet test
```

## Co odpuszczamy na start (Etap 2, nie MVP)
W przeciwieństwie do pierwotnej propozycji, logowanie i uprawnienia **nie** są tu odpuszczane —
to wymaganie od wersji 1. Nadal odkładamy na później:
- SQLite FTS5 dla pełnotekstowego wyszukiwania (dodaj, gdy liczba rekordów faktycznie to uzasadni)
- OCR załączników
- historię zmian pojedynczej uwagi (kto co zmienił w treści — dziś tylko `updated_by`/`updated_at`)
- masowe importy
- statusy uwag / przypinanie ważnych
- backup i restore z poziomu UI (na start: ręczna kopia pliku `notes.db` + katalogu `attachments`)
