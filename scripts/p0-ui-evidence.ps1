# G1-D44 — Registro locale delle evidenze visive P0 (validazione immagini e stato).
# Esegue solo operazioni su file locali. Non avvia il gioco e non legge salvataggi.
[CmdletBinding()]
param(
    [ValidateSet('Init','Capture','Report','Verify')]
    [string]$Action = 'Init',
    [string]$EvidenceDirectory = '',
    [ValidatePattern('^UI-(0[1-9]|1[0-9]|2[0-2])$')]
    [string]$UiId,
    [ValidateSet('1040x740','1240x850')]
    [string]$Viewport,
    [ValidateSet('v13','v14')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) {
    $EvidenceDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) '.local-cache\p0-ui-evidence'
}
$EvidenceDirectory = [System.IO.Path]::GetFullPath($EvidenceDirectory)
$Screenshots = Join-Path $EvidenceDirectory 'screenshots'
$CsvPath = Join-Path $EvidenceDirectory 'results.csv'
$HtmlPath = Join-Path $EvidenceDirectory 'report.html'

$Areas = @(
    'Navigazione','Risorse - colonne','Risorse - numeri','Risorse - capacità',
    'Risorse - flussi','Risorse - selezione','Edifici - elenco','Edifici - tier',
    'Edifici - blocchi','Edifici - comandi','Edifici - lotti','Edifici - dettagli',
    'Pane','Popolazione','Esercito','Scuola','Tecnologie',
    'Hover e focus','Selezioni al refresh','Nuova partita','Persistenza',
    'Invarianti abitanti'
)

function Html([string]$value) {
    return [System.Net.WebUtility]::HtmlEncode($value)
}
function ScreenshotName([string]$id, [string]$viewportValue, [string]$versionValue) {
    return ('{0}_{1}_{2}.png' -f $id, $viewportValue, $versionValue)
}
function Ensure-Directories {
    New-Item -ItemType Directory -Path $Screenshots -Force | Out-Null
}
function Required-ScreenshotVersions([string]$id) {
    if ($id -eq 'UI-16' -or $id -eq 'UI-17') { return @('v14') }
    return @('v13','v14')
}
# La semplice presenza del file non dimostra che esista una prova visiva.
# System.Drawing è disponibile su Windows, dove avviene l'audit manuale.
function Screenshot-Issue([string]$path, [string]$viewportValue) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return 'file assente' }
    if ((Get-Item -LiteralPath $path).Length -lt 64) { return 'file vuoto o incompleto' }
    $image = $null
    try {
        Add-Type -AssemblyName System.Drawing -ErrorAction Stop
        $image = [System.Drawing.Image]::FromFile($path)
        if ($image.RawFormat.Guid -ne [System.Drawing.Imaging.ImageFormat]::Png.Guid) {
            return 'il contenuto non è PNG'
        }
        # La cattura della finestra deve essere compatibile con la viewport dichiarata.
        # 16 px di tolleranza per bordi/scaling: non valida l'identità del gioco.
        $dims = $viewportValue.Split('x')
        $minWidth = [int]$dims[0] - 16
        $minHeight = [int]$dims[1] - 16
        if ($image.Width -lt $minWidth -or $image.Height -lt $minHeight) {
            return "dimensioni inferiori alla viewport $viewportValue ($($image.Width)x$($image.Height))"
        }
        return $null
    } catch {
        return 'immagine non decodificabile o sistema di lettura non disponibile'
    } finally {
        if ($null -ne $image) { $image.Dispose() }
    }
}

if ($Action -eq 'Init') {
    Ensure-Directories
    if (-not (Test-Path -LiteralPath $CsvPath)) {
        $rows = @()
        for ($i = 1; $i -le 22; $i++) {
            $id = 'UI-{0:D2}' -f $i
            foreach ($resolution in @('1040x740','1240x850')) {
                $rows += [pscustomobject]@{
                    UI_ID = $id; Area = $Areas[$i-1]; Viewport = $resolution
                    Stato = 'NON_ESEGUITA'; Passi = ''; Testatore = ''
                    Data = ''; Note = ''; Issue = ''; RiferimentoTRX = ''
                }
            }
        }
        $rows | Export-Csv -LiteralPath $CsvPath -NoTypeInformation -Encoding UTF8
        Write-Host "Creato registro: $CsvPath (44 scenari, nessuno eseguito)"
    } else {
        Write-Host "Registro già presente, conservato senza sovrascrittura: $CsvPath"
    }
    Write-Host "Screenshot in: $Screenshots"
    Write-Host 'Modificare results.csv; poi usare -Action Report. Lo script non assegna automaticamente esiti PASS.'
    exit 0
}

