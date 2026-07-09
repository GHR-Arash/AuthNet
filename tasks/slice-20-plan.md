# Slice 20 Plan: Committed Package Consumer Sample

## Overview

Add a committed package-consumer sample that proves AuthNet can be consumed through the primary NuGet-style integration package instead of monorepo project references. The sample should stay small, compile against locally packed AuthNet packages, and document the consumer workflow without requiring public NuGet publication.

## Scope

In scope:

- Add a committed ASP.NET Core sample app under `samples/` dedicated to package consumption.
- Reference AuthNet packages through package references, with `AuthNet.AspNetCore` as the primary dependency.
- Configure the sample with minimal `AddAuthNet`, `UseAuthentication`, `UseAuthorization`, and `MapAuthNet` setup.
- Include a minimal host-owned protected endpoint or page proving standard ASP.NET authorization still works.
- Add a repeatable local verification path that packs AuthNet packages, restores the sample from `artifacts/packages`, and builds the sample.
- Update docs and compact context to explain the committed package-consumer sample.

Out of scope:

- Publishing packages to NuGet.
- Changing package IDs, versioning, repository URL, license, or ownership metadata.
- Runtime PostgreSQL smoke testing for the package consumer sample.
- Adding new AuthNet product features.
- Admin JSON APIs, JWT, refresh tokens, cross-origin auth, or new persistence providers.

## Architecture Decisions

- Keep generated packages under ignored `artifacts/packages`; do not commit `.nupkg` files.
- Commit the sample source under `samples/AuthNet.PackageConsumer` or a similarly clear name.
- Prefer package references over project references in the committed sample to catch package dependency and asset problems.
- Use a repo-local NuGet configuration or documented restore command so the sample can restore from `artifacts/packages` after `scripts/verify.ps1` packs the packages.
- Keep the sample build-only by default; use PostgreSQL configuration placeholders but do not require a live database for verification unless a future slice explicitly scopes runtime smoke.
- Avoid adding the sample to `AuthNet.slnx` if that would make ordinary solution restore depend on generated local packages before they exist; instead use an explicit focused sample verification command.

## Candidate Shape

```text
samples/AuthNet.PackageConsumer/
  AuthNet.PackageConsumer.csproj
  NuGet.config
  Program.cs
  appsettings.json
  README.md
```

Expected package dependency:

```xml
<PackageReference Include="AuthNet.AspNetCore" Version="0.1.0" />
```

Expected verification flow:

```powershell
.\scripts\verify.ps1
.\scripts\verify-package-consumer.ps1
```

The checked-in sample-local `NuGet.config` scopes package sources to `samples/AuthNet.PackageConsumer` and does not affect root solution restore.

## Task List

### Phase 1: Sample Shape and Restore Strategy

- [x] Task 1: Decide and document the committed package-consumer sample restore strategy.
- [x] Task 2: Add the package-consumer sample project and minimal configuration.
- [x] Task 3: Add minimal host code using package-consumed AuthNet APIs.

### Checkpoint: Sample Foundation

- [x] Sample restores from freshly packed local AuthNet packages.
- [x] Sample builds without project references to AuthNet source projects.
- [x] Existing root solution restore/build behavior is not broken.

### Phase 2: Verification Automation

- [x] Task 4: Add a focused package-consumer verification command or script.
- [x] Task 5: Integrate the package-consumer check into the canonical verification path only if it does not create circular package prerequisites.
- [x] Task 6: Add docs for manual restore/build and troubleshooting.

### Checkpoint: Verification

- [x] The package-consumer sample can be verified from a clean package output.
- [x] Verification fails if required local packages are missing.
- [x] Generated packages and restore artifacts remain ignored.

### Phase 3: Docs, Context, and Closeout

- [x] Task 7: Update user/developer docs and package-readiness docs.
- [x] Task 8: Update `docs/architecture-context.md`, `docs/next-iteration-context.md`, and `context.md`.
- [x] Task 9: Run focused package-consumer verification and full local verification.
- [x] Task 10: Final diff review and commit.

### Checkpoint: Complete

- [x] Committed sample proves `AuthNet.AspNetCore` package consumption.
- [x] Package-consumer verification is documented and repeatable.
- [x] Full verification passes.
- [x] Slice 20 artifacts are complete and named with `slice-20`.
- [x] Publication, admin APIs, JWT, refresh tokens, and cross-origin auth remain explicitly deferred.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Root solution restore fails before packages are generated | High | Do not add the package-consumer sample to `AuthNet.slnx` unless restore can succeed without local packages. Use focused sample restore/build commands. |
| Sample accidentally uses project references | High | Assert the sample `.csproj` uses package references and no `ProjectReference` to AuthNet source projects. |
| Local packages become committed | Medium | Keep `.nupkg`, `bin`, `obj`, and `artifacts` ignored; commit source only. |
| Verification depends on public publication | Medium | Restore from `artifacts/packages` created by local pack commands. |
| Sample implies production-ready defaults | Medium | Keep config clearly sample-only and document that real consumers must provide PostgreSQL and production email sender settings. |

## Open Questions

- Should the package-consumer sample have a sample-local `NuGet.config`, or should docs/scripts pass `--source` explicitly? Decision: use a sample-local `NuGet.config` so local artifacts and nuget.org are both explicit without affecting root solution restore.
- Should the focused package-consumer check become part of `scripts/verify.ps1`? Default plan: add it only if it can run after package packing inside the script without disrupting existing restore order.
- Should the sample be runnable without PostgreSQL using InMemory? Default plan: no, keep InMemory as sample-host-only behavior and verify package consumption by build unless runtime smoke is explicitly scoped.
