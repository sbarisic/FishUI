param(
    [string]$Configuration = "Debug",
    [double]$MinimumLineCoverage = 55,
    [double]$MinimumBranchCoverage = 45,
    [string]$ResultsDirectory
)

$ErrorActionPreference = "Stop"
$repository = Split-Path -Parent $PSScriptRoot
$ownsResults = [string]::IsNullOrWhiteSpace($ResultsDirectory)
if ($ownsResults) {
    $ResultsDirectory = Join-Path ([System.IO.Path]::GetTempPath()) (
        "fishui-coverage-gate-" + [Guid]::NewGuid().ToString("N"))
}
else {
    $ResultsDirectory = [System.IO.Path]::GetFullPath(
        (Join-Path (Get-Location) $ResultsDirectory))
}

try {
    New-Item -ItemType Directory -Path $ResultsDirectory -Force | Out-Null
    dotnet test (Join-Path $repository "UnitTest/UnitTest.csproj") `
        -c $Configuration `
        --no-build `
        --no-restore `
        --collect:"XPlat Code Coverage" `
        --results-directory $ResultsDirectory `
        --logger "console;verbosity=minimal"
    if ($LASTEXITCODE -ne 0) { throw "Coverage test run failed." }

    $coverageFile = Get-ChildItem $ResultsDirectory -Recurse `
        -Filter coverage.cobertura.xml | Select-Object -First 1
    if ($null -eq $coverageFile) { throw "The coverage collector produced no Cobertura report." }

    [xml]$coverage = Get-Content -LiteralPath $coverageFile.FullName
    $linePercent = [double]$coverage.coverage.'line-rate' * 100
    $branchPercent = [double]$coverage.coverage.'branch-rate' * 100
    Write-Host ("FishUI coverage: {0:N2}% line, {1:N2}% branch" -f $linePercent, $branchPercent)

    if ($linePercent -lt $MinimumLineCoverage) {
        throw ("Line coverage {0:N2}% is below the {1:N2}% floor." -f $linePercent, $MinimumLineCoverage)
    }
    if ($branchPercent -lt $MinimumBranchCoverage) {
        throw ("Branch coverage {0:N2}% is below the {1:N2}% floor." -f $branchPercent, $MinimumBranchCoverage)
    }
}
finally {
    if ($ownsResults -and (Test-Path -LiteralPath $ResultsDirectory)) {
        $resolvedResults = [System.IO.Path]::GetFullPath($ResultsDirectory)
        $resolvedTemp = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if (-not $resolvedResults.StartsWith($resolvedTemp, [StringComparison]::OrdinalIgnoreCase) -or
            [System.IO.Path]::GetFileName($resolvedResults) -notlike "fishui-coverage-gate-*") {
            throw "Refusing to remove an unexpected coverage directory: $resolvedResults"
        }
        Remove-Item -LiteralPath $resolvedResults -Recurse -Force
    }
}