if ($Action -eq 'Capture') {
    if (-not $UiId -or -not $Viewport -or -not $Version) {
        throw 'Capture richiede -UiId, -Viewport e -Version.'
    }
    if (($UiId -eq 'UI-16' -or $UiId -eq 'UI-17') -and $Version -eq 'v13') {
        throw 'Scuola e Albero tecnologico non hanno una schermata equivalente v13: catturare solo v14.'
    }
    if ([Environment]::OSVersion.Platform -ne [System.PlatformID]::Win32NT) {
        throw 'La cattura dalla clipboard richiede Windows.'
    }
    Ensure-Directories
    $target = Join-Path $Screenshots (ScreenshotName $UiId $Viewport $Version)
    if (Test-Path -LiteralPath $target) {
        throw "Screenshot già presente: $target. Rinominare/spostare manualmente il precedente prima di una nuova cattura."
    }
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing
    if (-not [System.Windows.Forms.Clipboard]::ContainsImage()) {
        throw 'Non c’è un’immagine negli Appunti. Usare Alt+Stamp sulla finestra del gioco; ripetere con PowerShell in modalità STA se necessario.'
    }
    $image = [System.Windows.Forms.Clipboard]::GetImage()
    if ($null -eq $image) { throw 'Impossibile leggere l’immagine dagli Appunti.' }
    try {
        $image.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
        $issue = Screenshot-Issue $target $Viewport
        if ($issue) {
            Remove-Item -LiteralPath $target -Force
            throw "Cattura non valida: $issue. Ripetere la cattura con l'intera finestra visibile."
        }
        Write-Host "Salvato: $target ($($image.Width)x$($image.Height) px)"
        Write-Host 'La cattura non equivale a PASS: completare Passi, Testatore, Data e Stato in results.csv.'
    } finally {
        $image.Dispose()
    }
    exit 0
}

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "Registro non presente: $CsvPath. Eseguire prima -Action Init."
}
Ensure-Directories
$rows = @(Import-Csv -LiteralPath $CsvPath -Encoding UTF8)
if ($rows.Count -ne 44) { throw "Attese esattamente 44 righe, trovate $($rows.Count)." }
foreach ($field in @('UI_ID','Area','Viewport','Stato','Passi','Testatore','Data','Note','Issue','RiferimentoTRX')) {
    if (-not $rows[0].PSObject.Properties[$field]) {
        throw "Colonna obbligatoria mancante dal registro CSV: $field"
    }
}
$keys = @{}
foreach ($r in $rows) {
    if ($r.UI_ID -notmatch '^UI-(0[1-9]|1[0-9]|2[0-2])$' -or
        $r.Viewport -notin @('1040x740','1240x850')) {
        throw "ID/viewport non valido: $($r.UI_ID) $($r.Viewport)"
    }
    $key = "$($r.UI_ID)|$($r.Viewport)"
    if ($keys.ContainsKey($key)) { throw "Riga duplicata: $key" }
    $keys[$key] = $true
    if ($r.Stato -notin @('NON_ESEGUITA','PASS','FAIL','NA')) {
        throw "Stato non valido per $key. Valori: NON_ESEGUITA, PASS, FAIL, NA."
    }
}
$counts = @{ 'PASS' = 0; 'FAIL' = 0; 'NA' = 0; 'NON_ESEGUITA' = 0; 'EVIDENZA_MANCANTE' = 0 }
$blocks = New-Object System.Collections.Generic.List[string]
foreach ($r in ($rows | Sort-Object UI_ID,Viewport)) {
    $missing = @()
    if ($r.Stato -in @('PASS','FAIL','NA')) {
        if ([string]::IsNullOrWhiteSpace($r.Testatore)) { $missing += 'testatore' }
        if ([string]::IsNullOrWhiteSpace($r.Data)) { $missing += 'data' }
        if ([string]::IsNullOrWhiteSpace($r.Passi)) { $missing += 'passi effettuati' }
    }
    if ($r.Stato -eq 'NA' -and [string]::IsNullOrWhiteSpace($r.Note)) { $missing += 'motivazione NA (Note)' }
    if ($r.Stato -eq 'FAIL' -and [string]::IsNullOrWhiteSpace($r.Issue)) { $missing += 'issue/bug' }
    if ($r.Stato -eq 'PASS') {
        foreach ($ver in (Required-ScreenshotVersions $r.UI_ID)) {
            $filename = ScreenshotName $r.UI_ID $r.Viewport $ver
            $issue = Screenshot-Issue (Join-Path $Screenshots $filename) $r.Viewport
            if ($issue) { $missing += "immagine $filename ($issue)" }
        }
    }
    $effective = $r.Stato
    if ($missing.Count -gt 0) { $effective = 'EVIDENZA_MANCANTE' }
    $counts[$effective]++
    $images = New-Object System.Collections.Generic.List[string]
    foreach ($ver in @('v13','v14')) {
        $filename = ScreenshotName $r.UI_ID $r.Viewport $ver
        $filePath = Join-Path $Screenshots $filename
        $imageIssue = Screenshot-Issue $filePath $r.Viewport
        if (-not $imageIssue) {
            $images.Add('<figure><img src="screenshots/' + (Html $filename) + '" alt="' + (Html $filename) + '"><figcaption>' + (Html $ver) + '</figcaption></figure>')
        } elseif ($ver -eq 'v13' -and $r.UI_ID -in @('UI-16','UI-17')) {
            $images.Add('<figure class="na">v13 non applicabile (pagina nuova nella v14)</figure>')
        } else {
            $images.Add('<figure class="missing">Screenshot non utilizzabile: ' + (Html $filename) + ' — ' + (Html $imageIssue) + '</figure>')
        }
    }
    $details = 'Passi: ' + (Html $r.Passi) + ' | Testatore: ' + (Html $r.Testatore) + ' | Data: ' + (Html $r.Data)
    if ($r.Note) { $details += '<br>Note/NA: ' + (Html $r.Note) }
    if ($r.Issue) { $details += '<br>Issue: ' + (Html $r.Issue) }
    if ($r.RiferimentoTRX) { $details += '<br>TRX: ' + (Html $r.RiferimentoTRX) }
    if ($missing.Count -gt 0) { $details += '<br><b>Da integrare:</b> ' + (Html ($missing -join ', ')) }
    $blocks.Add('<section><header><strong>' + (Html $r.UI_ID) + ' · ' + (Html $r.Area) +
                ' · ' + (Html $r.Viewport) + '</strong><span class="status">' + (Html $effective) +
                '</span></header><div class="images">' + ($images -join "`n") +
                '</div><p>' + $details + '</p></section>')
}
# Tutte le 44 prove riguardano la v14. NA è una deroga da approvare,
# non una prova positiva: 44 NA non devono apparire come audit completato.
$complete = ($counts['PASS'] -eq 44 -and $counts['FAIL'] -eq 0 -and
             $counts['NA'] -eq 0 -and $counts['NON_ESEGUITA'] -eq 0 -and
             $counts['EVIDENZA_MANCANTE'] -eq 0)
