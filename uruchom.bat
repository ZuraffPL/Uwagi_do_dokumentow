@echo off
setlocal
chcp 65001 > nul
cd /d "%~dp0"

set "SOLUTION=UwagiDoDokumentow.slnx"
set "EXE_PATH=UwagiDoDokumentow.App\bin\Debug\net10.0-windows\UwagiDoDokumentow.App.exe"

echo ============================================
echo   Uwagi do dokumentow - uruchamianie
echo ============================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo BLAD: Nie znaleziono polecenia "dotnet". Zainstaluj .NET SDK 10 i sprobuj ponownie.
    pause
    exit /b 1
)

echo Budowanie aplikacji ^(dotnet build^)...
dotnet build "%SOLUTION%" -c Debug
if errorlevel 1 (
    echo.
    echo BLAD: Budowanie nie powiodlo sie. Sprawdz powyzsze komunikaty.
    pause
    exit /b 1
)

if not exist "%EXE_PATH%" (
    echo BLAD: Nie znaleziono pliku "%EXE_PATH%" po zbudowaniu.
    pause
    exit /b 1
)

echo.
echo Uruchamianie aplikacji...
start "" "%EXE_PATH%"

endlocal
