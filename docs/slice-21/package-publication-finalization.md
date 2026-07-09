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

Strict future public-publication metadata verification:

```powershell
.\scripts\verify-package-metadata.ps1 -RequirePublicPublicationMetadata
```

The strict mode is expected to fail until repository URL and license metadata are explicitly decided.

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

Before public NuGet publication, confirm:

- public repository URL;
- license expression or license file;
- final package authors/owners;
- XML documentation generation policy;
- package signing policy;
- CI tag-publish or trusted-publishing strategy.

## Explicit Non-Goals

- No NuGet publishing in this slice.
- No package signing in this slice.
- No trusted publishing setup in this slice.
- No guessed repository URL or license metadata.
- No JWT, refresh tokens, admin JSON APIs, or other product scope changes.
