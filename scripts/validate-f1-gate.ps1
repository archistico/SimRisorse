param(
    [int]$ExpectedTests = 926,
    [int]$StressSeconds = 120,
    [switch]$Combined24Hours
)

# F1 / RC72 acceptance gate. This is deliberately separate from validate.cmd:
# the historical validation remains unchanged, and the longer 24h soak is opt-in.
# Uses only synthetic audit scenarios and the standard test artifacts; no player saves.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$cache = Join-Path $root '.local-cache\m4'
$started = [DateTime]::UtcNow
$gateDirectory = Join-Path $cache ('f1-gates\' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $gateDirectory -Force | Out-Null
$reportFile = Join-Path $gateDirectory 'f1-gate.json'
$report = [ordered]@{
    milestone = 'F1-D05e / RC78 analytical economic curve comparison (RC72 gate)'
    status = 'incomplete'
    startedUtc = $started.ToString('o')
    expectedTests = $ExpectedTests
    stressSeconds = $StressSeconds
    testResults = $null
    total = $null
    passed = $null
    failed = $null
    skipped = $null
    nativeSmoke = $null
    baselineAudit = $null
    combinedAudit = $null
    recoveryBenchmark = $null
    economicCurveStudy = $null
    longAudit = $null
    error = $null
}

function Assert-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}

