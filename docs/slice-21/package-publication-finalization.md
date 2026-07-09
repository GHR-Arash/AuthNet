# Slice 21 Package Publication Finalization

Slice 21 tightens package publication readiness without publishing packages or inventing owner/legal metadata.

## Current Local Gate

Canonical verification:

```powershell
.\scripts\verify.ps1
```

This now verifies:

- solution restore, Debug build, tests, and Release build;
- six package artifacts under `artifacts/packages`;
- package metadata and packaged assets;
- committed package-consumer restore/build.

Focused metadata verification:

```powershell
.\scripts\verify-package-metadata.ps1
```

Strict public-publication metadata verification:

```powershell
.\scripts\verify-package-metadata.ps1 -RequirePublicPublicationMetadata
```

The strict mode must pass before publishing packages.

Manual publishing steps are documented in `docs/developer/nuget-publishing.md`.

## Validated Package Metadata

For each generated package, the focused verifier checks:

- package id;
- version;
- authors;
- description;
- tags;
- release notes;
- README metadata and packaged `README.md`;
- `lib/net10.0/{PackageId}.dll`;
- repository type.

## Remaining Public Publication Decisions

Public NuGet publication metadata now includes:

- repository URL: `https://github.com/GHR-Arash/AuthNet`;
- MIT license file: `LICENSE`;
- package authors: `Arash Ghoreyhsie`;

Before changing publication policy, confirm:

- XML documentation generation policy;
- package signing policy;
- CI tag-publish or trusted-publishing strategy.

## Explicit Non-Goals

- No manual NuGet publishing in this slice.
- No package signing in this slice.
- No trusted publishing setup in this slice.
- No guessed repository URL or license metadata remains; repository URL and license file are configured.
- No JWT, refresh tokens, admin JSON APIs, or other product scope changes.
