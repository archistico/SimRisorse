param([switch]$DryRun)

# Deletes the bin and obj folders next to every .csproj in the repository.
# Only folders named exactly bin/obj, directly beside a project file and inside the repository, are touched.
# .git and .local-cache (NuGet packages, validation artifacts) are never scanned; links/junctions are skipped.
$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path
$excluded = @('.git', '.local-cache') | ForEach-Object { Join-Path $repositoryRoot $_ }

$projects = Get-ChildItem -LiteralPath $repositoryRoot -Recurse -File -Filter '*.csproj' | Where-Object {
    $path = $_.FullName
    -not ($excluded | Where-Object { $path.StartsWith($_ + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) })
}

$removed = 0; $skipped = 0; $failed = 0
foreach ($project in $projects) {
    foreach ($name in @('bin', 'obj')) {
        $target = Join-Path $project.DirectoryName $name
        if (-not (Test-Path -LiteralPath $target -PathType Container)) { continue }
        $item = Get-Item -LiteralPath $target -Force
        $relative = $item.FullName.Substring($repositoryRoot.Length + 1)
        if (-not $item.FullName.StartsWith($repositoryRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
            ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            Write-Warning "Skipped (outside the repository or a link): $relative"
            $skipped++
            continue
        }
        if ($DryRun) { Write-Host "Would remove $relative"; $removed++; continue }
        try {
            Remove-Item -LiteralPath $item.FullName -Recurse -Force
            Write-Host "Removed $relative"
            $removed++
        }
        catch {
            Write-Warning "Could not remove ${relative}: $($_.Exception.Message) (is the game, an IDE or a test host still running?)"
            $failed++
        }
    }
}

$verb = if ($DryRun) { 'to remove' } else { 'removed' }
Write-Host "$($projects.Count) projects scanned, $removed folders $verb, $skipped skipped, $failed failed."
if ($failed -gt 0) { exit 1 }
