param(
    [switch] $RequirePublicPublicationMetadata
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

. (Join-Path $repoRoot 'scripts\package-manifest.ps1')

$packageOutput = Join-Path $repoRoot 'artifacts\packages'
$requiredTags = @('auth', 'identity', 'aspnetcore', 'razor-pages', 'postgresql')

if (-not (Test-Path -LiteralPath $packageOutput)) {
    throw "Local package output was not found. Run .\scripts\verify.ps1 first to create packages."
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-ZipEntryText {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.Compression.ZipArchive] $Archive,

        [Parameter(Mandatory = $true)]
        [string] $EntryName
    )

    $entry = $Archive.GetEntry($EntryName)
    if ($null -eq $entry) {
        return $null
    }

    $reader = [System.IO.StreamReader]::new($entry.Open())
    try {
        return $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }
}

function Get-NuspecText {
    param(
        [Parameter(Mandatory = $true)]
        [xml] $Nuspec,

        [Parameter(Mandatory = $true)]
        [string] $ElementName
    )

    $node = $Nuspec.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='$ElementName']")
    if ($null -eq $node) {
        return $null
    }

    return $node.InnerText
}

function Get-NuspecAttribute {
    param(
        [Parameter(Mandatory = $true)]
        [xml] $Nuspec,

        [Parameter(Mandatory = $true)]
        [string] $ElementName,

        [Parameter(Mandatory = $true)]
        [string] $AttributeName
    )

    $node = $Nuspec.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='$ElementName']")
    if ($null -eq $node) {
        return $null
    }

    return $node.GetAttribute($AttributeName)
}

foreach ($packageId in $AuthNetPackageIds) {
    $packagePath = Join-Path $packageOutput "$packageId.$AuthNetPackageVersion.nupkg"
    if (-not (Test-Path -LiteralPath $packagePath)) {
        throw "Expected package was not found: $packagePath"
    }

    $archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $nuspecName = "$packageId.nuspec"
        $nuspecText = Get-ZipEntryText -Archive $archive -EntryName $nuspecName
        if ([string]::IsNullOrWhiteSpace($nuspecText)) {
            throw "Package $packageId is missing $nuspecName."
        }

        [xml] $nuspec = $nuspecText
        $id = Get-NuspecText -Nuspec $nuspec -ElementName 'id'
        $version = Get-NuspecText -Nuspec $nuspec -ElementName 'version'
        $authors = Get-NuspecText -Nuspec $nuspec -ElementName 'authors'
        $description = Get-NuspecText -Nuspec $nuspec -ElementName 'description'
        $releaseNotes = Get-NuspecText -Nuspec $nuspec -ElementName 'releaseNotes'
        $readme = Get-NuspecText -Nuspec $nuspec -ElementName 'readme'
        $tagsText = Get-NuspecText -Nuspec $nuspec -ElementName 'tags'
        $repositoryType = Get-NuspecAttribute -Nuspec $nuspec -ElementName 'repository' -AttributeName 'type'
        $repositoryUrl = Get-NuspecAttribute -Nuspec $nuspec -ElementName 'repository' -AttributeName 'url'
        $licenseText = Get-NuspecText -Nuspec $nuspec -ElementName 'license'
        $licenseType = Get-NuspecAttribute -Nuspec $nuspec -ElementName 'license' -AttributeName 'type'

        if ($id -ne $packageId) {
            throw "Package $packageId has unexpected id '$id'."
        }

        if ($version -ne $AuthNetPackageVersion) {
            throw "Package $packageId has unexpected version '$version'."
        }

        if ([string]::IsNullOrWhiteSpace($authors)) {
            throw "Package $packageId is missing authors metadata."
        }

        if ([string]::IsNullOrWhiteSpace($description)) {
            throw "Package $packageId is missing description metadata."
        }

        if ([string]::IsNullOrWhiteSpace($releaseNotes)) {
            throw "Package $packageId is missing release notes metadata."
        }

        if ($readme -ne 'README.md') {
            throw "Package $packageId should use README.md as PackageReadmeFile."
        }

        $tags = "$tagsText" -split '\s+' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        $missingTags = @($requiredTags | Where-Object { $_ -notin $tags })
        if ($missingTags.Count -gt 0) {
            throw "Package $packageId is missing tags: $($missingTags -join ', ')."
        }

        if ($null -eq $archive.GetEntry('README.md')) {
            throw "Package $packageId is missing packaged README.md."
        }

        $libraryPath = "lib/net10.0/$packageId.dll"
        if ($null -eq $archive.GetEntry($libraryPath)) {
            throw "Package $packageId is missing $libraryPath."
        }

        if ($repositoryType -ne 'git') {
            throw "Package $packageId should declare repository type 'git'."
        }

        if ($RequirePublicPublicationMetadata) {
            if ([string]::IsNullOrWhiteSpace($repositoryUrl)) {
                throw "Package $packageId is missing repository URL metadata required for public publication."
            }

            $hasLicenseExpression = $licenseType -eq 'expression' -and -not [string]::IsNullOrWhiteSpace($licenseText)
            $hasLicenseFile = $licenseType -eq 'file' -and -not [string]::IsNullOrWhiteSpace($licenseText)
            if (-not $hasLicenseExpression -and -not $hasLicenseFile) {
                throw "Package $packageId is missing license metadata required for public publication."
            }

            if ($hasLicenseFile -and $null -eq $archive.GetEntry($licenseText)) {
                throw "Package $packageId references license file '$licenseText' but does not package it."
            }
        }

        Write-Host "Verified metadata for $packageId"
    }
    finally {
        $archive.Dispose()
    }
}

if (-not $RequirePublicPublicationMetadata) {
    Write-Host 'Public repository URL and license metadata are owner-required and are not enforced by this local gate.'
}
