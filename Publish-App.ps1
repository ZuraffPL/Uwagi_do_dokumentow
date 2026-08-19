[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir
)

$ErrorActionPreference = "Stop"

# $PSScriptRoot bywa pusty w domyślnych wartościach parametrów przy niektórych sposobach
# wywołania (np. powershell.exe -File z Windows PowerShell 5.1) - dlatego katalog skryptu
# wyliczamy tutaj, z zapasowym $MyInvocation, zamiast polegać na $PSScriptRoot w param().
$scriptDir = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($scriptDir)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $scriptDir "publish"
}

$projectPath = Join-Path $scriptDir "UwagiDoDokumentow.App\UwagiDoDokumentow.App.csproj"

Write-Host "Odczytywanie wersji aplikacji z .csproj..." -ForegroundColor Cyan
[xml]$projectXml = Get-Content $projectPath
$appVersion = $projectXml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($appVersion)) {
    Write-Error "Nie udalo sie odczytac <Version> z $projectPath."
    exit 1
}
Write-Host "Wersja aplikacji: $appVersion" -ForegroundColor Cyan

Write-Host "Czyszczenie katalogu publikacji: $OutputDir" -ForegroundColor Cyan
$dataBackupPath = $null
$existingDataPath = Join-Path $OutputDir "Data"
if (Test-Path $existingDataPath) {
    Write-Host "Zachowuje istniejacy folder Data (baza danych + zalaczniki) przed czyszczeniem..." -ForegroundColor Yellow
    $dataBackupPath = Join-Path $env:TEMP ("UwagiDoDokumentow_DataBackup_" + [Guid]::NewGuid())
    Move-Item -Path $existingDataPath -Destination $dataBackupPath
}
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}

Write-Host "Publikowanie aplikacji (self-contained, $Runtime, $Configuration)..." -ForegroundColor Cyan
dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $OutputDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish zakonczyl sie bledem (kod $LASTEXITCODE)."
    exit $LASTEXITCODE
}

if ($dataBackupPath -and (Test-Path $dataBackupPath)) {
    Write-Host "Przywracanie folderu Data do katalogu publikacji..." -ForegroundColor Yellow
    Move-Item -Path $dataBackupPath -Destination (Join-Path $OutputDir "Data")
}

Write-Host "Szukanie kompilatora Inno Setup 6 (ISCC.exe)..." -ForegroundColor Cyan
$innoCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)
$iscc = $innoCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Error "Nie znaleziono ISCC.exe (Inno Setup 6). Zainstaluj Inno Setup 6 z https://jrsoftware.org/isdl.php i sprobuj ponownie."
    exit 1
}

Write-Host "Kompilacja instalatora Inno Setup 6..." -ForegroundColor Cyan
$issPath = Join-Path $scriptDir "Installer\Setup.iss"
& $iscc "/DMyAppVersion=$appVersion" $issPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Kompilacja instalatora zakonczyla sie bledem (kod $LASTEXITCODE)."
    exit $LASTEXITCODE
}

Write-Host "Gotowe. Instalator znajduje sie w katalogu glownym repozytorium." -ForegroundColor Green
