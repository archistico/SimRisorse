param(
    [int]$ExpectedTests = 885,
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
    milestone = 'F1-D05e / RC76-HF04 icons, header and twentyfold v15 growth (RC72 gate)'
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

    if ($Combined24Hours) {
        $report.longAudit = Invoke-Audit -Name 'combined-24h' -Seconds 86400 -Combined $true `
            -Arguments @('--audit-v15', '--hours', '24', '--profile', 'combined')
    }
    $report.status = 'passed'
    $report.finishedUtc = [DateTime]::UtcNow.ToString('o')
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportFile -Encoding UTF8
    Write-Host "F1 GATE PASSED: $ExpectedTests/$ExpectedTests tests, baseline + combined audit."
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
