param([string]$OfflinePackageSource = '', [switch]$SkipNativeSmoke)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $repositoryRoot '.local-cache/m4'
$packages = Join-Path $repositoryRoot '.local-cache/nuget'
$runDirectory = Join-Path $artifacts ('runs/' + [Guid]::NewGuid().ToString('N'))
$screenshotDirectory = Join-Path $runDirectory 'screenshots'
$previousTelemetry = $env:AVALONIA_TELEMETRY_OPTOUT
$previousArtifacts = $env:SIMRISORSE_DESKTOP_ARTIFACTS
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $screenshotDirectory -Force | Out-Null
$env:AVALONIA_TELEMETRY_OPTOUT = '1'
$env:SIMRISORSE_DESKTOP_ARTIFACTS = $screenshotDirectory

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
    foreach ($scenario in @('economy-foundation', 'civil-growth')) {
        $output = & dotnet $simulation --check-catalog --scenario $scenario
        if ($LASTEXITCODE -ne 0) { $output; throw "Unreachable unlocks in the $scenario catalog." }
        $output | Set-Content -LiteralPath (Join-Path $artifacts ("catalog-$scenario.json")) -Encoding UTF8
    }
    $cases = @(
        @{ Name = '60-seconds'; Arguments = @('--seconds', '60'); Ticks = 60 * [TimeSpan]::TicksPerSecond },
        @{ Name = '8-hours'; Arguments = @('--hours', '8'); Ticks = 8 * [TimeSpan]::TicksPerHour },
        @{ Name = '32-hours'; Arguments = @('--hours', '32'); Ticks = 32 * [TimeSpan]::TicksPerHour },
        @{ Name = 'civil-bootstrap'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'bootstrap', '--seconds', '121'); Ticks = 121 * [TimeSpan]::TicksPerSecond; Population = 9 },
        @{ Name = 'civil-recovery'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'recovery', '--seconds', '662'); Ticks = 662 * [TimeSpan]::TicksPerSecond; Population = 14 },
        @{ Name = 'civil-storage'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'storage', '--seconds', '1260'); Ticks = 1260 * [TimeSpan]::TicksPerSecond; Population = 16 },
        @{ Name = 'civil-bread'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'bread', '--seconds', '660'); Ticks = 660 * [TimeSpan]::TicksPerSecond; Population = 16 },
        @{ Name = 'civil-bread-start'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'bread', '--seconds', '610'); Ticks = 610 * [TimeSpan]::TicksPerSecond; Population = 16 },
        @{ Name = 'civil-advanced'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'advanced', '--seconds', '880'); Ticks = 880 * [TimeSpan]::TicksPerSecond; Population = 18 },
        @{ Name = 'civil-advanced-8-hours'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'advanced', '--hours', '8'); Ticks = 8 * [TimeSpan]::TicksPerHour; Population = 48 },
        @{ Name = 'civil-8-hours'; Arguments = @('--scenario', 'civil-growth', '--hours', '8'); Ticks = 8 * [TimeSpan]::TicksPerHour; Population = 16 },
        @{ Name = 'civil-32-hours'; Arguments = @('--scenario', 'civil-growth', '--hours', '32'); Ticks = 32 * [TimeSpan]::TicksPerHour; Population = 16 },
        @{ Name = 'industry-start'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'industry', '--seconds', '1102'); Ticks = 1102 * [TimeSpan]::TicksPerSecond },
        @{ Name = 'industry-8-hours'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'industry', '--hours', '8'); Ticks = 8 * [TimeSpan]::TicksPerHour },
        @{ Name = 'gold-start'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'gold', '--seconds', '1502'); Ticks = 1502 * [TimeSpan]::TicksPerSecond },
        @{ Name = 'gold-8-hours'; Arguments = @('--scenario', 'civil-growth', '--strategy', 'gold', '--hours', '8'); Ticks = 8 * [TimeSpan]::TicksPerHour }
    )
    foreach ($case in $cases) {
        # Historical trajectories remain reproducible under civil-growth-4;
        # the active civil-growth-7 rules are checked by the industrial and accelerated probes.
        $arguments = @($simulation) + $case.Arguments
        if ($case.Name -like 'civil-*') { $arguments += @('--rules', 'legacy') }
        $name = $case.Name
        $output = & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { throw "Simulation failed: $name" }
        $result = $output | ConvertFrom-Json
        if ($result.ElapsedTicks -ne $case.Ticks) { throw 'Wrong simulation duration.' }
        if ($case.Population -and ($result.Population -ne $case.Population -or $result.PopulationState.ProgressSeconds -ne 0)) {
            throw "Wrong population/progress for $name."
        }
        if ($name -eq '60-seconds') {
            if (($result.Resources | Where-Object Id -eq 'planks').Quantity -ne 179) {
                throw 'The reference minute timeline differs.'
            }
        }
        elseif ($name -eq 'civil-storage') {
            if (($result.Resources | Where-Object Id -eq 'stone').Quantity -ne 1000 -or
                ($result.Resources | Where-Object { $_.Id -ne 'bread' -and $_.Capacity -ne 1500 }) -or
                ($result.Resources | Where-Object Id -eq 'bread').Capacity -ne 600) {
                throw 'The warehouse timeline differs.'
            }
        }
        elseif ($name -eq 'civil-bread-start') {
            $bakery = $result.Buildings | Where-Object Id -eq 'bakery'
            if (($result.Resources | Where-Object Id -eq 'bread').ProductionPerSecond -ne 3 -or $bakery.RunnableWorkers -ne 1) {
                throw 'The initial bread production differs.'
            }
        }
        elseif ($name -eq 'civil-bread') {
            $bread = $result.Resources | Where-Object Id -eq 'bread'
            if ($bread.Quantity -ne 99 -or $bread.Capacity -ne 100 -or
                ($result.Resources | Where-Object Id -eq 'food').ProductionPerSecond -ne 1) {
                throw 'The limited bread storage timeline differs.'
            }
        }
        elseif ($name -eq 'civil-advanced') {
            if ($result.PopulationState.HousingCapacity -ne 48 -or $result.PopulationState.AdvancedResidents -ne 2 -or
                $result.PopulationState.RequiredBreadPerSecond -ne 0.030) { throw 'The advanced housing timeline differs.' }
        }
        elseif ($name -like 'industry-*') {
            $iron = $result.Resources | Where-Object Id -eq 'iron'
            $smelter = $result.Buildings | Where-Object Id -eq 'iron-smelter'
            if ($result.RulesVersion -ne 'civil-growth-7' -or $smelter.Count -ne 1 -or $smelter.AssignedWorkers -ne 1) {
                throw 'The industrial rules, construction or workforce differ.'
            }
            if ($name -eq 'industry-start' -and ($iron.Quantity -ne 1 -or
                ($result.Resources | Where-Object Id -eq 'iron-ore').Quantity -ne 2 -or
                ($result.Resources | Where-Object Id -eq 'coal').Quantity -ne 3)) {
                throw 'The first industrial production differs.'
            }
            if ($name -eq 'industry-8-hours' -and ($iron.Quantity -ne 1000 -or $result.Population -ne 48)) {
                throw 'The long industrial timeline differs.'
            }
        }
        elseif ($name -like 'gold-*') {
            $gold = $result.Resources | Where-Object Id -eq 'gold'
            $smelter = $result.Buildings | Where-Object Id -eq 'gold-smelter'
            if ($result.RulesVersion -ne 'civil-growth-7' -or $smelter.Count -ne 1 -or $smelter.AssignedWorkers -ne 1) {
                throw 'The gold rules, construction or workforce differ.'
            }
            if ($name -eq 'gold-start' -and ($gold.Quantity -ne 1 -or
                ($result.Resources | Where-Object Id -eq 'gold-ore').Quantity -ne 1 -or
                ($result.Resources | Where-Object Id -eq 'iron').Quantity -ne 401 -or
                ($result.Resources | Where-Object Id -eq 'coal').Quantity -ne 402)) {
                throw 'The first gold production or shared coal balance differs.'
            }
            if ($name -eq 'gold-8-hours' -and ($gold.Quantity -ne 1000 -or $result.Population -ne 48 -or
                ($result.Resources | Where-Object Id -eq 'iron').Quantity -ne 1000)) {
                throw 'The long gold and iron timeline differs.'
            }
        }
        if ($name -ne '60-seconds') {
            foreach ($resource in $result.Resources) {
                if ($resource.Quantity -lt 0 -or $resource.Quantity -gt $resource.Capacity) { throw 'Resource outside capacity.' }
            }
        }
        $output | Set-Content -LiteralPath (Join-Path $artifacts ($name + '.json')) -Encoding UTF8
    }
    $acceleratedOutput = & dotnet $simulation --scenario civil-growth --strategy bootstrap --seconds 180
    if ($LASTEXITCODE -ne 0) { $acceleratedOutput; throw 'Accelerated civil-growth-7 probe failed.' }
    $accelerated = $acceleratedOutput | ConvertFrom-Json
    if ($accelerated.RulesVersion -ne 'civil-growth-7' -or $accelerated.Population -ne 10 -or
        $accelerated.PopulationState.GrowthRateTwentieths -ne 23 -or
        $accelerated.PopulationState.GrowthRemainderTwentieths -lt 0 -or
        $accelerated.PopulationState.GrowthRemainderTwentieths -ge 20) {
        throw 'Unexpected accelerated housing growth or invalid fractional progress.'
    }
    $acceleratedOutput | Set-Content -LiteralPath (Join-Path $artifacts 'civil-accelerated.json') -Encoding UTF8
    $housingOutput = & dotnet $simulation --benchmark-housing
    if ($LASTEXITCODE -ne 0) { $housingOutput; throw 'Housing comparison failed.' }
    $housing = $housingOutput | ConvertFrom-Json
    if ($housing.RulesVersion -ne 'civil-growth-4' -or $housing.HorizonSeconds -ne 28800 -or
        $housing.Strategies.Count -ne 3) { throw 'Unexpected housing benchmark version or shape.' }
    $expectedHousing = @{
        'small-only'     = @{ T48 = 2461; T100 = 5581; T136 = 7741; Population = 136; Small = 17; Large = 0; Fish = 2; Store = 0; Planks = 1480; Stone = 740 }
        'large-standard' = @{ T48 = 2680; T100 = 5800; T136 = 7960; Population = 144; Small = 2; Large = 4; Fish = 2; Store = 0; Planks = 1340; Stone = 740 }
        'large-early'    = @{ T48 = 2561; T100 = 5681; T136 = 7841; Population = 144; Small = 2; Large = 4; Fish = 2; Store = 1; Planks = 1440; Stone = 800 }
    }
    foreach ($name in $expectedHousing.Keys) {
        $actual = @($housing.Strategies | Where-Object Id -eq $name)
        $expected = $expectedHousing[$name]
        if ($actual.Count -ne 1 -or $actual[0].SecondsTo48 -ne $expected.T48 -or
            $actual[0].SecondsTo100 -ne $expected.T100 -or $actual[0].SecondsTo136 -ne $expected.T136 -or
            $actual[0].FinalPopulation -ne $expected.Population -or $actual[0].SmallHouses -ne $expected.Small -or
            $actual[0].LargeHouses -ne $expected.Large -or $actual[0].Fishermen -ne $expected.Fish -or
            $actual[0].Warehouses -ne $expected.Store -or $actual[0].PlanksSpent -ne $expected.Planks -or
            $actual[0].StoneSpent -ne $expected.Stone -or $actual[0].MissingFoodSeconds -ne 0 -or
            $actual[0].MissingBreadSeconds -ne 0 -or $actual[0].HousingFullSeconds -le 0 -or
            $actual[0].ReconciledSeconds -ne $housing.HorizonSeconds) {
            throw "Housing benchmark differs for $name."
        }
        if ($name -eq 'small-only') {
            if ($actual[0].BakeryActiveSeconds -ne 0 -or $actual[0].BakeryStorageBlockedSeconds -ne 0 -or
                $actual[0].FinalBread -ne 0) { throw 'The small-house strategy unexpectedly produced bread.' }
        }
        elseif ($actual[0].BakeryActiveSeconds -le 0 -or $actual[0].BakeryStorageBlockedSeconds -le 0 -or
            $actual[0].FinalBread -le 0) { throw "The bread production diagnostics are missing for $name." }
    }
    $housingOutput | Set-Content -LiteralPath (Join-Path $artifacts 'housing-benchmark.json') -Encoding UTF8
    $fasterOutput = & dotnet $simulation --benchmark-housing-accelerated
    if ($LASTEXITCODE -ne 0) { $fasterOutput; throw 'Accelerated housing comparison failed.' }
    $faster = $fasterOutput | ConvertFrom-Json
    if ($faster.RulesVersion -ne 'civil-growth-5' -or $faster.HorizonSeconds -ne $housing.HorizonSeconds -or
        $faster.Strategies.Count -ne $housing.Strategies.Count) { throw 'Accelerated benchmark shape differs.' }
    foreach ($strategy in $faster.Strategies) {
        $historical = @($housing.Strategies | Where-Object Id -eq $strategy.Id)
        if ($historical.Count -ne 1 -or $strategy.ReconciledSeconds -ne $faster.HorizonSeconds -or
            $strategy.FinalPopulation -ne $historical[0].FinalPopulation -or
            $strategy.SecondsTo48 -ge $historical[0].SecondsTo48 -or
            $strategy.SecondsTo100 -ge $historical[0].SecondsTo100 -or
            $strategy.SecondsTo136 -ge $historical[0].SecondsTo136) {
            throw "Accelerated housing comparison failed for $($strategy.Id)."
        }
    }
    $fasterOutput | Set-Content -LiteralPath (Join-Path $artifacts 'housing-accelerated-benchmark.json') -Encoding UTF8
    $breadScaleOutput = & dotnet $simulation --benchmark-bread-scale
    if ($LASTEXITCODE -ne 0) { $breadScaleOutput; throw 'Advanced bread scaling comparison failed.' }
    $breadScale = $breadScaleOutput | ConvertFrom-Json
    if ($breadScale.ScenarioId -ne 'civil-growth' -or $breadScale.RulesVersion -ne 'civil-growth-4' -or
        $breadScale.StartPopulation -ne 220 -or $breadScale.DurationSeconds -ne 1800 -or
        $breadScale.SingleBakeryContinuousAdvancedLimit -ne 200 -or
        $breadScale.SecondChainWorkplaces -ne 5 -or $breadScale.LeanChainWorkplaces -ne 4 -or
        $breadScale.Strategies.Count -ne 4) {
        throw 'Unexpected bread scaling scenario or production parameters.'
    }
    $one = @($breadScale.Strategies | Where-Object Id -eq 'one-chain')
    $two = @($breadScale.Strategies | Where-Object Id -eq 'two-chains')
    $late = @($breadScale.Strategies | Where-Object Id -eq 'second-chain-late')
    $lean = @($breadScale.Strategies | Where-Object Id -eq 'lean-chain')
    if ($one.Count -ne 1 -or $two.Count -ne 1 -or $late.Count -ne 1 -or $lean.Count -ne 1) {
        throw 'Bread scaling strategies are missing or duplicated.'
    }
    foreach ($strategy in $breadScale.Strategies) {
        if ($strategy.ReconciledSeconds -ne $breadScale.DurationSeconds -or
            $strategy.HousingCapacity -ne 264 -or $strategy.HousingFullSeconds -ne 0 -or
            $strategy.MissingFoodSeconds -ne 0 -or $strategy.FinalBread -lt 0 -or
            $strategy.FinalFood -lt 0 -or $strategy.FinalAdvancedResidents -ne ($strategy.FinalPopulation - 8) -or
            $strategy.FarmRunningWorkerSeconds -lt 0 -or $strategy.WellRunningWorkerSeconds -lt 0 -or
            $strategy.MillRunningWorkerSeconds -lt 0 -or
            $strategy.BreadProduced -ne (3 * $strategy.BakeryRunningWorkerSeconds) -or
            [math]::Abs(([double]$strategy.FinalBread) -
                (100 + [double]$strategy.BreadProduced - [double]$strategy.BreadConsumed)) -gt 0.001) {
            throw "The bread scaling forecast or capacity differs for $($strategy.Id)."
        }
    }
    if ($two[0].FinalPopulation -ne 250 -or $two[0].SecondsTo250 -ne 1800 -or
        $two[0].MissingBreadSeconds -ne 0 -or
        $one[0].FinalPopulation -ge $late[0].FinalPopulation -or
        $late[0].FinalPopulation -gt $two[0].FinalPopulation -or
        $one[0].MissingBreadSeconds -le 0 -or
        $one[0].AdditionalPlanksSpent -ne 0 -or $one[0].AdditionalStoneSpent -ne 0 -or
        $two[0].AdditionalPlanksSpent -ne 420 -or $two[0].AdditionalStoneSpent -ne 280 -or
        $late[0].AdditionalPlanksSpent -ne 420 -or $late[0].AdditionalStoneSpent -ne 280 -or
        $two[0].AssignedWorkers -ne ($one[0].AssignedWorkers + 5) -or
        $late[0].AssignedWorkers -ne $two[0].AssignedWorkers) {
        throw 'The cost, workforce or growth advantage of the second bread chain differs.'
    }
    if ($lean[0].AdditionalPlanksSpent -ne 360 -or $lean[0].AdditionalStoneSpent -ne 250 -or
        $lean[0].AssignedWorkers -ne ($one[0].AssignedWorkers + 4) -or
        $lean[0].FinalPopulation -lt $one[0].FinalPopulation -or
        $lean[0].FinalPopulation -gt $two[0].FinalPopulation -or
        $lean[0].MissingBreadSeconds -gt $one[0].MissingBreadSeconds) {
        throw 'The lean bread chain cost, staffing or measured progression differs.'
    }
    $breadScaleOutput | Set-Content -LiteralPath (Join-Path $artifacts 'bread-scale-benchmark.json') -Encoding UTF8
    $highOutput = & dotnet $simulation --benchmark-bread-high-density
    if ($LASTEXITCODE -ne 0) { $highOutput; throw 'High-density bread comparison failed.' }
    $high = $highOutput | ConvertFrom-Json
    if ($high.ScenarioId -ne 'civil-growth' -or $high.RulesVersion -ne 'civil-growth-4' -or
        $high.StartPopulation -ne 320 -or $high.DurationSeconds -ne 1800 -or
        $high.SecondChainWorkplaces -ne 5 -or $high.LeanChainWorkplaces -ne 4 -or
        $high.Strategies.Count -ne 4) {
        throw 'Unexpected high-density bread benchmark parameters.'
    }
    $highOne = @($high.Strategies | Where-Object Id -eq 'one-chain')
    $highTwo = @($high.Strategies | Where-Object Id -eq 'two-chains')
    $highLate = @($high.Strategies | Where-Object Id -eq 'second-chain-late')
    $highLean = @($high.Strategies | Where-Object Id -eq 'lean-chain')
    if ($highOne.Count -ne 1 -or $highTwo.Count -ne 1 -or $highLate.Count -ne 1 -or $highLean.Count -ne 1) {
        throw 'High-density bread strategies are missing or duplicated.'
    }
    foreach ($strategy in $high.Strategies) {
        if ($strategy.ReconciledSeconds -ne 1800 -or $strategy.HousingCapacity -ne 360 -or
            $strategy.HousingFullSeconds -ne 0 -or $strategy.MissingFoodSeconds -ne 0 -or
            $strategy.FinalPopulation -lt 320 -or $strategy.FinalPopulation -gt 350 -or
            $strategy.FinalAdvancedResidents -ne ($strategy.FinalPopulation - 8) -or
            $strategy.SecondsTo240 -ne 0 -or $strategy.SecondsTo250 -ne 0 -or
            $strategy.FinalBread -lt 0 -or $strategy.FinalFood -lt 0 -or
            $strategy.BreadProduced -ne (3 * $strategy.BakeryRunningWorkerSeconds) -or
            [math]::Abs(([double]$strategy.FinalBread) -
                (100 + [double]$strategy.BreadProduced - [double]$strategy.BreadConsumed)) -gt 0.001) {
            throw "High-density bread accounting differs for $($strategy.Id)."
        }
    }
    if ($highOne[0].AdditionalPlanksSpent -ne 0 -or $highOne[0].AdditionalStoneSpent -ne 0 -or
        $highLean[0].AdditionalPlanksSpent -ne 360 -or $highLean[0].AdditionalStoneSpent -ne 250 -or
        $highTwo[0].AdditionalPlanksSpent -ne 420 -or $highTwo[0].AdditionalStoneSpent -ne 280 -or
        $highLate[0].AdditionalPlanksSpent -ne 420 -or $highLate[0].AdditionalStoneSpent -ne 280 -or
        $highLean[0].AssignedWorkers -ne ($highOne[0].AssignedWorkers + 4) -or
        $highTwo[0].AssignedWorkers -ne ($highOne[0].AssignedWorkers + 5) -or
        $highLate[0].AssignedWorkers -ne $highTwo[0].AssignedWorkers -or
        $highTwo[0].FinalPopulation -ne 350 -or $highTwo[0].SecondsTo350 -ne 1800 -or
        $highTwo[0].MissingBreadSeconds -ne 0 -or
        $highOne[0].FinalPopulation -ge $highLean[0].FinalPopulation -or
        $highLean[0].FinalPopulation -ge $highTwo[0].FinalPopulation -or
        $highLean[0].MissingBreadSeconds -le 0) {
        throw 'High-density bread progression did not distinguish lean and full chains as expected.'
    }
    $highOutput | Set-Content -LiteralPath (Join-Path $artifacts 'bread-high-density-benchmark.json') -Encoding UTF8
    $publish = Join-Path $artifacts 'publish'
    Invoke-Dotnet -Arguments @('publish', 'src/SimRisorse.Desktop/SimRisorse.Desktop.csproj', '--no-restore', '--no-build',
        '-c', 'Release', '--self-contained', 'false', '--output', $publish, '--verbosity', 'minimal')
    $nativeSmoke = 'Not run'
    if (-not $SkipNativeSmoke -and [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        foreach ($scenario in @('civil-growth', 'economy-foundation')) {
            $smokePath = Join-Path $runDirectory ($scenario + '-native-save.sqlite3')
            $stderr = Join-Path $runDirectory ($scenario + '-native-stderr.log')
            $process = Start-Process -FilePath (Join-Path $publish 'SimRisorse.Desktop.exe') -WindowStyle Hidden -PassThru `
                -ArgumentList @('--smoke-test', '--scenario', $scenario, '--save-path', ('"' + $smokePath + '"')) -WorkingDirectory $publish `
                -RedirectStandardError $stderr -RedirectStandardOutput (Join-Path $runDirectory ($scenario + '-native-stdout.log'))
            # Keep the process handle before it exits; Windows PowerShell otherwise may lose ExitCode.
            $null = $process.Handle
            if (-not $process.WaitForExit(10000)) { Stop-Process -Id $process.Id; throw 'Desktop native smoke timed out.' }
            $nativeExitCode = $process.ExitCode
            $process.Dispose()
            if ($nativeExitCode -ne 0) { Get-Content -LiteralPath $stderr; throw "Desktop native smoke failed: $scenario, $nativeExitCode" }
            if (-not (Test-Path -LiteralPath $smokePath)) { throw 'Native smoke did not save the game.' }
        }
        $nativeSmoke = 'Passed on Windows for both scenarios: save, load, growth resume, suspend notification, close'
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
    $screenshots = @('buildings.png', 'buildings-compact.png', 'resources.png', 'resources-compact.png',
        'civil-buildings.png', 'civil-buildings-compact.png', 'civil-resources.png', 'civil-resources-compact.png',
        'civil-advanced-bread-compact.png', 'industry-buildings.png', 'industry-buildings-compact.png',
        'industry-resources.png', 'industry-resources-compact.png', 'gold-buildings.png', 'gold-buildings-compact.png',
        'gold-resources.png', 'gold-resources-compact.png')
    foreach ($screenshot in $screenshots) {
        $path = Join-Path $screenshotDirectory $screenshot
        if (-not (Test-Path -LiteralPath $path)) { throw "Missing render from this test run: $screenshot" }
    }
    $report = [ordered]@{
        sdk = (& dotnet --version); total = $total; passed = $passed; failed = $failed
        avaloniaVersion = $dependencies.Avalonia.resolved
        sqliteProviderVersion = $dependencies.'Microsoft.Data.Sqlite'.resolved
        sqliteNativePackageVersion = $dependencies.'SQLitePCLRaw.lib.e_sqlite3'.resolved
        nativeSmoke = $nativeSmoke; offlineRestore = [bool]$OfflinePackageSource
        dependencyAuditRequested = (-not [bool]$OfflinePackageSource)
        testResults = (Join-Path $runDirectory 'test-results')
        housingBenchmark = (Join-Path $artifacts 'housing-benchmark.json')
        breadScaleBenchmark = (Join-Path $artifacts 'bread-scale-benchmark.json')
        breadHighDensityBenchmark = (Join-Path $artifacts 'bread-high-density-benchmark.json')
        screenshots = @($screenshots | ForEach-Object { Join-Path $screenshotDirectory $_ })
    }
    $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $artifacts 'validation.json') -Encoding UTF8
    $report | ConvertTo-Json
}
finally {
    Pop-Location
    $env:AVALONIA_TELEMETRY_OPTOUT = $previousTelemetry
    $env:SIMRISORSE_DESKTOP_ARTIFACTS = $previousArtifacts
}
