Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$dotnet = Join-Path $repoRoot '.dotnet\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) {
    $dotnet = 'dotnet'
}

$packageOutput = Join-Path $repoRoot 'artifacts\packages'
$expectedPackages = @(
    'AuthNet.Core.0.1.0.nupkg',
    'AuthNet.ExternalProviders.0.1.0.nupkg',
    'AuthNet.Persistence.Postgres.0.1.0.nupkg',
    'AuthNet.UI.Razor.0.1.0.nupkg',
    'AuthNet.Api.0.1.0.nupkg',
    'AuthNet.AspNetCore.0.1.0.nupkg'
)

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Name,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    Write-Host "==> $Name"
    & $Command
}

function Invoke-DotNet {
    & $dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code $LASTEXITCODE`: $($args -join ' ')"
    }
}

Invoke-Step 'Restore' {
    Invoke-DotNet restore AuthNet.slnx
}

Invoke-Step 'Build Debug' {
    Invoke-DotNet build AuthNet.slnx --no-restore
}

Invoke-Step 'Test Debug' {
    Invoke-DotNet test AuthNet.slnx --no-build
}

Invoke-Step 'Build Release' {
    Invoke-DotNet build AuthNet.slnx --configuration Release --no-restore
}

Invoke-Step 'Clean package output' {
    New-Item -ItemType Directory -Force -Path $packageOutput | Out-Null
    Get-ChildItem -LiteralPath $packageOutput -Filter 'AuthNet.*.nupkg' -File |
        Remove-Item -Force
}

$packProjects = @(
    'src\AuthNet.Core\AuthNet.Core.csproj',
    'src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj',
    'src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj',
    'src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj',
    'src\AuthNet.Api\AuthNet.Api.csproj',
    'src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj'
)

foreach ($project in $packProjects) {
    Invoke-Step "Pack $project" {
        Invoke-DotNet pack $project --configuration Release --no-build --output $packageOutput
    }
}

Invoke-Step 'Verify package output' {
    $actualPackages = Get-ChildItem -LiteralPath $packageOutput -Filter 'AuthNet.*.nupkg' -File |
        Select-Object -ExpandProperty Name |
        Sort-Object
    $expectedSorted = $expectedPackages | Sort-Object

    $missing = @($expectedSorted | Where-Object { $_ -notin $actualPackages })
    $extra = @($actualPackages | Where-Object { $_ -notin $expectedSorted })

    if ($missing.Count -gt 0 -or $extra.Count -gt 0) {
        if ($missing.Count -gt 0) {
            Write-Error "Missing packages: $($missing -join ', ')"
        }

        if ($extra.Count -gt 0) {
            Write-Error "Unexpected packages: $($extra -join ', ')"
        }

        throw 'Package output verification failed.'
    }

    Write-Host "Verified packages: $($actualPackages -join ', ')"
}
