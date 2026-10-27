param(
    [string]$ArtifactsDirectory = '',
    [string]$OutputPath = '',
    [switch]$Force
)

# Read-only analysis by default. This file does not run a game, change its rules,
# touch personal SQLite saves, or re-run the benchmarks.
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($ArtifactsDirectory)) {
    $ArtifactsDirectory = Join-Path $repositoryRoot '.local-cache/m4'
}
$culture = [Globalization.CultureInfo]::GetCultureInfo('it-IT')

function Read-Benchmark {
    param([string]$FileName)
    $path = Join-Path $ArtifactsDirectory $FileName
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Manca $FileName in '$ArtifactsDirectory'. Eseguire prima validate.cmd su Windows."
    }
    try { return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json }
    catch { throw "Impossibile leggere $FileName come JSON: $($_.Exception.Message)" }
}

function Require-Comparison {
    param([object]$Report, [string]$Name, [string]$RulesVersion, [int]$Seconds, [string[]]$Ids)
    if ($Report.ScenarioId -ne 'civil-growth' -or $Report.RulesVersion -ne $RulesVersion) {
        throw "$Name contiene uno scenario/versione regole inatteso; non confrontare dati di versioni diverse."
    }
    $duration = if ($null -ne $Report.HorizonSeconds) { $Report.HorizonSeconds } else { $Report.DurationSeconds }
    if ($duration -ne $Seconds) { throw "$Name ha una durata inattesa: $duration s invece di $Seconds s." }
    $strategies = @($Report.Strategies)
    if ($strategies.Count -ne $Ids.Count) { throw "$Name ha $($strategies.Count) strategie, ne erano attese $($Ids.Count)." }
    $result = @{}
    foreach ($strategy in $strategies) {
        if ($null -eq $strategy.Id -or $result.ContainsKey([string]$strategy.Id)) {
            throw "$Name contiene strategie senza ID o duplicate."
        }
        $result[[string]$strategy.Id] = $strategy
        if ($strategy.ReconciledSeconds -ne $Seconds) {
            throw "$Name/$($strategy.Id): secondi riconciliati $($strategy.ReconciledSeconds), attesi $Seconds."
        }
    }
    foreach ($id in $Ids) {
        if (-not $result.ContainsKey($id)) { throw "$Name non contiene la strategia $id." }
    }
    return ,$result
}

function Format-Integer {
    param($Value)
    if ($null -eq $Value) { return 'n.r.' }
    return ([int]$Value).ToString('N0', $culture)
}

function Format-Decimal {
    param($Value)
    if ($null -eq $Value) { return 'n.r.' }
    return ([decimal]$Value).ToString('N3', $culture)
}

function Format-TimePair {
    param($Earlier, $Current)
    return "$(Format-Threshold $Earlier) → $(Format-Threshold $Current)"
}

function Format-Threshold {
    param($Value)
    if ($null -eq $Value) { return 'non raggiunto' }
    return Format-Integer $Value
}

$housing = Read-Benchmark 'housing-benchmark.json'
$accelerated = Read-Benchmark 'housing-accelerated-benchmark.json'
$bread220 = Read-Benchmark 'bread-scale-benchmark.json'
$bread320 = Read-Benchmark 'bread-high-density-benchmark.json'

$housingIds = @('small-only', 'large-standard', 'large-early')
$breadIds = @('one-chain', 'lean-chain', 'two-chains', 'second-chain-late')
$originalHouses = Require-Comparison $housing 'housing-benchmark.json' 'civil-growth-4' 28800 $housingIds
$fasterHouses = Require-Comparison $accelerated 'housing-accelerated-benchmark.json' 'civil-growth-5' 28800 $housingIds
$middleBread = Require-Comparison $bread220 'bread-scale-benchmark.json' 'civil-growth-4' 1800 $breadIds
$denseBread = Require-Comparison $bread320 'bread-high-density-benchmark.json' 'civil-growth-4' 1800 $breadIds
if ($bread220.StartPopulation -ne 220 -or $bread320.StartPopulation -ne 320) {
    throw 'I benchmark del Pane non partono dalle popolazioni attese (220 e 320).'
}

$validationPath = Join-Path $ArtifactsDirectory 'validation.json'
$validation = $null
if (Test-Path -LiteralPath $validationPath -PathType Leaf) {
    try { $validation = Get-Content -LiteralPath $validationPath -Raw -Encoding UTF8 | ConvertFrom-Json }
    catch { throw "validation.json non è leggibile: $($_.Exception.Message)" }
}

