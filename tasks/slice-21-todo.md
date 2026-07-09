# Slice 21 Todo: Package Publication Finalization

## Task 1: Add shared package manifest

**Description:** Add a script-owned manifest for package IDs, version, expected package files, and pack projects.

**Acceptance criteria:**

- [x] Manifest lists all six packable packages.
- [x] Manifest version matches `Directory.Build.props`.
- [x] Verification scripts can dot-source the manifest.

**Verification:**

- [x] Manual script review confirms no stale five-package list remains in verification scripts.

## Task 2: Update verification scripts to use manifest

**Description:** Remove duplicated package lists from canonical and package-consumer verification.

**Acceptance criteria:**

- [x] `scripts/verify.ps1` uses the shared pack project list.
- [x] `scripts/verify.ps1` uses the shared expected package file list.
- [x] `scripts/verify-package-consumer.ps1` uses the shared expected package file list.

**Verification:**

- [x] `.\scripts\verify-package-consumer.ps1`

## Task 3: Add package metadata verifier

**Description:** Add a focused package verifier that opens generated `.nupkg` files and validates key `.nuspec` and package asset metadata.

**Acceptance criteria:**

- [x] Verifies package id and version.
- [x] Verifies authors, description, tags, release notes, readme metadata, and repository type.
- [x] Verifies packaged `README.md`.
- [x] Verifies `lib/net10.0/{PackageId}.dll`.
- [x] Fails clearly when expected packages are missing.

**Verification:**

- [x] `.\scripts\verify-package-metadata.ps1`

## Task 4: Add strict public publication mode

**Description:** Add an opt-in strict mode for metadata that cannot be finalized without owner/legal decisions.

**Acceptance criteria:**

- [x] Strict mode requires repository URL.
- [x] Strict mode requires license expression or license file.
- [x] Default local verification does not require guessed public metadata.

**Verification:**

- [x] Manual script review confirms strict mode is opt-in.

## Task 5: Integrate metadata verification

**Description:** Add package metadata verification to the canonical full verification path.

**Acceptance criteria:**

- [x] `scripts/verify.ps1` runs metadata verification after package output verification.
- [x] Package-consumer verification still runs after package metadata verification.
- [x] Existing package output validation remains intact.

**Verification:**

- [x] `.\scripts\verify.ps1`

## Task 6: Update package docs

**Description:** Update docs and manual commands to reflect six packable packages and the new metadata verifier.

**Acceptance criteria:**

- [x] Root README pack commands include `AuthNet.Api`.
- [x] Developer quick start documents package metadata verification.
- [x] Slice 05 publication-readiness docs distinguish local metadata verification from public publication metadata blockers.
- [x] Package readiness docs mention metadata verification.

**Verification:**

- [x] Manual doc read-through confirms package commands match current project state.

## Task 7: Update compact context

**Description:** Record Slice 21 package publication finalization state in compact project memory.

**Acceptance criteria:**

- [x] `docs/architecture-context.md` lists the metadata verification command.
- [x] `docs/next-iteration-context.md` records Slice 21 completion.
- [x] `context.md` records the current package publication gate and blockers.

**Verification:**

- [x] Manual context read-through confirms no expanded spec content was added.

## Task 8: Run final verification and review

**Description:** Run focused and full verification, check whitespace, review staged diff, and prepare the final commit.

**Acceptance criteria:**

- [x] Focused package metadata verification passes.
- [x] Focused package-consumer verification passes.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Slice 21 changes are staged without unrelated local changes.

**Verification:**

- [x] `.\scripts\verify-package-metadata.ps1`
- [x] `.\scripts\verify-package-consumer.ps1`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
