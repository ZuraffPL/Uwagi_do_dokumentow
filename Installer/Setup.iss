; Instalator aplikacji "Uwagi do dokumentow" -- Inno Setup 6
;
; WAZNE: AppId ponizej MUSI pozostac NIEZMIENIONY we wszystkich kolejnych wersjach.
; Inno Setup natywnie zapamietuje katalog instalacji wybrany przy PIERWSZEJ instalacji
; (poprzez UsePreviousAppDir, powiazane z AppId w rejestrze) i podstawia go domyslnie
; przy instalacji kolejnych wersji -- dzieki temu operator wskazuje wspolny dysk
; sieciowy raz, a przy aktualizacjach instalator sam trafi w to samo miejsce.
;
#define MyAppName "Uwagi do dokumentow"
; MyAppVersion jest normalnie przekazywana z Publish-App.ps1 (/DMyAppVersion=...), odczytana
; automatycznie z <Version> w UwagiDoDokumentow.App.csproj. Wartosc ponizej to tylko fallback
; na wypadek recznego uruchomienia ISCC.exe bez tego przelacznika -- pamietaj, ze moze sie
; zdezaktualizowac, docelowym zrodlem prawdy jest zawsze .csproj.
#ifndef MyAppVersion
  #define MyAppVersion "1.0.3"
#endif
#define MyAppPublisher "Marcin Zurawicz"
#define MyAppExeName "UwagiDoDokumentow.App.exe"
#define MyPublishDir "..\publish"

[Setup]
AppId={{9B1E5B9E-7C2F-4B1B-9B0B-8A6F2C3D5E01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppVerName={#MyAppName} {#MyAppVersion}
DefaultDirName={autopf}\UwagiDoDokumentow
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Kluczowe dla wymagania "pamietaj katalog instalacji przy kolejnych wersjach":
UsePreviousAppDir=yes
OutputDir=..
OutputBaseFilename=UwagiDoDokumentow_Setup_{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
; Instalacja na dysku sieciowym/wspoldzielonym nie wymaga uprawnien administratora.
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Utworz ikone na pulpicie"; GroupDescription: "Dodatkowe ikony:"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Odinstaluj {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Uruchom {#MyAppName}"; Flags: nowait postinstall skipifsilent
