@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\validate-f1-gate.ps1" %*
exit /b %errorlevel%
