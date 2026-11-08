param(
    [string]$KeepGatePath = '',
    [string]$KeepRunPath = '',
    [switch]$RemovePublish,
    [switch]$DryRun
)

# Deletes ONLY old GUID-named validation evidence folders under .local-cache/m4.
# Keeps the current folder, or the most recently modified one if called manually.
# Does not touch NuGet, game saves, .git, source code, or unrecognised folders.
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$cache = Join-Path $root '.local-cache\m4'
# Never descend through a linked/junction cache root.
foreach ($candidate in @((Join-Path $root '.local-cache'), $cache)) {
    if ((Test-Path -LiteralPath $candidate) -and
        ((Get-Item -LiteralPath $candidate -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "Refusing to clean linked local cache: $candidate"
    }
}
# Manual cleanup must not run while the validation gate owns the lock.
# A call from that gate passes KeepGatePath and already holds the lock.
$manualLock = $null
if ([string]::IsNullOrWhiteSpace($KeepGatePath)) {
    if (-not (Test-Path -LiteralPath $cache -PathType Container)) { return }
    try {
        $manualLock = [IO.File]::Open((Join-Path $cache 'validation.lock'),
            [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    }
    catch { throw 'Validation currently running; cache cleanup skipped to protect evidence.' }
}
$deleted = 0
$skipped = 0
$errors = 0

function Prune-Folder {
    param([string]$Name, [string]$KeepPath)
    $base = Join-Path $cache $Name
    if (-not (Test-Path -LiteralPath $base -PathType Container)) { return }
    $baseItem = Get-Item -LiteralPath $base -Force
    if ($baseItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        Write-Warning "Skipped linked directory: $base"
        return
    }
    $items = @(Get-ChildItem -LiteralPath $base -Directory -Force |
        Where-Object { $_.Name -match '^[0-9a-fA-F]{32}$' -and
            -not ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) } |
        Sort-Object LastWriteTimeUtc -Descending)
    $keep = ''
    if (-not [string]::IsNullOrWhiteSpace($KeepPath)) {
        $expected = [IO.Path]::GetFullPath($KeepPath).TrimEnd('\')
        if ($expected.StartsWith(([IO.Path]::GetFullPath($base).TrimEnd('\') + '\'),
                [StringComparison]::OrdinalIgnoreCase) -and
            (Split-Path -Leaf $expected) -match '^[0-9a-fA-F]{32}$') { $keep = $expected }
    }
    if (-not $keep -and $items.Count -gt 0) { $keep = $items[0].FullName }
    foreach ($item in $items) {
        if ($item.FullName -eq $keep -or $item.FullName.Equals($keep, [StringComparison]::OrdinalIgnoreCase)) {
            $script:skipped++
            continue
        }
        if ($DryRun) { Write-Host "Would remove: $($item.FullName)"; $script:deleted++; continue }
        try {
            Remove-Item -LiteralPath $item.FullName -Recurse -Force -ErrorAction Stop
            $script:deleted++
        }
        catch { Write-Warning "Could not remove $($item.Name): $($_.Exception.Message)"; $script:errors++ }
    }
}

Prune-Folder -Name 'runs' -KeepPath $KeepRunPath
Prune-Folder -Name 'f1-gates' -KeepPath $KeepGatePath
if ($RemovePublish) {
    $publish = Join-Path $cache 'publish'
    if (Test-Path -LiteralPath $publish -PathType Container) {
        $item = Get-Item -LiteralPath $publish -Force
        if (-not ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            if ($DryRun) { Write-Host "Would remove: $publish" }
            else {
                try { Remove-Item -LiteralPath $publish -Recurse -Force -ErrorAction Stop }
                catch { Write-Warning "Could not remove publish: $($_.Exception.Message)"; $errors++ }
            }
        }
    }
}
Write-Host "Validation cache: $deleted old runs/gates removed; $skipped preserved; $errors errors. NuGet cache untouched."
if ($null -ne $manualLock) { $manualLock.Dispose() }
if ($errors -gt 0) { throw "Could not remove $errors old validation cache folders." }
