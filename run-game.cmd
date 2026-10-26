@echo off
setlocal
set AVALONIA_TELEMETRY_OPTOUT=1
pushd "%~dp0"
dotnet run --project src\SimRisorse.Desktop --configuration Release --no-restore -- %*
set gameExitCode=%errorlevel%
popd
exit /b %gameExitCode%
