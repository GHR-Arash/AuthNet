# Slice 05: CI and Publication Metadata Readiness

## Intent

Add repeatable verification locally and in CI, then document the remaining decisions required before AuthNet packages can be published publicly.

## Proposed CI Coverage

The workflow verifies:

- Restore.
- Debug build.
- Tests.
- Release build.
- Pack the six intended packages.
- Confirm package output contains exactly:
  - `AuthNet.Core`
  - `AuthNet.ExternalProviders`
  - `AuthNet.Persistence.Postgres`
  - `AuthNet.UI.Razor`
  - `AuthNet.Api`
  - `AuthNet.AspNetCore`

The workflow does not publish packages, upload artifacts, or require secrets in this slice.

## Proposed Local Script

Added:

```text
scripts/verify.ps1
```

The script should be the contributor-friendly equivalent of the CI verification path.

Current script behavior:

- Uses `.dotnet\dotnet.exe` when present, otherwise falls back to `dotnet`.
- Restores the solution.
- Builds Debug.
- Runs tests.
- Builds Release.
- Packs the six intended package projects.
- Verifies exactly the expected six `AuthNet.*.nupkg` files are present.
- Verifies generated package metadata and packaged assets.
- Verifies the committed package-consumer sample restores/builds from local package artifacts.

## Publication Metadata State

Confirmed:

- Package IDs are aligned with project names.
- Version is currently `0.1.0`.
- Package readme is the root `README.md`.
- Package artifacts are generated under ignored `artifacts/packages`.
- Package release notes are set to a neutral MVP package-readiness description.
- Local package metadata verification checks `.nuspec`, README, tags, descriptions, release notes, repository type, and package library assets.

Needs owner decision before public publication:

- Public repository URL.
- License expression or license file.
- Final package authors/owners.
- Whether XML documentation generation is required before publishing.
- Whether packages should be signed before publishing.
- Whether CI should later upload package artifacts or publish on tags.

Current explicit deferrals:

- No fake repository URL is configured.
- No license expression or license file is configured.
- XML documentation generation is not enabled yet.
- Strict public-publication metadata verification is available through `.\scripts\verify-package-metadata.ps1 -RequirePublicPublicationMetadata`, but is expected to fail until repository URL and license metadata are finalized.

## Non-Goals

- No NuGet publishing.
- No API keys or trusted publishing setup.
- No package ID rename unless a conflict is found in a later publication slice.
- No new AuthNet product scope.
