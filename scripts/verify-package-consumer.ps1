Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$dotnet = Join-Path $repoRoot '.dotnet\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) {
    $dotnet = 'dotnet'
}

. (Join-Path $repoRoot 'scripts\package-manifest.ps1')

$sampleProject = Join-Path $repoRoot 'samples\AuthNet.PackageConsumer\AuthNet.PackageConsumer.csproj'
$sampleNuGetConfig = Join-Path $repoRoot 'samples\AuthNet.PackageConsumer\NuGet.config'
$packageOutput = Join-Path $repoRoot 'artifacts\packages'
$samplePackagesPath = Join-Path $repoRoot 'artifacts\package-consumer-packages'

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
$missing = @($AuthNetExpectedPackageFiles | Where-Object { $_ -notin $actualPackages })
if ($missing.Count -gt 0) {
    throw "Missing local packages: $($missing -join ', '). Run .\scripts\verify.ps1 first."
}

Write-Host '==> Restore package consumer sample'
$globalPackagesLine = & $dotnet nuget locals global-packages --list
if ($LASTEXITCODE -ne 0) {
    throw "dotnet command failed with exit code $LASTEXITCODE`: nuget locals global-packages --list"
}

$globalPackages = ($globalPackagesLine -replace '^global-packages:\s*', '').Trim()
$resolvedSamplePackages = [System.IO.Path]::GetFullPath($samplePackagesPath)
$resolvedRepoRoot = [System.IO.Path]::GetFullPath($repoRoot)
if (-not $resolvedSamplePackages.StartsWith($resolvedRepoRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clean package-consumer cache outside repository: $resolvedSamplePackages"
}

New-Item -ItemType Directory -Force -Path $resolvedSamplePackages | Out-Null
Get-ChildItem -LiteralPath $resolvedSamplePackages -Directory -Filter 'authnet.*' -ErrorAction SilentlyContinue |
    Remove-Item -Recurse -Force

$restoreArgs = @('restore', $sampleProject, '--configfile', $sampleNuGetConfig, '--packages', $resolvedSamplePackages)
if (-not [string]::IsNullOrWhiteSpace($globalPackages) -and (Test-Path -LiteralPath $globalPackages)) {
    $restoreArgs += "/p:RestoreFallbackFolders=$globalPackages"
}

Invoke-DotNet @restoreArgs

Write-Host '==> Build package consumer sample'
Invoke-DotNet build $sampleProject --no-restore
