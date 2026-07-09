Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$dotnet = Join-Path $repoRoot '.dotnet\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) {
    $dotnet = 'dotnet'
}

$sampleProject = Join-Path $repoRoot 'samples\AuthNet.PackageConsumer\AuthNet.PackageConsumer.csproj'
$sampleNuGetConfig = Join-Path $repoRoot 'samples\AuthNet.PackageConsumer\NuGet.config'
$packageOutput = Join-Path $repoRoot 'artifacts\packages'
$expectedPackages = @(
    'AuthNet.Core.0.1.0.nupkg',
    'AuthNet.ExternalProviders.0.1.0.nupkg',
    'AuthNet.Persistence.Postgres.0.1.0.nupkg',
    'AuthNet.UI.Razor.0.1.0.nupkg',
    'AuthNet.Api.0.1.0.nupkg',
    'AuthNet.AspNetCore.0.1.0.nupkg'
)

function Invoke-DotNet {
    & $dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code $LASTEXITCODE`: $($args -join ' ')"
    }
}

if (-not (Test-Path -LiteralPath $sampleProject)) {
    throw "Package consumer sample project was not found: $sampleProject"
}

if (-not (Test-Path -LiteralPath $sampleNuGetConfig)) {
    throw "Package consumer NuGet config was not found: $sampleNuGetConfig"
}

if (-not (Test-Path -LiteralPath $packageOutput)) {
    throw "Local package output was not found. Run .\scripts\verify.ps1 first to create packages."
}

$actualPackages = Get-ChildItem -LiteralPath $packageOutput -Filter 'AuthNet.*.nupkg' -File |
    Select-Object -ExpandProperty Name
$missing = @($expectedPackages | Where-Object { $_ -notin $actualPackages })
if ($missing.Count -gt 0) {
    throw "Missing local packages: $($missing -join ', '). Run .\scripts\verify.ps1 first."
}

Write-Host '==> Restore package consumer sample'
Invoke-DotNet restore $sampleProject --configfile $sampleNuGetConfig

Write-Host '==> Build package consumer sample'
Invoke-DotNet build $sampleProject --no-restore
