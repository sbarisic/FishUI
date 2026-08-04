$ErrorActionPreference = "Stop"
$repository = Split-Path -Parent $PSScriptRoot
$core = Join-Path $repository "FishUI"
$project = Join-Path $repository "UnityFishUI/UnityFishUI.csproj"
[xml]$xml = Get-Content -LiteralPath $project
$linked = @{}
foreach ($compile in $xml.Project.ItemGroup.Compile) {
    if ($compile.Include -like "..\FishUI\*.cs" -or $compile.Include -like "..\FishUI\**\*.cs") {
        $resolved = [System.IO.Path]::GetFullPath((Join-Path (Split-Path $project) $compile.Include))
        $linked[$resolved] = $true
    }
}
$missing = Get-ChildItem -LiteralPath $core -Filter *.cs -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and -not $linked.ContainsKey($_.FullName)
}
if ($missing) {
    throw "UnityFishUI is missing linked sources:`n$($missing.FullName -join "`n")"
}
Write-Output "UnityFishUI source links are synchronized."
