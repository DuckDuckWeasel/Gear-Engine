$roots = @(
    "$PSScriptRoot\LiveOps",
    "$PSScriptRoot\Assets\Packages\com.scaffold.liveops",
    "$PSScriptRoot\Assets\LiveOps",
    "$PSScriptRoot\Assets\GearEngine\Scripts"
)
foreach ($r in $roots) {
    if (-not (Test-Path $r)) { continue }
    $empty = Get-ChildItem $r -Recurse -Directory -ErrorAction SilentlyContinue |
        Where-Object { @(Get-ChildItem $_.FullName -Force -ErrorAction SilentlyContinue).Count -eq 0 }
    foreach ($d in $empty) {
        Remove-Item -LiteralPath $d.FullName -Force -ErrorAction SilentlyContinue
        $meta = $d.FullName + '.meta'
        if (Test-Path -LiteralPath $meta) {
            Remove-Item -LiteralPath $meta -Force -ErrorAction SilentlyContinue
        }
    }
}
