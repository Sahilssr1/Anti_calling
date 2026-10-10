@echo off
chcp 65001 >nul
set PYTHONUTF8=1
cd /d %~dp0

set "PY_EXE=python"
where python >nul 2>nul
if errorlevel 1 (
    if exist "%LOCALAPPDATA%\Programs\Python\Python311\python.exe" (
        set "PY_EXE=%LOCALAPPDATA%\Programs\Python\Python311\python.exe"
    )
)

echo Starting AnyDesk Voice Listener...
"%PY_EXE%" server.py
pause

