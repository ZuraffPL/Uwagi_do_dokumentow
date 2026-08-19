@echo off
chcp 65001 >nul
cd /d "%~dp0"

REM Wlasciwa logika czyszczenia (z zachowaniem folderu Data z baza/zalacznikami)
REM jest w Czysc-Buildy.ps1 - batch jest tylko wygodnym launcherem do klikniecia.
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%~dp0Czysc-Buildy.ps1"

echo.
echo Nacisnij dowolny klawisz, aby zamknac...
pause >nul
