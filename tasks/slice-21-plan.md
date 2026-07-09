# Slice 21 Plan: Package Publication Finalization

## Overview

Tighten AuthNet package publication readiness without publishing packages or inventing owner/legal metadata. The slice adds repeatable package metadata verification, records remaining owner-required publication decisions, and keeps canonical verification aligned with the six current package artifacts.

## Scope

In scope:

- Add package metadata validation for locally packed `.nupkg` files.
- Share package IDs, version, expected package files, and pack project list across verification scripts.
- Update package documentation and manual commands for the current six-package shape, including `AuthNet.Api`.
- Add a strict publication metadata gate that can be enabled once repository URL and license decisions are known.
- Update compact context after verification.

Out of scope:

- Publishing packages to NuGet.
- Adding NuGet API keys, trusted publishing, signing, or tag-release automation.
- Guessing repository URL, license expression/file, final owners, or package ownership.
- Enabling XML documentation generation before the missing-comment policy is decided.
- Changing package IDs, version, public APIs, authentication flows, or product scope.

## Architecture Decisions

- Keep `Directory.Build.props` as the central MSBuild package metadata location.
- Keep generated packages under ignored `artifacts/packages`.
- Keep public publication metadata that requires owner/legal input absent until explicitly decided.
- Add `scripts/package-manifest.ps1` as the shared verification manifest for package IDs, version, pack projects, and expected `.nupkg` files.
- Add `scripts/verify-package-metadata.ps1` as the local metadata validator.
- Keep `.\scripts\verify.ps1` as the canonical all-up local gate.

## Task List

### Phase 1: Verification Manifest

- [x] Task 1: Add shared package manifest for package verification scripts.
- [x] Task 2: Update canonical package output and package-consumer checks to use the shared manifest.

### Phase 2: Metadata Gate

- [x] Task 3: Add package metadata verifier for `.nuspec`, README, and library assets.
- [x] Task 4: Integrate package metadata verification into `scripts/verify.ps1`.
- [x] Task 5: Add strict public-publication mode for future repository URL and license enforcement.

### Phase 3: Docs and Context

- [x] Task 6: Update package docs and commands for the current six-package shape.
- [x] Task 7: Document final publication blockers and the metadata verification command.
- [x] Task 8: Update compact context files.

### Phase 4: Verification and Closeout

- [x] Task 9: Run focused metadata/package-consumer checks and full verification.
- [x] Task 10: Review Slice 21 and prepare the final commit.

## Checkpoint: Complete

- [x] Canonical verification validates package output, package metadata, and package-consumer restore/build.
- [x] Package publication blockers are explicit and not guessed.
- [x] Manual package commands list all six packable packages.
- [x] Generated `.nupkg` files remain ignored.
- [x] NuGet publication, signing, trusted publishing, and release automation remain deferred.

## Remaining Owner Decisions

Before public NuGet publication, confirm:

- Public repository URL.
- License expression or license file.
- Final package authors/owners.
- XML documentation generation policy.
- Whether packages should be signed.
- Whether CI should publish on tags through trusted publishing.