$statusText = if ($complete) { 'Evidenze registrate per 44/44 scenari; gate P0 ancora da approvare esplicitamente' }
              else { 'Gate P0 aperto: verifiche incomplete, anomalie o eccezioni NA da approvare' }
if ($Action -eq 'Verify') {
    Write-Host "Verifica evidenze: PASS=$($counts['PASS']), FAIL=$($counts['FAIL']), NA=$($counts['NA']), NON_ESEGUITA=$($counts['NON_ESEGUITA']), EVIDENZA_MANCANTE=$($counts['EVIDENZA_MANCANTE'])"
    Write-Host $statusText
    if ($complete) { exit 0 }
    exit 2
}
$page = @"
<!doctype html><html lang="it"><head><meta charset="utf-8">
<title>SimRisorse · G1-D43 · Audit visuale P0</title>
<style>
body{font:14px/1.55 system-ui,sans-serif;max-width:1180px;margin:2rem auto;padding:0 1rem;background:#f7f6f1;color:#252e34}
header{display:flex;justify-content:space-between;gap:1rem;align-items:baseline;padding:8px 12px;background:#ebebdf}
section{border:1px solid #cbc9bb;margin:16px 0;background:white}
section p{padding:0 14px 10px;overflow-wrap:anywhere}.images{display:grid;grid-template-columns:1fr 1fr;gap:12px;padding:12px}
figure{margin:0;min-height:140px;border:1px dashed #b8b8ad;display:flex;align-items:center;justify-content:center;flex-direction:column;padding:8px;overflow:hidden}
figure img{display:block;max-width:100%;height:auto}figcaption{margin-top:6px;font-weight:600}
.na,.missing{color:#5d6365}.status{font-weight:700}small{color:#525b5a}
@media(max-width:720px){.images{grid-template-columns:1fr}header{display:block}}
</style></head><body>
<h1>SimRisorse · G1-D43 · Confronto GUI v13 / v14</h1>
<p><strong>$(Html $statusText)</strong></p>
<p>PASS: $($counts['PASS']) · FAIL: $($counts['FAIL']) · NA: $($counts['NA']) · NON ESEGUITA: $($counts['NON_ESEGUITA']) · EVIDENZA MANCANTE: $($counts['EVIDENZA_MANCANTE']) · Totale: 44.</p>
<p><small>I PNG sono controllati per leggibilità, formato e dimensioni coerenti con la viewport, non per autenticità del contenuto né correttezza della GUI. Gli esiti sono assegnati dall'operatore. Una riga NA richiede una deroga esplicita: non è un PASS. Anche 44/44 PASS NON chiudono P0: servono suite .NET cumulativa verde e approvazione visiva.</small></p>
$($blocks -join "`n")
</body></html>
"@
[System.IO.File]::WriteAllText($HtmlPath, $page, ([System.Text.UTF8Encoding]::new($false)))
Write-Host "Creato rapporto: $HtmlPath"
Write-Host "Esiti registrati: PASS=$($counts['PASS']), FAIL=$($counts['FAIL']), NA=$($counts['NA']), NON_ESEGUITA=$($counts['NON_ESEGUITA']), EVIDENZA_MANCANTE=$($counts['EVIDENZA_MANCANTE'])"
Write-Host $statusText
