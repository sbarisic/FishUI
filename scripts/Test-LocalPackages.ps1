param([string]$Configuration = "Release")

$ErrorActionPreference = "Stop"
$repository = Split-Path -Parent $PSScriptRoot
$temporary = Join-Path ([System.IO.Path]::GetTempPath()) ("fishui-pack-smoke-" + [Guid]::NewGuid().ToString("N"))
$feed = Join-Path $temporary "feed"
$consumer = Join-Path $temporary "consumer"

try {
    New-Item -ItemType Directory -Path $feed, $consumer | Out-Null
    dotnet pack (Join-Path $repository "FishUI/FishUI.csproj") -c $Configuration -o $feed
    if ($LASTEXITCODE -ne 0) { throw "FishUI pack failed." }
    dotnet pack (Join-Path $repository "RaylibFishUI/RaylibFishGfx.csproj") -c $Configuration -o $feed
    if ($LASTEXITCODE -ne 0) { throw "RaylibFishGfx pack failed." }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $fishPackage = Join-Path $feed "FishUI.2.0.0.nupkg"
    $archive = [System.IO.Compression.ZipFile]::OpenRead($fishPackage)
    try {
        $names = @($archive.Entries | ForEach-Object FullName)
        foreach ($required in @("lib/net9.0/FishUI.dll", "NUGET_FISHUI_README.md", "build/FishUI.props", "build/FishUI.targets", "data/themes/gwen.yaml")) {
            if ($required -notin $names) { throw "FishUI package is missing $required." }
        }
    }
    finally {
        $archive.Dispose()
    }

    dotnet new console --framework net9.0 --no-restore --output $consumer | Out-Null
    dotnet add (Join-Path $consumer "consumer.csproj") package FishUI --version 2.0.0 --source $feed --no-restore
    dotnet add (Join-Path $consumer "consumer.csproj") package RaylibFishGfx --version 2.0.0 --source $feed --no-restore
    $nugetConfig = Join-Path $temporary "NuGet.Config"
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath $nugetConfig -Encoding utf8
    dotnet restore (Join-Path $consumer "consumer.csproj") --configfile $nugetConfig
    if ($LASTEXITCODE -ne 0) { throw "Local package restore failed." }
    dotnet build (Join-Path $consumer "consumer.csproj") -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Local package consumer build failed." }
}
finally {
    if (Test-Path -LiteralPath $temporary) {
        Remove-Item -LiteralPath $temporary -Recurse -Force
    }
}