function Invoke-Audit {
    param([string]$Name, [string[]]$Arguments, [int]$Seconds, [bool]$Combined)
    $dll = Join-Path $root 'tools\SimRisorse.Simulation\bin\Release\net10.0\SimRisorse.Simulation.dll'
    Assert-Condition (Test-Path -LiteralPath $dll) 'The simulation assembly was not built by validate.cmd.'
    $lines = @(& dotnet $dll @Arguments)
    Assert-Condition ($LASTEXITCODE -eq 0) "Audit $Name failed with a nonzero exit code."
    $raw = $lines -join "`n"
    Assert-Condition (-not [string]::IsNullOrWhiteSpace($raw)) "Audit $Name returned no JSON."
    $result = $raw | ConvertFrom-Json
    Assert-Condition ($result.RequestedSeconds -eq $Seconds) "Audit $Name has the wrong requested duration."
    Assert-Condition ($result.SimulatedSeconds -eq $Seconds) "Audit $Name advanced the wrong number of seconds."
    Assert-Condition ($result.ImbalancedSeconds -eq 0) "Audit $Name detected unbalanced inventories."
    Assert-Condition ($result.LegacyProducedFoodMilliunits -eq 0) "Audit $Name ran the retired food-store recipe."
    Assert-Condition ($result.ServedMeals -ge 0 -and $result.MissedMeals -ge 0 -and
        ($result.ServedMeals + $result.MissedMeals) -eq $Seconds) "Audit $Name has inconsistent meal accounting."
    if ($Combined) {
        foreach ($stage in @('CattleProducedMilliunits', 'GameProducedMilliunits', 'MeatProducedMilliunits',
            'VegetablesProducedMilliunits', 'FruitProducedMilliunits', 'BreadProducedMilliunits',
            'PreparedFoodProducedMilliunits')) {
            Assert-Condition ($result.$stage -gt 0) "Audit $Name did not run production stage $stage."
        }
    }
    else {
        Assert-Condition ($result.PreparedFoodMilliunits -ge 0 -and $result.FishMilliunits -ge 0) `
            "Audit $Name contains a negative physical inventory."
    }
    $path = Join-Path $gateDirectory ("audit-" + $Name + '.json')
    $raw | Set-Content -LiteralPath $path -Encoding UTF8
    return $path
}

try {
    Assert-Condition ($ExpectedTests -ge 1) 'ExpectedTests must be positive.'
    Assert-Condition ($StressSeconds -ge 4 -and $StressSeconds -le 172800) `
        'StressSeconds must be between 4 and 172800.'

    # Always run a fresh complete validation. An old success report is not gate evidence.
    $validationCommand = Join-Path $root 'validate.cmd'
    Assert-Condition (Test-Path -LiteralPath $validationCommand) 'validate.cmd was not found.'
    Push-Location -LiteralPath $root
    try {
        & $validationCommand | Tee-Object -FilePath (Join-Path $gateDirectory 'validate-output.log')
        Assert-Condition ($LASTEXITCODE -eq 0) 'validate.cmd returned a nonzero exit code.'
    }
    finally { Pop-Location }

    $validationFile = Join-Path $cache 'validation.json'
    Assert-Condition (Test-Path -LiteralPath $validationFile) 'Validation did not produce validation.json.'
    Assert-Condition ((Get-Item -LiteralPath $validationFile).LastWriteTimeUtc -ge $started.AddSeconds(-2)) `
        'Validation report is stale; this gate cannot reuse an older success.'
    $validation = Get-Content -LiteralPath $validationFile -Raw | ConvertFrom-Json
    Assert-Condition ($validation.total -eq $ExpectedTests -and $validation.passed -eq $ExpectedTests -and
        $validation.failed -eq 0) 'Validation counters do not match the exact expected green suite.'
    Assert-Condition ($validation.nativeSmoke -like 'Passed on Windows*') `
        'The Windows desktop native smoke was skipped or failed.'
    Assert-Condition (Test-Path -LiteralPath $validation.testResults) 'The TRX results directory is missing.'
    $files = @(Get-ChildItem -LiteralPath $validation.testResults -Filter '*.trx' -File)
    Assert-Condition ($files.Count -ge 3) 'Expected TRX files for all test projects.'
    $total = 0; $passed = 0; $failed = 0; $skipped = 0
    foreach ($file in $files) {
        [xml]$trx = Get-Content -LiteralPath $file.FullName -Raw
        $counters = $trx.TestRun.ResultSummary.Counters
        $total += [int]$counters.total
        $passed += [int]$counters.passed
        $failed += [int]$counters.failed
        $skipped += [int]$counters.notExecuted
    }
    Assert-Condition ($total -eq $ExpectedTests -and $passed -eq $ExpectedTests -and
        $failed -eq 0 -and $skipped -eq 0) 'TRX counters show missing, skipped or failing tests.'
    $report.testResults = $validation.testResults
    $report.total = $total
    $report.passed = $passed
    $report.failed = $failed
    $report.skipped = $skipped
    $report.nativeSmoke = $validation.nativeSmoke
    foreach ($screenshot in @($validation.screenshots)) {
        Assert-Condition (Test-Path -LiteralPath $screenshot) "A desktop screenshot is missing: $screenshot"
    }

    $report.baselineAudit = Invoke-Audit -Name 'baseline' -Seconds $StressSeconds -Combined $false `
        -Arguments @('--audit-v15', '--seconds', $StressSeconds.ToString())
    $report.combinedAudit = Invoke-Audit -Name 'combined' -Seconds $StressSeconds -Combined $true `
        -Arguments @('--audit-v15', '--seconds', $StressSeconds.ToString(), '--profile', 'combined')

    # RC77: run the actual CLI and preserve complete, fresh controlled evidence.
    $recoveryDirectory = Join-Path $gateDirectory 'industrial-recovery'
    $simulation = Join-Path $root 'tools\SimRisorse.Simulation\bin\Release\net10.0\SimRisorse.Simulation.dll'
    $recoveryLines = @(& dotnet $simulation --benchmark-e4a-v15-recovery --output $recoveryDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'Controlled recovery benchmark failed.'
    # Windows PowerShell 5.1 preserves the JSON array as one pipeline object;
    # wrapping that pipeline in @() would introduce a second array level.
    $recoveryRuns = ($recoveryLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($recoveryRuns.Count -eq 6) 'Expected all six controlled recovery runs.'
    foreach ($run in $recoveryRuns) {
        Assert-Condition ($run.Fixture -eq 'controlled/declared-grants' -and -not $run.Diagnostic -and
            $run.DeclaredHorizonSeconds -eq 120 -and $run.EffectiveHorizonSeconds -eq 120) `
            'Recovery benchmark has incorrect provenance or duration.'
        Assert-Condition ($run.MealsServed -eq 120 -and $run.MealsMissed -eq 0) `
            'Recovery fixture was not fed for every measured second.'
        Assert-Condition (@($run.Actions | Where-Object { -not $_.Succeeded }).Count -eq 0) `
            'A controlled recovery action failed.'
        foreach ($total in $run.Totals) {
            Assert-Condition ($total.NominalMilliunits -eq ($total.ProducedMilliunits +
                $total.StorageOnlyUnproducedMilliunits + $total.InputsOnlyUnproducedMilliunits +
                $total.MixedUnproducedMilliunits + $total.OtherUnproducedMilliunits)) `
                'Production-loss categories do not reconcile.'
        }
    }
    foreach ($name in @('e4a-v15-recovery-results.json', 'e4a-v15-recovery-samples.csv',
        'e4a-v15-recovery-actions.csv', 'e4a-v15-recovery.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $recoveryDirectory $name)) `
            "A controlled recovery report is missing: $name"
    }
    $report.recoveryBenchmark = Join-Path $recoveryDirectory 'e4a-v15-recovery-results.json'

    # RC78: isolated analytical proposals; never install them in a live catalog.
    $studyDirectory = Join-Path $gateDirectory 'economic-curves'
    $studyLines = @(& dotnet $simulation --study-e4b-curves --output $studyDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'Economic curve study failed.'
    $study = ($studyLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($study.Mode -eq 'analytical-proposal/not-applied' -and
        $study.Curves -eq 3 -and $study.Quotes -eq 81 -and $study.Investments -eq 18 -and
        $study.MeasuredBaselineRuns -eq 0) 'Economic study has inconsistent scope or invented baseline.'
    foreach ($name in @('e4b-curves.json', 'e4b-quotes.csv', 'e4b-investments.csv',
        'e4b-curves.md', 'baseline-source.json')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $studyDirectory $name)) "Study output missing: $name"
    }
    $report.economicCurveStudy = Join-Path $studyDirectory 'e4b-curves.json'

    if ($Combined24Hours) {
        $report.longAudit = Invoke-Audit -Name 'combined-24h' -Seconds 86400 -Combined $true `
            -Arguments @('--audit-v15', '--hours', '24', '--profile', 'combined')
    }
    $report.status = 'passed'
    $report.finishedUtc = [DateTime]::UtcNow.ToString('o')
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportFile -Encoding UTF8
    Write-Host "F1 GATE PASSED: $ExpectedTests/$ExpectedTests tests, baseline + combined audit, controlled recovery, economic study."
    Write-Host "Evidence: $reportFile"
}
catch {
    $report.status = 'failed'
    $report.error = $_.Exception.Message
    $report.finishedUtc = [DateTime]::UtcNow.ToString('o')
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportFile -Encoding UTF8
    Write-Error "F1 GATE FAILED: $($report.error). Evidence: $reportFile"
    exit 1
}
