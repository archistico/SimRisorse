param([string]$OfflinePackageSource = '', [switch]$SkipNativeSmoke)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $repositoryRoot '.local-cache/m3'
$packages = Join-Path $repositoryRoot '.local-cache/nuget'
$runDirectory = Join-Path $artifacts ('runs/' + [Guid]::NewGuid().ToString('N'))
$previousTelemetry = $env:AVALONIA_TELEMETRY_OPTOUT
$previousArtifacts = $env:SIMRISORSE_DESKTOP_ARTIFACTS
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
$env:AVALONIA_TELEMETRY_OPTOUT = '1'
$env:SIMRISORSE_DESKTOP_ARTIFACTS = $artifacts

function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed with exit code $LASTEXITCODE" }
}

Push-Location -LiteralPath $repositoryRoot
try {
    $restore = @('restore', 'SimRisorse.slnx', '--locked-mode', '--packages', $packages, '--verbosity', 'minimal')
    if ($OfflinePackageSource) { $restore += @('--source', $OfflinePackageSource, '-p:NuGetAudit=false') }
    Invoke-Dotnet -Arguments $restore
    Invoke-Dotnet -Arguments @('build', 'SimRisorse.slnx', '--no-restore', '-c', 'Release', '--verbosity', 'minimal')
    Invoke-Dotnet -Arguments @('test', 'SimRisorse.slnx', '--no-restore', '--no-build', '-c', 'Release',
        '--logger', 'trx', '--results-directory', (Join-Path $runDirectory 'test-results'), '--verbosity', 'minimal')

    $simulation = Join-Path $repositoryRoot 'tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll'
    $cases = @(
        @{ Name = '60-seconds'; Arguments = @('--seconds', '60'); Ticks = 60 * [TimeSpan]::TicksPerSecond },
        @{ Name = '8-hours'; Arguments = @('--hours', '8'); Ticks = 8 * [TimeSpan]::TicksPerHour },
        @{ Name = '32-hours'; Arguments = @('--hours', '32'); Ticks = 32 * [TimeSpan]::TicksPerHour }
    )
    foreach ($case in $cases) {
        $arguments = @($simulation) + $case.Arguments
        $name = $case.Name
        $output = & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { throw "Simulation failed: $name" }
        $result = $output | ConvertFrom-Json
        if ($result.ElapsedTicks -ne $case.Ticks) { throw 'Wrong simulation duration.' }
        if ($name -eq '60-seconds') {
            if (($result.Resources | Where-Object Id -eq 'planks').Quantity -ne 179) {
                throw 'The reference minute timeline differs.'
            }
        }
        else {
            foreach ($resource in $result.Resources) {
                if ($resource.Quantity -lt 0 -or $resource.Quantity -gt $resource.Capacity) { throw 'Resource outside capacity.' }
            }
        }
        $output | Set-Content -LiteralPath (Join-Path $artifacts ($name + '.json')) -Encoding UTF8
    }
    $publish = Join-Path $artifacts 'publish'
    Invoke-Dotnet -Arguments @('publish', 'src/SimRisorse.Desktop/SimRisorse.Desktop.csproj', '--no-restore', '--no-build',
        '-c', 'Release', '--self-contained', 'false', '--output', $publish, '--verbosity', 'minimal')
    $nativeSmoke = 'Not run'
    if (-not $SkipNativeSmoke -and [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        $smokePath = Join-Path $runDirectory 'native-save.sqlite3'
        $process = Start-Process -FilePath (Join-Path $publish 'SimRisorse.Desktop.exe') -WindowStyle Hidden -PassThru `
            -ArgumentList @('--smoke-test', '--save-path', ('"' + $smokePath + '"')) -WorkingDirectory $publish `
            -RedirectStandardError (Join-Path $runDirectory 'native-stderr.log') -RedirectStandardOutput (Join-Path $runDirectory 'native-stdout.log')
        # Keep the process handle before it exits; Windows PowerShell otherwise may lose ExitCode.
        $null = $process.Handle
        if (-not $process.WaitForExit(10000)) { Stop-Process -Id $process.Id; throw 'Desktop native smoke timed out.' }
        $nativeExitCode = $process.ExitCode
        $process.Dispose()
        if ($nativeExitCode -ne 0) { Get-Content -LiteralPath (Join-Path $runDirectory 'native-stderr.log'); throw "Desktop native smoke failed: $nativeExitCode" }
        if (-not (Test-Path -LiteralPath $smokePath)) { throw 'Native smoke did not save the game.' }
        $nativeSmoke = 'Passed on Windows: save, load, suspend notification, close'
    }
    $total = $passed = $failed = 0
    Get-ChildItem -LiteralPath (Join-Path $runDirectory 'test-results') -Filter '*.trx' | ForEach-Object {
        [xml]$xml = Get-Content -LiteralPath $_.FullName -Raw
        $total += [int]$xml.TestRun.ResultSummary.Counters.total
        $passed += [int]$xml.TestRun.ResultSummary.Counters.passed
        $failed += [int]$xml.TestRun.ResultSummary.Counters.failed
    }
    $desktopLock = Get-Content -LiteralPath 'src/SimRisorse.Desktop/packages.lock.json' -Raw | ConvertFrom-Json
    $dependencies = $desktopLock.dependencies.'net10.0'
    $report = [ordered]@{
        sdk = (& dotnet --version); total = $total; passed = $passed; failed = $failed
        avaloniaVersion = $dependencies.Avalonia.resolved
        sqliteProviderVersion = $dependencies.'Microsoft.Data.Sqlite'.resolved
        sqliteNativePackageVersion = $dependencies.'SQLitePCLRaw.lib.e_sqlite3'.resolved
        nativeSmoke = $nativeSmoke; offlineRestore = [bool]$OfflinePackageSource
        dependencyAuditRequested = (-not [bool]$OfflinePackageSource)
        testResults = (Join-Path $runDirectory 'test-results')
        screenshots = @('buildings.png', 'buildings-compact.png', 'resources.png', 'resources-compact.png')
    }
    $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $artifacts 'validation.json') -Encoding UTF8
    $report | ConvertTo-Json
}
finally {
    Pop-Location
    $env:AVALONIA_TELEMETRY_OPTOUT = $previousTelemetry
    $env:SIMRISORSE_DESKTOP_ARTIFACTS = $previousArtifacts
}
