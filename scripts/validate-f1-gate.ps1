param(
    [int]$ExpectedTests = 1233,
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
    milestone = 'RC94.2 / mint-crew coal delta test hotfix; RC94.1 1228/1233 passed; RC94 shadow remains sandbox (RC93.1 green baseline 1220)'
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
    financingFeasibilityStudy = $null
    crewOperationalStudy = $null
    mintNaturalDiagnostic = $null
    mintScaleRiskStudy = $null
    priceReadinessStudy = $null
    curveAFundingSandbox = $null
    v15FundingShadow = $null
    mintFeasibilityAudit = $null
    mintHousingBufferReview = $null
    firstResearchBootstrapAudit = $null
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

    # RC79: opt-in financing feasibility. No real escrow or revised price is enabled.
    $financingDirectory = Join-Path $gateDirectory 'construction-financing'
    $financingLines = @(& dotnet $simulation --study-e4b-financing --output $financingDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'Construction financing study failed.'
    $financing = ($financingLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($financing.Mode -eq 'analytical-proposal/not-applied' -and
        $financing.Rows -eq 27 -and $financing.Options -eq 'atomic/sequential/escrow') `
        'Financing study has the wrong scope or was presented as live.'
    foreach ($name in @('e4b-financing.json', 'e4b-financing.csv', 'e4b-financing.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $financingDirectory $name)) `
            "Financing study output missing: $name"
    }
    $report.financingFeasibilityStudy = Join-Path $financingDirectory 'e4b-financing.json'

    # RC80: the real mint recipe through the existing planner, but crew size 10
    # and 1/3/10 modules in a controlled, NOT PLAYABLE projection.
    $crewDirectory = Join-Path $gateDirectory 'mint-crews'
    $crewLines = @(& dotnet $simulation --study-e4b-crews --output $crewDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'Crew projection study failed.'
    $crew = ($crewLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($crew.Mode -eq 'controlled-projection/not-applied' -and
        $crew.CrewSize -eq 10 -and $crew.Rows -eq 10) `
        'Crew projection has incorrect size, scope or mode.'
    foreach ($name in @('e4b-crews.json', 'e4b-crews.csv', 'e4b-crews.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $crewDirectory $name)) `
            "Crew projection output missing: $name"
    }
    $report.crewOperationalStudy = Join-Path $crewDirectory 'e4b-crews.json'

    # RC82: cheap, genuine fresh-game MintTen diagnostic, not a fabricated
    # natural completion. This command also compares an identical Legacy start.
    $mintDirectory = Join-Path $gateDirectory 'mint-natural-diagnostic'
    $mintLines = @(& dotnet $simulation --benchmark-e4b-mint-ten --max-seconds 0 `
        --compare-legacy --output $mintDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'MintTen natural diagnostic CLI failed.'
    $mintRuns = ($mintLines -join "`n") | ConvertFrom-Json
    Assert-Condition (@($mintRuns).Count -eq 4) 'MintTen diagnostic must contain two policies for two regimes.'
    foreach ($run in $mintRuns) {
        Assert-Condition ($run.Fixture -eq 'fresh-game/no-grants' -and $run.Diagnostic -and
            $run.EffectiveHorizonSeconds -eq 0 -and $run.Outcome -eq 'timeout' -and
            $null -eq $run.CompletionSeconds) 'MintTen zero-second diagnostic claimed an unobserved result.'
        Assert-Condition ($run.RulesVersion -in @('civil-growth-15', 'civil-growth-15-mint-crew10')) `
            'Unknown rules version in natural MintTen diagnostic.'
        Assert-Condition ($run.InitialPhysicalStockMilliunits.pencils -eq 2000) `
            'RC86 natural MintTen fixture must include exactly two initial Municipio pencils.'
    }
    foreach ($name in @('results.json', 'events.csv', 'samples.csv', 'analysis.json',
        'analysis.csv', 'analysis.md', 'constraints.json', 'constraints.csv', 'constraints.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $mintDirectory ("e4b-mint-ten-" + $name))) `
            "MintTen diagnostic output missing: $name"
    }
    $report.mintNaturalDiagnostic = Join-Path $mintDirectory 'e4b-mint-ten-results.json'

    # RC83: only an analytical counterfactual. A route failure is not a natural softlock.
    $scaleDirectory = Join-Path $gateDirectory 'mint-scale-risk'
    $scaleLines = @(& dotnet $simulation --study-e4b-mint-scale --output $scaleDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'Two-mint scaling risk study failed.'
    $scale = ($scaleLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($scale.Mode -eq 'analytical-proposal/not-applied' -and
        $scale.Rows -eq 9 -and $scale.Curves -eq 3 -and $scale.Tiers -eq 3 -and
        $scale.StorageScope -like 'warehouse-only*') `
        'Mint scale comparison has inconsistent scope or was presented as gameplay.'
    foreach ($name in @('e4b-mint-scale.json', 'e4b-mint-scale.csv', 'e4b-mint-scale.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $scaleDirectory $name)) `
            "Mint scale output missing: $name"
    }
    $report.mintScaleRiskStudy = Join-Path $scaleDirectory 'e4b-mint-scale.json'

    # RC84: independently re-analyze the RC82 raw diagnostic JSON. This MUST NOT
    # claim a natural completion, enable curve A/B/C or advance a campaign clock.
    $feasibilityDirectory = Join-Path $gateDirectory 'mint-natural-feasibility'
    $feasibilityLines = @(& dotnet $simulation --analyze-e4b-mint-ten `
        --input $report.mintNaturalDiagnostic --output $feasibilityDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC84 offline natural mint feasibility audit failed.'
    $feasibility = ($feasibilityLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($feasibility.ObservedRuns -eq 4 -and
        $feasibility.DiagnosticRuns -eq 4 -and $feasibility.FullHorizonRuns -eq 0 -and
        $feasibility.VerifiedMintTenMilestones -eq 0 -and
        -not $feasibility.CompleteNaturalPolicyPairObserved -and
        $feasibility.Hypotheses -eq 9 -and
        $feasibility.ProposalScope -like '*not-applied*' -and
        $feasibility.InputSha256 -match '^[0-9a-f]{64}$') `
        'RC84 audit conflated diagnostic evidence with natural completion or active pricing.'
    foreach ($name in @('e4b-mint-feasibility.json', 'e4b-mint-feasibility.csv',
        'e4b-mint-feasibility.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $feasibilityDirectory $name)) `
            "RC84 evidence file missing: $name"
    }
    $report.mintFeasibilityAudit = Join-Path $feasibilityDirectory 'e4b-mint-feasibility.json'

    # RC85: three distinct natural MintTen scripted strategies and a matching
    # Legacy control. All six zero-second runs are DIAGNOSTIC, not evidence
    # of actual workforce feasibility. No player save is opened.
    $housingDirectory = Join-Path $gateDirectory 'mint-housing-buffer'
    $housingLines = @(& dotnet $simulation --benchmark-e4b-mint-ten --max-seconds 0 `
        --include-housing-buffer --compare-legacy --output $housingDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC85 MintTen housing-buffer diagnostic failed.'
    $housingRuns = ($housingLines -join "`n") | ConvertFrom-Json
    Assert-Condition (@($housingRuns).Count -eq 6) 'RC85 expected three policies in each of two workforce regimes.'
    Assert-Condition (@($housingRuns | Where-Object { $_.PolicyId -eq 'housing-buffer' }).Count -eq 2) `
        'RC85 housing-buffer must appear once in each saved workforce regime.'
    foreach ($run in $housingRuns) {
        Assert-Condition ($run.Fixture -eq 'fresh-game/no-grants' -and $run.Diagnostic -and
            $run.EffectiveHorizonSeconds -eq 0 -and $run.Outcome -eq 'timeout' -and
            $null -eq $run.CompletionSeconds) 'RC85 zero-second diagnostic reported an unobserved success.'
    }
    $housingResults = Join-Path $housingDirectory 'e4b-mint-ten-results.json'
    Assert-Condition (Test-Path -LiteralPath $housingResults) 'RC85 raw natural evidence JSON missing.'
    $housingReviewDirectory = Join-Path $gateDirectory 'mint-housing-review'
    $housingReviewLines = @(& dotnet $simulation --review-e4b-housing-buffer `
        --input $housingResults --output $housingReviewDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC85 independent housing-buffer review failed.'
    $housingReview = ($housingReviewLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($housingReview.NaturalRunCount -eq 6 -and
        $housingReview.PairedContracts -eq 2 -and
        $housingReview.ComparableCompletedContracts -eq 0 -and
        $housingReview.InputSha256 -match '^[0-9a-f]{64}$') `
        'RC85 housing review misrepresented diagnostic data as completed milestones.'
    foreach ($name in @('e4b-housing-buffer.json', 'e4b-housing-buffer.csv',
        'e4b-housing-buffer.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $housingReviewDirectory $name)) `
            "RC85 housing review output missing: $name"
    }
    $report.mintHousingBufferReview = Join-Path $housingReviewDirectory 'e4b-housing-buffer.json'

    # RC87: first research unlock is checked without downstream coal, and
    # with actual starter pencils. This fast gate is DIAGNOSTIC (0 seconds).
    # It must never claim that the full natural campaign has completed.
    $researchDirectory = Join-Path $gateDirectory 'research-bootstrap'
    $researchLines = @(& dotnet $simulation --audit-v15-research-bootstrap `
        --max-seconds 0 --output $researchDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC87 research bootstrap command failed.'
    $research = ($researchLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($research.Version -eq 'rc87-bootstrap-evidence-1' -and
        $research.EvidenceScope -eq 'natural-v15/real-receipts/no-grants' -and
        $research.RunCount -eq 2 -and $research.VerifiedRuns -eq 0 -and
        $research.StartingPencilsMilliunits -eq 2000 -and
        $research.PencilsPerStudentMilliunits -eq 2000 -and
        $research.ExtractionCostMilliPoints -eq 150000 -and
        $null -eq $research.BreadMinusResearchSeconds) `
        'RC87 research audit has invalid provenance, prices or evidence.'
    foreach ($policy in @('research-first', 'bread-first')) {
        $row = @($research.Outcomes | Where-Object { $_.PolicyId -eq $policy })
        Assert-Condition ($row.Count -eq 1 -and $row[0].Diagnostic -and
            $row[0].Outcome -eq 'timeout' -and -not $row[0].Verified) `
            "RC87 missing or invalid diagnostic outcome: $policy"
    }
    foreach ($name in @('rc87-research-bootstrap-results.json', 'rc87-research-bootstrap-audit.json',
        'rc87-research-bootstrap.csv', 'rc87-research-bootstrap.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $researchDirectory $name)) `
            "RC87 research evidence file missing: $name"
    }
    $report.firstResearchBootstrapAudit = Join-Path $researchDirectory 'rc87-research-bootstrap-audit.json'

    # RC88: seven zero-second real no-grant diagnostic runs (swords, coins,
    # legacy/MintTen, three MintTen policies). No synthetic completion accepted.
    $industrialChainDirectory = Join-Path $gateDirectory 'rc88-industrial-chain'
    $industrialChainLines = @(& dotnet $simulation --audit-v15-industry-chain `
        --max-seconds 0 --output $industrialChainDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC88 industrial chain diagnostic failed.'
    $industrialChain = ($industrialChainLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($industrialChain.Version -eq 'rc88-industrial-chain-evidence-1' -and
        $industrialChain.EvidenceScope -eq 'natural-v15/command-and-clock-receipts/no-grants' -and
        $industrialChain.RunCount -eq 7 -and $industrialChain.DiagnosticRuns -eq 7 -and
        $industrialChain.VerifiedRuns -eq 0) 'RC88 industrial chain report claimed unsupported success.'
    foreach ($row in @($industrialChain.Outcomes)) {
        Assert-Condition ($row.Diagnostic -and $row.Outcome -eq 'timeout' -and -not $row.Verified) `
            'RC88 zero-second sample must remain non-conclusive.'
    }
    Assert-Condition (@($industrialChain.Outcomes | Where-Object {
        $_.ScenarioId -eq 'V15-I02' -and $_.RulesVersion -eq 'civil-growth-15' }).Count -eq 2) `
        'RC88 expected two Forge policy diagnostics.'
    Assert-Condition (@($industrialChain.Outcomes | Where-Object {
        $_.ScenarioId -eq 'V15-I03' -and $_.RulesVersion -eq 'civil-growth-15' }).Count -eq 2) `
        'RC88 expected two Legacy Mint policy diagnostics.'
    Assert-Condition (@($industrialChain.Outcomes | Where-Object {
        $_.ScenarioId -eq 'V15-I03' -and $_.RulesVersion -eq 'civil-growth-15-mint-crew10' }).Count -eq 3) `
        'RC88 expected three MintTen policy diagnostics.'
    foreach ($name in @('rc88-industry-chain-results.json', 'rc88-industry-chain-audit.json',
        'rc88-industry-chain.csv', 'rc88-industry-chain.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $industrialChainDirectory $name)) `
            "RC88 report file missing: $name"
    }
    $industrialOfflineLines = @(& dotnet $simulation --audit-v15-industry-chain `
        --input (Join-Path $industrialChainDirectory 'rc88-industry-chain-results.json'))
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC88 industrial chain offline JSON replay failed.'
    $industrialOffline = ($industrialOfflineLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($industrialOffline.RunCount -eq 7 -and
        $industrialOffline.DiagnosticRuns -eq 7 -and $industrialOffline.VerifiedRuns -eq 0) `
        'RC88 offline run changed the evidence classification.'
    $report.industrialChainAudit = Join-Path $industrialChainDirectory 'rc88-industry-chain-audit.json'

    # RC89: pre-activation review must import the RC88 zero-second receipts,
    # never pretend that A/B/C costs were tested in a live campaign.
    $pricingDirectory = Join-Path $gateDirectory 'rc89-price-readiness'
    $pricingLines = @(& dotnet $simulation --study-e4b-price-readiness `
        --input (Join-Path $industrialChainDirectory 'rc88-industry-chain-results.json') `
        --output $pricingDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC89 pricing readiness calculation failed.'
    $pricing = ($pricingLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($pricing.Version -eq 'rc89-e4b-price-readiness-1' -and
        $pricing.Mode -eq 'analytical-proposal/not-applied' -and
        $pricing.ApprovalStatus -eq 'not-approved/pricing-and-financing-policy-pending' -and
        $pricing.Rows -eq 9 -and $pricing.Curves -eq 3 -and $pricing.Tiers -eq 3 -and
        $pricing.ImportedRuns -eq 7 -and $pricing.DiagnosticRuns -eq 7 -and
        $pricing.VerifiedNaturalRuns -eq 0 -and $pricing.VerifiedMintTenRuns -eq 0 -and
        $pricing.InputSha256 -match '^[0-9a-f]{64}$') `
        'RC89 proposal confused diagnostics with validated economic gameplay.'
    foreach ($name in @('rc89-price-readiness.json', 'rc89-price-readiness.csv',
        'rc89-price-readiness.md', 'rc89-source.json')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $pricingDirectory $name)) `
            "RC89 pricing report missing: $name"
    }
    $report.priceReadinessStudy = Join-Path $pricingDirectory 'rc89-price-readiness.json'

    # RC92: all transfers are in-memory, hypothetical and NEVER applied to
    # live prices, the v15 city, workers or SQLite. Verify all six sandbox
    # settlements, the CSV/MD evidence and explicit unapproved provenance.
    $fundingDirectory = Join-Path $gateDirectory 'rc92-curve-a-funding'
    $fundingLines = @(& dotnet $simulation --study-e4b-curve-a-funding --output $fundingDirectory)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC92 Curve A funding sandbox failed.'
    $funding = ($fundingLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($funding.Version -eq 'rc92-curve-a-funding-1' -and
        $funding.Mode -eq 'curve-A/opt-in/in-memory-only/not-playable' -and
        $funding.InputProvenance -eq 'hypothetical-material-deliveries/no-natural-clock/no-live-prices' -and
        $funding.Rows -eq 6 -and $funding.Reconciled -eq 6 -and
        $funding.ApprovalStatus -like 'not-approved/*') `
        'RC92 incorrectly presents hypothetical funding as approved or natural gameplay.'
    foreach ($name in @('rc92-curve-a-funding.json', 'rc92-curve-a-funding.csv',
        'rc92-curve-a-funding.md')) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $fundingDirectory $name)) `
            "RC92 report file missing: $name"
    }
    $report.curveAFundingSandbox = Join-Path $fundingDirectory 'rc92-curve-a-funding.json'

    # RC94: an isolated, real-v15-checkpoint shadow. Must never grant
    # constructions or modify player saves, even on a fully valid replay.
    $shadowLines = @(& dotnet $simulation --study-e4b-v15-shadow)
    Assert-Condition ($LASTEXITCODE -eq 0) 'RC94 composite shadow CLI failed.'
    $shadow = ($shadowLines -join "`n") | ConvertFrom-Json
    Assert-Condition ($shadow.Version -eq 'rc94-v15-shadow-1' -and
        $shadow.Mode -like '*not-playable*' -and
        $shadow.InitialRevision -eq 0 -and
        $shadow.DepositedRevision -eq 2 -and
        $shadow.RefundedRevision -eq 3 -and
        $shadow.ShadowDepositReconciles -eq $true -and
        $shadow.FrozenQuoteSurvivesRestore -eq $true -and
        $shadow.CityAndEscrowMatch -eq $true -and
        $shadow.RefundRestoresInventory -eq $true -and
        $shadow.LiveSnapshotUnmodified -eq $true -and
        $shadow.NoConstructionGranted -eq $true -and
        $shadow.ApprovalStatus -like 'not-approved*') 'RC94 v15 shadow contract did not reconcile.'
    $report.v15FundingShadow = Join-Path $gateDirectory 'rc94-v15-shadow.json'
    ($shadowLines -join "`n") | Set-Content -LiteralPath $report.v15FundingShadow -Encoding UTF8


    if ($Combined24Hours) {
        $report.longAudit = Invoke-Audit -Name 'combined-24h' -Seconds 86400 -Combined $true `
            -Arguments @('--audit-v15', '--hours', '24', '--profile', 'combined')
    }
    $report.status = 'passed'
    $report.finishedUtc = [DateTime]::UtcNow.ToString('o')
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportFile -Encoding UTF8
    Write-Host "F1 GATE PASSED: $ExpectedTests/$ExpectedTests tests, baseline + combined audit, controlled recovery, economic/financing/crew studies MintTen diagnostic two-mint scaling risk study RC84 offline evidence audit and RC85 MintTen housing review RC94.1 municipal 100 coal and 2 pencils bootstrap RC87 research receipt audit and RC88 industrial clock-receipt audit plus RC89 pricing readiness and RC89.1 per-source flow tooltips, RC90 food depot tiers and RC90.3 technology glyph rows plus RC91 sector building filters RC92 non-playable Curve A escrow and RC93 isolated transactional SQLite replay tests, and RC94 composite v15-shadow checkpoint reconciliation."
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
