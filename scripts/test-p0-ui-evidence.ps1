# G1-D44 — Test autonomi per lo strumento di evidenze P0 (senza Pester o .NET SDK).
# Su Windows: powershell.exe -NoProfile -File .\scripts\test-p0-ui-evidence.ps1
# I PNG di test sono fittizi, usati esclusivamente in una directory temporanea.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$tool = Join-Path $PSScriptRoot 'p0-ui-evidence.ps1'
$exe = [System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName
$root = Join-Path ([System.IO.Path]::GetTempPath()) ('simrisorse-p0-d44-' + [guid]::NewGuid().ToString('N'))
$csv = Join-Path $root 'results.csv'
$report = Join-Path $root 'report.html'
$shots = Join-Path $root 'screenshots'
$steps = 0

function Require([bool]$condition, [string]$message) {
    if (-not $condition) { throw "Test P0 fallito: $message" }
    $script:steps++
}
function Run-Tool([string]$action, [int]$expectedExit) {
    $output = & $exe -NoProfile -NonInteractive -File $tool -Action $action -EvidenceDirectory $root 2>&1
    $actualExit = $LASTEXITCODE
    if ($actualExit -ne $expectedExit) {
        throw "Action=$action: uscita $actualExit, attesa $expectedExit. Output: $($output -join ' ')"
    }
    return ($output -join "`n")
}
function Update-Rows([string]$status) {
    $rows = @(Import-Csv -LiteralPath $csv -Encoding UTF8)
    foreach ($r in $rows) {
        $r.Stato = $status
        $r.Testatore = 'TEST-FITTIZIO'
        $r.Data = '2026-11-04'
        $r.Passi = 'esercizio strumento senza gioco'
        $r.Note = 'prova automatica locale, non evidenza reale'
    }
    $rows | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding UTF8
}

try {
    $null = New-Item -ItemType Directory -Path $root -Force
    $null = Run-Tool 'Init' 0
    $rows = @(Import-Csv -LiteralPath $csv -Encoding UTF8)
    Require ($rows.Count -eq 44) 'Init deve creare 44 scenari'
    $rows[0].Note = 'preserva questa riga'
    $rows | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding UTF8
    $null = Run-Tool 'Init' 0
    $again = @(Import-Csv -LiteralPath $csv -Encoding UTF8)
    Require ($again[0].Note -eq 'preserva questa riga') 'Init non deve sovrascrivere il CSV'

    $null = Run-Tool 'Report' 0
    Require ((Get-Content -LiteralPath $report -Raw -Encoding UTF8) -match 'Gate P0 aperto') 'Registro vuoto non può risultare completo'
    $null = Run-Tool 'Verify' 2

    Update-Rows 'NA'
    $null = Run-Tool 'Report' 0
    Require ((Get-Content -LiteralPath $report -Raw -Encoding UTF8) -match 'Gate P0 aperto') '44 NA non possono risultare completi'
    $null = Run-Tool 'Verify' 2

    Update-Rows 'NON_ESEGUITA'
    $rows = @(Import-Csv -LiteralPath $csv -Encoding UTF8)
    $rows[0].Stato = 'PASS'
    $rows | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding UTF8
    $badImage = Join-Path $shots 'UI-01_1040x740_v13.png'
    [System.IO.File]::WriteAllBytes($badImage, ([byte[]](1..128)))
    $null = Run-Tool 'Report' 0
    $html = Get-Content -LiteralPath $report -Raw -Encoding UTF8
    Require ($html -match 'EVIDENZA_MANCANTE') 'PNG inesistente/corrotto non può fare PASS'
    $null = Run-Tool 'Verify' 2

    # PNG generati localmente: esercitano la logica del validatore, NON sono screenshot.
    Add-Type -AssemblyName System.Drawing
    foreach ($size in @('1040x740','1240x850')) {
        $dims = $size.Split('x')
        $bitmap = [System.Drawing.Bitmap]::new([int]$dims[0], [int]$dims[1])
        $tempPng = Join-Path $root ("fake-$size.png")
        try { $bitmap.Save($tempPng, [System.Drawing.Imaging.ImageFormat]::Png) }
        finally { $bitmap.Dispose() }
        for ($i = 1; $i -le 22; $i++) {
            $id = 'UI-{0:D2}' -f $i
            $versions = if ($i -eq 16 -or $i -eq 17) { @('v14') } else { @('v13','v14') }
            foreach ($ver in $versions) {
                Copy-Item -LiteralPath $tempPng -Destination (Join-Path $shots ("${id}_${size}_${ver}.png")) -Force
            }
        }
    }
    Update-Rows 'PASS'
    # Una riga altrimenti completa non può diventare PASS con PNG danneggiato.
    $corruptTarget = Join-Path $shots 'UI-01_1040x740_v14.png'
    [System.IO.File]::WriteAllBytes($corruptTarget, ([byte[]](1..128)))
    $null = Run-Tool 'Report' 0
    Require ((Get-Content -LiteralPath $report -Raw -Encoding UTF8) -match 'immagine non decodificabile') 'Un PNG corrotto deve avere diagnosi specifica'
    $null = Run-Tool 'Verify' 2
    Copy-Item -LiteralPath (Join-Path $root 'fake-1040x740.png') -Destination $corruptTarget -Force

    # Una cattura di 1040x740 non può soddisfare la viewport 1240x850.
    $wrongSizeTarget = Join-Path $shots 'UI-01_1240x850_v14.png'
    Copy-Item -LiteralPath (Join-Path $root 'fake-1040x740.png') -Destination $wrongSizeTarget -Force
    $null = Run-Tool 'Report' 0
    Require ((Get-Content -LiteralPath $report -Raw -Encoding UTF8) -match 'dimensioni inferiori alla viewport') 'Viewport diversa deve essere rilevata'
    $null = Run-Tool 'Verify' 2
    Copy-Item -LiteralPath (Join-Path $root 'fake-1240x850.png') -Destination $wrongSizeTarget -Force

    $null = Run-Tool 'Report' 0
    Require ((Get-Content -LiteralPath $report -Raw -Encoding UTF8) -match '44/44 scenari') '44 evidenze leggibili devono poter essere registrate'
    $null = Run-Tool 'Verify' 0
    Write-Host "Controlli locali superati: $steps. Le immagini fittizie NON dimostrano parità GUI o chiusura P0."
} finally {
    if (Test-Path -LiteralPath $root) {
        Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
    }
}
