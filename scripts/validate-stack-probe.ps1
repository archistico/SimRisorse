param(
    [string]$OfflinePackageSource = '',
    [switch]$SkipNativeSmoke
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$probeDirectory = Join-Path $repositoryRoot 'experiments/stack-probe'
$artifactDirectory = Join-Path $repositoryRoot '.local-cache/stack-probe'
$packageDirectory = Join-Path $repositoryRoot '.local-cache/nuget'
$publishDirectory = Join-Path $artifactDirectory 'publish'
$previousTelemetrySetting = $env:AVALONIA_TELEMETRY_OPTOUT
$previousArtifactSetting = $env:SIMRISORSE_PROBE_ARTIFACTS

function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed with exit code $LASTEXITCODE" }
}

New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$env:AVALONIA_TELEMETRY_OPTOUT = '1'
$env:SIMRISORSE_PROBE_ARTIFACTS = $artifactDirectory
Push-Location -LiteralPath $probeDirectory
try {
    $restoreArguments = @('restore', 'SimRisorse.StackProbe.slnx', '--locked-mode', '--packages', $packageDirectory, '--verbosity', 'minimal')
    if ($OfflinePackageSource) {
        $restoreArguments += @('--source', $OfflinePackageSource, '-p:NuGetAudit=false')
    }
    Invoke-Dotnet -Arguments $restoreArguments
    Invoke-Dotnet -Arguments @('build', 'SimRisorse.StackProbe.slnx', '--no-restore', '-c', 'Release', '--verbosity', 'minimal')
    Invoke-Dotnet -Arguments @('test', 'SimRisorse.StackProbe.slnx', '--no-restore', '--no-build', '-c', 'Release',
        '--logger', 'trx;LogFileName=stack-probe.trx', '--results-directory', (Join-Path $artifactDirectory 'test-results'), '--verbosity', 'minimal')
    Invoke-Dotnet -Arguments @('publish', 'Desktop/SimRisorse.StackProbe.Desktop.csproj', '--no-restore', '--no-build', '-c', 'Release',
        '--self-contained', 'false', '--output', $publishDirectory, '--verbosity', 'minimal')

    $nativeSmoke = 'Not run'
    if (-not $SkipNativeSmoke -and [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        $executable = Join-Path $publishDirectory 'SimRisorse.StackProbe.Desktop.exe'
        $process = Start-Process -FilePath $executable -ArgumentList '--smoke-test' -WorkingDirectory $publishDirectory -WindowStyle Hidden -PassThru
        if (-not $process.WaitForExit(10000)) {
            Stop-Process -Id $process.Id
            throw 'Native desktop smoke test timed out.'
        }
        $process.Refresh()
        if ($process.ExitCode -ne 0) { throw "Native desktop smoke test failed: $($process.ExitCode)" }
        $nativeSmoke = 'Passed on Windows'
    }
    [xml]$testResults = Get-Content -LiteralPath (Join-Path $artifactDirectory 'test-results/stack-probe.trx') -Raw
    $counters = $testResults.TestRun.ResultSummary.Counters
    $report = [ordered]@{
        sdk = (& dotnet --version)
        total = [int]$counters.total
        passed = [int]$counters.passed
        failed = [int]$counters.failed
        nativeSmoke = $nativeSmoke
        offlineRestore = [bool]$OfflinePackageSource
        dependencyAuditExecuted = (-not [bool]$OfflinePackageSource)
        frameworkDependentPublish = $true
        screenshots = @('main-window.png', 'compact-window.png')
    }
    $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $artifactDirectory 'validation.json') -Encoding UTF8
    $report | ConvertTo-Json
}
finally {
    Pop-Location
    $env:AVALONIA_TELEMETRY_OPTOUT = $previousTelemetrySetting
    $env:SIMRISORSE_PROBE_ARTIFACTS = $previousArtifactSetting
}
