@echo off
setlocal
set AVALONIA_TELEMETRY_OPTOUT=1
pushd "%~dp0..\experiments\stack-probe"
dotnet run --project Desktop\SimRisorse.StackProbe.Desktop.csproj --configuration Release --no-restore -- %*
set probeExitCode=%errorlevel%
popd
exit /b %probeExitCode%