$lines = New-Object 'System.Collections.Generic.List[string]'
$lines.Add('# SimRisorse — Lettura dei benchmark M4')
$lines.Add('')
$lines.Add('**Origine:** report JSON della validazione Windows; misure di quattro esperimenti controllati, non stime di una partita completa.')
$lines.Add('**Versioni:** abitazioni storiche `civil-growth-4` contro abitazioni accelerate `civil-growth-5`; Pane a 220/320 abitanti sempre `civil-growth-4`.')
$lines.Add('')
$lines.Add('## Gate di validazione')
$lines.Add('')
if ($null -eq $validation) {
    $lines.Add('- `validation.json` assente: la presenza dei benchmark non certifica il superamento di `validate.cmd`.')
} else {
    $valid = $validation.failed -eq 0 -and $validation.passed -eq $validation.total -and $validation.total -gt 0
    $state = if ($valid) { 'nessun test fallito dichiarato dal report' } else { 'attenzione: il report non dichiara tutti i test superati' }
    $lines.Add("- **$state** — $($validation.passed)/$($validation.total) passati; falliti $($validation.failed).")
    $lines.Add("- Avvio nativo: $($validation.nativeSmoke).")
}
$lines.Add('- Gli esiti TRX, i render e la provenienza della stessa esecuzione devono essere verificati separatamente. I JSON nella cartella M4 possono essere riscritti da sessioni diverse.')
$lines.Add('')
$lines.Add('## Abitazioni: confronto storico e accelerato (8 ore simulate)')
$lines.Add('')
$lines.Add('| Strategia | 48 abitanti (s: v4 → v5) | 100 abitanti (s) | 136 abitanti (s) | Risparmio per 136 (s) | Abitanti finali v4 → v5 |')
$lines.Add('| --- | ---: | ---: | ---: | ---: | ---: |')
foreach ($id in $housingIds) {
    $old = $originalHouses[$id]
    $new = $fasterHouses[$id]
    $saving = if ($null -ne $old.SecondsTo136 -and $null -ne $new.SecondsTo136) {
        Format-Integer ([int]$old.SecondsTo136 - [int]$new.SecondsTo136)
    } else { 'n.r.' }
    $lines.Add("| ``$id`` | $(Format-TimePair $old.SecondsTo48 $new.SecondsTo48) | $(Format-TimePair $old.SecondsTo100 $new.SecondsTo100) | $(Format-TimePair $old.SecondsTo136 $new.SecondsTo136) | $saving | $($old.FinalPopulation) → $($new.FinalPopulation) |")
}
$lines.Add('')
$lines.Add('**Lettura:** un risparmio positivo significa raggiungere la soglia prima con le regole accelerate. La popolazione finale può essere limitata dai posti: non confondere velocità e capienza.')

foreach ($profile in @(
    @{ Name = 'Pane: colonia da 220 abitanti'; Data = $middleBread; Targets = @('SecondsTo240', 'SecondsTo250') },
    @{ Name = 'Pane: colonia da 320 abitanti'; Data = $denseBread; Targets = @('SecondsTo340', 'SecondsTo350') }
)) {
    $lines.Add('')
    $lines.Add("## $($profile.Name) — 1.800 secondi simulati")
    $lines.Add('')
    $lines.Add('| Strategia | Abitanti finali | Prima soglia (s) | Seconda soglia (s) | Senza Pane (s) | Pane prodotto | Pane consumato | Pane finale | Addetti totali | Spesa extra legno/pietra |')
    $lines.Add('| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |')
    foreach ($id in $breadIds) {
        $data = $profile.Data[$id]
        $first = Format-Threshold ($data.PSObject.Properties[$profile.Targets[0]].Value)
        $second = Format-Threshold ($data.PSObject.Properties[$profile.Targets[1]].Value)
        $spent = "$(Format-Integer $data.AdditionalPlanksSpent) / $(Format-Integer $data.AdditionalStoneSpent)"
        $lines.Add("| ``$id`` | $(Format-Integer $data.FinalPopulation) | $first | $second | $(Format-Integer $data.MissingBreadSeconds) | $(Format-Decimal $data.BreadProduced) | $(Format-Decimal $data.BreadConsumed) | $(Format-Decimal $data.FinalBread) | $(Format-Integer $data.AssignedWorkers) | $spent |")
    }
}

$lines.Add('')
$lines.Add('## Interpretazione limitata ai profili testati')
$lines.Add('')
$lean = $denseBread['lean-chain']
$complete = $denseBread['two-chains']
$lines.Add("- A 320 abitanti, la filiera **snella** ha $($lean.MissingBreadSeconds) secondi di carenza di Pane e arriva a $($lean.FinalPopulation) abitanti; la seconda filiera **completa** ha $($complete.MissingBreadSeconds) secondi di carenza e arriva a $($complete.FinalPopulation).")
$lines.Add('- Il costo indicato per il Pane è **incrementale** rispetto allo stato sintetico iniziale; non comprende la costruzione della colonia dalle prime risorse.')
$lines.Add('- Nessun benchmark dimostra da solo la durata delle future condizioni di vittoria, la strategia ottima o la sicurezza di modificare prezzi e ricette.')
$lines.Add('- **M4 resta da chiudere formalmente**: risultati e grafica v25 da riesaminare, Carne e demolizione da classificare; il gate D0 delle regole M5 richiede decisioni esplicite.')

$markdown = $lines -join [Environment]::NewLine
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    if ((Test-Path -LiteralPath $OutputPath) -and -not $Force) {
        throw "Il report esiste già: $OutputPath. Specificare -Force per sovrascriverlo esplicitamente."
    }
    $targetDirectory = Split-Path -Parent $OutputPath
    if ($targetDirectory -and -not (Test-Path -LiteralPath $targetDirectory -PathType Container)) {
        throw "La directory di destinazione non esiste: $targetDirectory."
    }
    $markdown | Set-Content -LiteralPath $OutputPath -Encoding UTF8
}
else { Write-Output $markdown }
