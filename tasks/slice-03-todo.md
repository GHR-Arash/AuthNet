# Slice 03 Todo: Package Readiness

## Task 1: Inventory public package surface and assets

**Description:** Inspect the current projects, public types, project references, Razor content, migrations, and package dependencies so package decisions are grounded in the actual codebase.

**Acceptance criteria:**
- [x] Packable and non-packable projects are listed.
- [x] Public API surface is captured for review.
- [x] Razor UI and EF migration package assets are identified.

**Verification:**
- [x] Manual review notes added to `docs/slice-03/package-readiness.md`.

**Dependencies:** None

**Files likely touched:**
- `docs/slice-03/package-readiness.md`

**Estimated scope:** S

## Task 2: Add shared package metadata and packability defaults

**Description:** Add centralized MSBuild metadata for package ID prefix, authors, repository metadata, license placeholder, common tags, and default packability behavior.

**Acceptance criteria:**
- [x] Shared package metadata is defined once where practical.
- [x] Sample and test projects are explicitly non-packable.
- [x] Packable library projects retain project-reference development behavior.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore`

**Dependencies:** Task 1

**Files likely touched:**
- `Directory.Build.props`
- `samples/AuthNet.SampleHost/AuthNet.SampleHost.csproj`
- `tests/AuthNet.Tests/AuthNet.Tests.csproj`

**Estimated scope:** S

## Task 3: Add per-package metadata and package asset rules

**Description:** Configure each packable project with package descriptions and any asset rules needed for Razor UI and EF Core migration consumption.

**Acceptance criteria:**
- [x] Each packable project has a clear package description.
- [x] Razor UI package includes required compiled Razor assets.
- [x] Persistence package includes migrations and design-time metadata needed by EF tooling.
- [x] Internal build-only dependencies do not leak unnecessarily.

**Verification:**
- [x] Pack each packable project with `--configuration Release --no-build --output .\artifacts\packages`
- [x] Produced package list contains only intended packages.

**Dependencies:** Task 2

**Files likely touched:**
- `src/AuthNet.Core/AuthNet.Core.csproj`
- `src/AuthNet.AspNetCore/AuthNet.AspNetCore.csproj`
- `src/AuthNet.UI.Razor/AuthNet.UI.Razor.csproj`
- `src/AuthNet.Persistence.Postgres/AuthNet.Persistence.Postgres.csproj`
- `src/AuthNet.ExternalProviders/AuthNet.ExternalProviders.csproj`

**Estimated scope:** M

## Task 4: Add package artifact inspection guidance or tests

**Description:** Add a repeatable way to inspect generated `.nupkg` contents for expected assemblies, Razor assets, migrations, and dependency metadata.

**Acceptance criteria:**
- [x] Package inspection steps are documented or scripted.
- [x] Expected package contents are listed for each package.
- [x] The inspection path avoids checking generated `.nupkg` files into git.

**Verification:**
- [x] Package contents are inspected after `dotnet pack`.

**Dependencies:** Task 3

**Files likely touched:**
- `docs/slice-03/package-readiness.md`
- Possibly `docs/developer/quick-start.md`

**Estimated scope:** S

## Task 5: Create a package-consumer smoke test path

**Description:** Verify that a clean consumer can reference locally packed AuthNet packages and compile a minimal app using the documented setup.

**Acceptance criteria:**
- [x] A local package source workflow is defined.
- [x] Consumer smoke flow verifies `AddAuthNet` and `MapAuthNet` compile from packages.
- [x] The smoke path does not require publishing to NuGet.

**Verification:**
- [x] Local package-consumer smoke command or manual checklist succeeds.

**Dependencies:** Task 3

**Files likely touched:**
- `docs/slice-03/package-consumption-smoke.md`
- Possibly `samples/` if a committed smoke project is chosen

**Estimated scope:** M

## Task 6: Review and tighten public API surface

**Description:** Review public types and members exposed by the packable projects, then either document intended public APIs or reduce accidental exposure where safe.

**Acceptance criteria:**
- [x] Public API surface is reviewed package by package.
- [x] Accidental public APIs are made internal only when behavior remains unchanged.
- [x] Intended integration APIs are documented.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test AuthNet.slnx --no-build`

**Dependencies:** Tasks 1 and 3

**Files likely touched:**
- `src/AuthNet.Core/**/*.cs`
- `src/AuthNet.AspNetCore/**/*.cs`
- `src/AuthNet.Persistence.Postgres/**/*.cs`
- `src/AuthNet.UI.Razor/**/*.cs`
- `src/AuthNet.ExternalProviders/**/*.cs`
- `docs/slice-03/package-readiness.md`

**Estimated scope:** M

## Task 7: Update user docs for package-based installation

**Description:** Update consumer documentation to show package references, package roles, and the difference between consuming packages and developing from project references.

**Acceptance criteria:**
- [x] User guide lists the package install path.
- [x] Configuration docs remain accurate for package consumers.
- [x] Deferred package `AuthNet.Api` is not presented as available.

**Verification:**
- [x] Manual doc review confirms install docs match package names and setup code.

**Dependencies:** Tasks 3 and 5

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `README.md`

**Estimated scope:** S

## Task 8: Update developer docs and compact context

**Description:** Update developer-facing commands and compact project memory with Slice 03 package verification commands and artifact locations.

**Acceptance criteria:**
- [x] Developer quick start includes pack verification.
- [x] Architecture context reflects finalized package shape.
- [x] Next iteration context records Slice 03 status when completed.

**Verification:**
- [x] Manual read-through confirms docs are compact and synchronized.

**Dependencies:** Tasks 3 through 7

**Files likely touched:**
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`

**Estimated scope:** S

## Task 9: Final verification and local review

**Description:** Run the full verification path, inspect package output, and review the diff for packaging, docs, and API compatibility issues.

**Acceptance criteria:**
- [x] Restore passes.
- [x] Build passes.
- [x] Tests pass.
- [x] Pack passes.
- [x] Diff review finds no unintended package or documentation drift.

**Verification:**
- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore`
- [x] `.\.dotnet\dotnet.exe test AuthNet.slnx --no-build`
- [x] Pack each packable project with `--configuration Release --no-build --output .\artifacts\packages`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 8

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 10: Commit Slice 03

**Description:** Commit the completed package readiness slice with implementation, verification, and documentation updates.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 03 changes.
- [x] Commit message describes package readiness.
- [x] Working tree is clean after commit except for pre-existing/user-owned `AGENTS.md` edits.

**Verification:**
- [x] `git status --short`
- [x] `git log --oneline -1`

**Dependencies:** Task 9

**Files likely touched:** Git metadata only

**Estimated scope:** XS
