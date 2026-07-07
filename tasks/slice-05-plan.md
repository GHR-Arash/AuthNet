# Slice 05 Plan: CI and Publication Metadata Readiness

## Overview

Slice 05 adds repeatable automated verification for AuthNet and prepares package metadata for eventual public publishing. The slice should introduce a local verification script, a GitHub Actions workflow, and explicit publication metadata decisions without publishing packages. It should preserve the current package shape and avoid adding new product/authentication scope.

## Scope

In scope:

- Add a single local verification script that runs restore, build, test, Release build, package creation, and package output checks.
- Add a GitHub Actions workflow that runs the same core verification on pushes and pull requests.
- Decide and record package metadata requirements for repository URL, license, authors, and XML documentation.
- Add or defer license metadata explicitly; do not invent a public license silently.
- Update developer docs and compact context docs with the new canonical verification path.

Out of scope:

- Publishing packages to NuGet.
- Adding NuGet API keys, signing, trusted publishing, release tags, or automated release uploads.
- Changing package IDs or adding new packages.
- Adding API/JWT/SPA flows or any new account behavior.
- Replacing the project-local SDK for local development.

## Architecture Decisions

- CI should use the official .NET SDK setup on hosted runners instead of committing SDK binaries.
- The local script should live under `scripts/` and be the canonical source for developer verification.
- CI can call equivalent commands directly or invoke the script if runner shell compatibility is clean.
- Package artifacts should be generated but not committed.
- Publication metadata that requires owner input should be documented as blocked/deferred rather than guessed.

## Task List

### Phase 1: Verification Script

- [x] Task 1: Add local verification script.
- [x] Task 2: Add package output inspection to local verification.

### Checkpoint: Local Verification

- [x] `.\scripts\verify.ps1` passes locally.
- [x] Generated packages remain under ignored `artifacts/packages`.

### Phase 2: CI Workflow

- [x] Task 3: Add GitHub Actions build workflow.
- [x] Task 4: Add CI package artifact checks.

### Checkpoint: CI Shape

- [x] Workflow YAML is syntactically valid.
- [x] Workflow does not require secrets.
- [x] Workflow verifies restore, build, test, Release build, and package creation.

### Phase 3: Publication Metadata

- [x] Task 5: Record publication metadata decisions.
- [x] Task 6: Add safe package metadata improvements that do not require guessing.

### Checkpoint: Metadata

- [x] Repository URL/license decisions are documented.
- [x] Package metadata remains valid for local pack.
- [x] XML documentation decision is recorded.

### Phase 4: Documentation and Closeout

- [x] Task 7: Update developer docs and README.
- [x] Task 8: Update architecture, next-iteration context, and `context.md`.
- [x] Task 9: Final verification and local review.
- [x] Task 10: Commit Slice 05.

### Checkpoint: Complete

- [x] Local verification passes.
- [x] CI workflow exists and matches local verification intent.
- [x] Docs identify any remaining publication blockers.
- [x] Slice 05 artifacts are complete and named with `slice-05`.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| CI cannot install .NET 10 if hosted runner setup changes | Medium | Use `actions/setup-dotnet` with the SDK version from `global.json` if added, or a pinned SDK version matching `.dotnet`. |
| Workflow tries to use project-local `.dotnet` on Linux | Medium | CI should install SDK independently; local docs can still use `.dotnet\dotnet.exe`. |
| Publication metadata is guessed incorrectly | Medium | Document owner-required values and avoid adding false repository/license metadata. |
| Pack verification diverges between local and CI | Medium | Keep command lists aligned and document one canonical sequence. |
| Package artifacts pollute git | Low | Keep `artifacts/` ignored and verify package output location. |

## Open Questions

- What public repository URL should package metadata use?
- Which license should AuthNet use?
- Should XML documentation warnings be enabled now or deferred until API naming is stable?
- Should CI upload package artifacts for pull requests, or only verify they can be created?

## Recommended Default

Use Slice 05 to add CI verification and document publication blockers. Defer public license/repository metadata values until the owner confirms them.
