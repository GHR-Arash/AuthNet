param(
    [Parameter(Mandatory = $true)]
    [string] $ApiKey,

    [string] $Source = 'https://api.nuget.org/v3/index.json',

    [switch] $SkipDuplicate
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$dotnet = Join-Path $repoRoot '.dotnet\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) {
    $dotnet = 'dotnet'
}

. (Join-Path $repoRoot 'scripts\package-manifest.ps1')

$packageOutput = Join-Path $repoRoot 'artifacts\packages'
if (-not (Test-Path -LiteralPath $packageOutput)) {
    throw "Package output was not found: $packageOutput. Run .\scripts\verify.ps1 first."
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw 'A NuGet API key is required.'
}

function Invoke-DotNet {
    & $dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code $LASTEXITCODE`: $($args -join ' ')"
    }
}

foreach ($packageFile in $AuthNetExpectedPackageFiles) {
    $packagePath = Join-Path $packageOutput $packageFile
    if (-not (Test-Path -LiteralPath $packagePath)) {
        throw "Expected package was not found: $packagePath"
    }

    $pushArguments = @(
        'nuget',
        'push',
        $packagePath,
        '--api-key',
        $ApiKey,
        '--source',
        $Source
    )

    if ($SkipDuplicate) {
        $pushArguments += '--skip-duplicate'
    }

    Write-Host "Publishing $packageFile"
    Invoke-DotNet @pushArguments
}
