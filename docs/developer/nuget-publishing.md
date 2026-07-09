# NuGet Publishing Guide

AuthNet publishes packages automatically from GitHub Actions after a push or merge to `master`.

The automatic workflow is `.github/workflows/nuget-release.yml`. It runs the same local verification gate as CI, packs the expected packages, and publishes them to `https://api.nuget.org/v3/index.json`.

The workflow also supports manual runs through `workflow_dispatch`.

## Required GitHub Secret

Create a repository secret named:

```text
NUGET_API_KEY
```

The value must be a nuget.org API key with permission to push the `AuthNet.*` packages.

## 1. Package Metadata

Publication metadata is configured in `Directory.Build.props`:

```xml
<Authors>Arash Ghoreyhsie</Authors>
<RepositoryUrl>https://github.com/GHR-Arash/AuthNet</RepositoryUrl>
<PackageLicenseFile>LICENSE</PackageLicenseFile>
```

The repository root includes an MIT `LICENSE` file and packages it into every AuthNet NuGet package. Do not use the deprecated `LicenseUrl` metadata.

## 2. Run Verification

Run the full local gate:

```powershell
.\scripts\verify.ps1
```

This restores, builds, tests, packs all six packages, verifies package metadata, and checks the committed package-consumer sample.

Then run strict publication metadata verification:

```powershell
.\scripts\verify-package-metadata.ps1 -RequirePublicPublicationMetadata
```

Strict mode must pass before the GitHub release workflow publishes packages.

## 3. Create a NuGet API Key

On nuget.org:

- Sign in.
- Open the account menu.
- Select **API Keys**.
- Create a key with permission to push new packages and package versions.
- Scope it to `AuthNet.*` package IDs when possible.

Store it only in your local shell or secret manager:

```powershell
$env:NUGET_API_KEY = "your-api-key"
```

Do not commit API keys.

## 4. Push Packages

The workflow publishes packages in the order defined by `scripts/package-manifest.ps1`. For manual publishing, run verification and then push dependency packages first and `AuthNet.AspNetCore` last:

```powershell
.\.dotnet\dotnet.exe nuget push artifacts\packages\AuthNet.Core.0.1.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
.\.dotnet\dotnet.exe nuget push artifacts\packages\AuthNet.ExternalProviders.0.1.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
.\.dotnet\dotnet.exe nuget push artifacts\packages\AuthNet.Persistence.Postgres.0.1.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
.\.dotnet\dotnet.exe nuget push artifacts\packages\AuthNet.UI.Razor.0.1.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
.\.dotnet\dotnet.exe nuget push artifacts\packages\AuthNet.Api.0.1.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
.\.dotnet\dotnet.exe nuget push artifacts\packages\AuthNet.AspNetCore.0.1.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## 5. Package Ownership

NuGet ownership is not controlled by `<Authors>` or `<Owners>`.

- `<Authors>` controls the visible author metadata.
- The nuget.org account or API key used to publish becomes the package owner.
- To add or transfer owners later, use nuget.org: **Manage packages** -> select package -> **Manage package** -> **Owners**.

Do not add `<Owners>` expecting it to control nuget.org ownership; NuGet ignores that metadata for ownership.

## Notes

- NuGet package versions are immutable. If `0.1.0` is pushed with a problem, publish a fixed `0.1.1` or another new version.
- The automatic workflow uses `--skip-duplicate`, so rerunning the workflow does not fail only because a package version already exists on nuget.org.
- Packages may take several minutes to validate and appear in search.
- Current package publication readiness is tracked in `docs/slice-21/package-publication-finalization.md`.
