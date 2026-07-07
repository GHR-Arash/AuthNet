# Slice 03 Plan: Package Readiness

## Overview

Slice 03 prepares AuthNet to be consumed as NuGet packages instead of only as project references. The slice should make package boundaries explicit, add package metadata, verify `dotnet pack`, review the public API surface, and update consumer documentation. It should not add new auth flows, API/JWT support, SPA support, or provider-specific helpers.

## Scope

In scope:

- Decide which projects are packable packages for MVP.
- Add shared package metadata and per-package descriptions.
- Ensure package assets include Razor UI and EF Core migrations where needed.
- Add repeatable pack verification.
- Review public APIs exposed by the packable projects.
- Update consumer docs for package-based installation.

Out of scope:

- Publishing packages to NuGet.
- Signing, symbols, SourceLink, or CI publishing automation unless needed for local pack validation.
- API/JWT, refresh tokens, SPA flows, MFA, multi-tenancy, SAML, passkeys, or admin UI.
- Renaming package IDs after they are selected unless the plan uncovers a concrete conflict.

## Package Boundary Decision

Proposed MVP packable packages:

- `AuthNet.Core`
- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Persistence.Postgres`
- `AuthNet.ExternalProviders`

Non-packable projects:

- `AuthNet.SampleHost`
- `AuthNet.Tests`

Rationale:

- The current project layout already matches the documented package shape.
- `AuthNet.AspNetCore` remains the primary integration entry point.
- UI, persistence, and external provider support stay separate so a host can understand optional dependencies.
- `AuthNet.Api` remains deferred because API/JWT scope is explicitly future work.

## Architecture Decisions

- Use centralized MSBuild metadata where possible, likely `Directory.Build.props`, to avoid repeated package settings.
- Keep package references explicit in the individual `.csproj` files when they are project-specific.
- Prefer local pack verification before any publishing setup.
- Preserve project references for repository development; packages are for consumers.
- Treat public API review as an MVP compatibility pass, not a broad redesign.

## Task List

### Phase 1: Package Foundations

- [x] Task 1: Inventory public package surface and assets.
- [x] Task 2: Add shared package metadata and packability defaults.
- [x] Task 3: Add per-package metadata and package asset rules.

### Checkpoint: Pack Foundations

- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore`
- [x] Pack each packable project with `--configuration Release --no-build --output .\artifacts\packages`

### Phase 2: Package Verification

- [x] Task 4: Add package artifact inspection guidance or tests.
- [x] Task 5: Create a package-consumer smoke test path.
- [x] Task 6: Review and tighten public API surface.

### Checkpoint: Consumer Smoke

- [x] Package artifacts exist only for packable projects.
- [x] A temporary consumer can restore from local packages.
- [x] Consumer app can register `AddAuthNet`, call `MapAuthNet`, and compile.

### Phase 3: Documentation and Closeout

- [x] Task 7: Update user docs for package-based installation.
- [x] Task 8: Update developer docs and compact context.
- [x] Task 9: Final verification and local review.
- [x] Task 10: Commit Slice 03.

### Checkpoint: Complete

- [x] Restore, build, test, and pack pass.
- [x] Public docs explain package installation and project-reference development.
- [x] Slice 03 artifacts are complete and named with `slice-03`.
- [x] Working tree contains only intended Slice 03 changes before commit.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Razor UI package misses embedded views or static assets | High | Inspect `.nupkg` contents and run a consumer smoke test. |
| EF Core migrations are not usable from the packaged persistence assembly | High | Inspect package contents and compile a consumer using `AuthNetDbContext`. |
| Package dependencies become too broad through `AuthNet.AspNetCore` | Medium | Document package roles and review dependency graph before finalizing metadata. |
| Public API has accidental types exposed from implementation details | Medium | Generate/review public API surface and decide whether to hide or document types. |
| `dotnet pack AuthNet.slnx` packs sample or test projects | Medium | Set explicit `IsPackable` values and verify produced package list. |
| NuGet metadata choices are premature | Low | Use conservative placeholder metadata marked for owner confirmation before publishing. |

## Open Questions

- Confirm final NuGet package owner/authors before publication.
- Confirm repository URL and license expression before packages are published publicly.
- Decide whether Slice 03 should include a committed package-consumer smoke project or only a documented local smoke command.
- Decide whether XML documentation generation should be required now or deferred until API stabilization.

## Parallelization Opportunities

- Package metadata and documentation can be drafted in parallel after Task 1.
- Public API review can run in parallel with package-consumer smoke work after package assets are packable.
- Final verification and context-doc updates should stay sequential at the end.
