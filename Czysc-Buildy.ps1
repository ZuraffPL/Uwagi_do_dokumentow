[CmdletBinding()]
param()

# Skrypt czyszczacy katalogi bin/obj wszystkich projektow oraz katalog publish,
# z ZACHOWANIEM ewentualnego folderu "Data" (baza SQLite + zalaczniki + logi),
# ktory moze znajdowac sie obok skompilowanego .exe (np. przy uruchamianiu aplikacji
# bezposrednio z katalogu bin/Debug albo z katalogu publish podczas testow).
#
# Uwaga o wielkosci liter: katalog danych nazywa sie "Data" (wielka litera D - patrz
# AppPaths.DataRootDirectory), a WEWNATRZ niego jest jeszcze podkatalog "data" (mala
# litera) z plikiem bazy - dlatego wyszukiwanie folderu do zachowania musi rozrozniac
# wielkosc liter (-cmatch), inaczej zlapaloby tez ten podkatalog jako osobny wpis.
#
# WAZNE: ten plik NIE powinien zawierac polskich znakow diakrytycznych w stringach ani
# komentarzach - Windows PowerShell 5.1 bez BOM w pliku odczytuje go w kodowaniu ANSI
# i wielobajtowe sekwencje UTF-8 (a, e, o, z, l, n, s, c z ogonkami) psuja parsowanie
# skryptu (blad "TerminatorExpectedAtEndOfString" w zupelnie innym miejscu pliku).

$ErrorActionPreference = "Stop"

$scriptDir = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($scriptDir)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
}
Set-Location $scriptDir

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  CZYSZCZENIE: bin, obj, publish, stare instalatory .exe"          -ForegroundColor Cyan
Write-Host "  (folder Data z baza danych i zalacznikami NIE jest usuwany)"     -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

function Clear-DirectoryPreservingData {
    param([string]$TargetPath)

    if (-not (Test-Path $TargetPath)) {
        return
    }

    $dataDirs = Get-ChildItem -Path $TargetPath -Recurse -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -cmatch '^Data$' }

    $backups = @()
    foreach ($dataDir in $dataDirs) {
        # Pomin foldery "Data" zagniezdzone wewnatrz innego juz zabezpieczonego folderu "Data".
        $alreadyCovered = $backups | Where-Object {
            $dataDir.FullName.StartsWith($_.Original + [IO.Path]::DirectorySeparatorChar)
        }
        if ($alreadyCovered) {
            continue
        }

        $backupPath = Join-Path $env:TEMP ("uwagi_data_backup_" + [Guid]::NewGuid())
        Write-Host ("  Zachowuje folder danych: " + $dataDir.FullName) -ForegroundColor Yellow
        Move-Item -Path $dataDir.FullName -Destination $backupPath
        $backups += [PSCustomObject]@{ Original = $dataDir.FullName; Backup = $backupPath }
    }

    Write-Host ("Czyszcze: " + $TargetPath) -ForegroundColor Cyan
    Remove-Item -Path $TargetPath -Recurse -Force

    foreach ($backup in $backups) {
        $parent = Split-Path -Parent $backup.Original
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
        Write-Host ("  Przywracam folder danych: " + $backup.Original) -ForegroundColor Yellow
        Move-Item -Path $backup.Backup -Destination $backup.Original
    }
}

$projects = @(
    "UwagiDoDokumentow.App",
    "UwagiDoDokumentow.Application",
    "UwagiDoDokumentow.Domain",
    "UwagiDoDokumentow.Infrastructure"
)

foreach ($project in $projects) {
    Clear-DirectoryPreservingData (Join-Path $scriptDir "$project\bin")

    $objPath = Join-Path $scriptDir "$project\obj"
    if (Test-Path $objPath) {
        Write-Host ("Usuwam katalog " + $project + "\obj ...") -ForegroundColor Cyan
        Remove-Item -Path $objPath -Recurse -Force
    }
}

Clear-DirectoryPreservingData (Join-Path $scriptDir "publish")

# --- Usuwanie starych instalatorow .exe (zostaw najnowszy wg daty modyfikacji) ---
$installers = Get-ChildItem -Path $scriptDir -Filter "UwagiDoDokumentow_Setup_*.exe" -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending

if ($installers.Count -gt 1) {
    foreach ($old in ($installers | Select-Object -Skip 1)) {
        Write-Host ("Usuwam stary instalator: " + $old.Name) -ForegroundColor Cyan
        Remove-Item $old.FullName -Force
    }
}

Write-Host ""
Write-Host "[OK] Skonczono czyszczenie. Najnowszy instalator zostawiony, folder Data zachowany." -ForegroundColor Green
