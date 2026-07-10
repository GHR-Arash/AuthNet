# Slice 25 Todo: Provider-Neutral EF Persistence Split

## Task 1: Add Provider-Neutral EF Project

**Description:** Add a new `AuthNet.Persistence.EntityFrameworkCore` project for shared EF Core Identity model types and package it as a first-class AuthNet artifact.

**Acceptance criteria:**
- [x] `src/AuthNet.Persistence.EntityFrameworkCore/AuthNet.Persistence.EntityFrameworkCore.csproj` exists.
- [x] Project targets `net10.0`.
- [x] Project references `AuthNet.Core`.
- [x] Project references `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
- [x] Project has package metadata consistent with existing AuthNet packages.
- [x] Project is included in `AuthNet.slnx`.

**Verification:**
- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `AuthNet.slnx`
- `src/AuthNet.Persistence.EntityFrameworkCore/AuthNet.Persistence.EntityFrameworkCore.csproj`

**Estimated scope:** Small

## Task 2: Move Shared EF Model Types

**Description:** Move shared AuthNet EF model types from `AuthNet.Persistence.Postgres` into `AuthNet.Persistence.EntityFrameworkCore`.

**Acceptance criteria:**
- [x] `AuthNetDbContext` lives in the provider-neutral project.
- [x] `AuthNetUser` lives in the provider-neutral project.
- [x] `AuthNetInvitation` lives in the provider-neutral project.
- [x] `AuthNetInvitationToken` lives in the provider-neutral project.
- [x] `AuthNetAuditEvent` lives in the provider-neutral project.
- [x] Moved types use a provider-neutral namespace.
- [x] PostgreSQL provider no longer owns shared model type source files.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Persistence.EntityFrameworkCore/*.cs`
- `src/AuthNet.Persistence.Postgres/*.cs`

**Estimated scope:** Medium

## Task 3: Update Project References and Package Manifest

**Description:** Rewire package/project dependencies so shared-model consumers depend on `AuthNet.Persistence.EntityFrameworkCore`, while PostgreSQL depends on that provider-neutral package.

**Acceptance criteria:**
- [x] `AuthNet.Persistence.Postgres` references `AuthNet.Persistence.EntityFrameworkCore`.
- [x] `AuthNet.AspNetCore` references `AuthNet.Persistence.EntityFrameworkCore`.
- [x] `AuthNet.Api` references `AuthNet.Persistence.EntityFrameworkCore`, not PostgreSQL for shared model types.
- [x] `AuthNet.UI.Razor` references `AuthNet.Persistence.EntityFrameworkCore`, not PostgreSQL for shared model types.
- [x] Sample host and tests reference the provider-neutral project when they need shared model types.
- [x] Package manifest expects `AuthNet.Persistence.EntityFrameworkCore.0.1.0.nupkg`.
- [x] Package pack order includes the provider-neutral package before provider/runtime packages that depend on it.

**Verification:**
- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] `.\scripts\verify-package-metadata.ps1`

**Dependencies:** Tasks 1, 2

**Files likely touched:**
- `src/AuthNet.Persistence.Postgres/AuthNet.Persistence.Postgres.csproj`
- `src/AuthNet.AspNetCore/AuthNet.AspNetCore.csproj`
- `src/AuthNet.Api/AuthNet.Api.csproj`
- `src/AuthNet.UI.Razor/AuthNet.UI.Razor.csproj`
- `samples/AuthNet.SampleHost/AuthNet.SampleHost.csproj`
- `tests/AuthNet.Tests/AuthNet.Tests.csproj`
- `scripts/package-manifest.ps1`
- `scripts/verify.ps1`

**Estimated scope:** Medium

## Task 4: Update Runtime Code Namespaces

**Description:** Update runtime source files to use provider-neutral EF model namespaces.

**Acceptance criteria:**
- [x] `AuthNet.AspNetCore` uses provider-neutral `AuthNetDbContext` and `AuthNetUser`.
- [x] `AuthNet.Api` uses provider-neutral `AuthNetDbContext`, `AuthNetUser`, invitation, and token types.
- [x] `AuthNet.UI.Razor` uses provider-neutral EF model types.
- [x] `rg "using AuthNet.Persistence.Postgres" src` returns only legitimate PostgreSQL provider/migration references.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Tasks 1-3

**Files likely touched:**
- `src/AuthNet.AspNetCore`
- `src/AuthNet.Api`
- `src/AuthNet.UI.Razor`

**Estimated scope:** Medium

## Task 5: Update Tests and Sample Namespaces

**Description:** Update tests and sample-host source files to use provider-neutral EF model namespaces where they resolve users, DbContext, invitations, or audit events.

**Acceptance criteria:**
- [x] Integration test host uses provider-neutral `AuthNetUser` and `AuthNetDbContext`.
- [x] Account, admin, audit, invitation, MFA, startup, and SPA tests compile with provider-neutral model types.
- [x] Sample-host persistence tests compile with provider-neutral `AuthNetDbContext`.
- [x] `rg "AuthNet.Persistence.Postgres" tests samples -g *.cs` returns only legitimate provider-specific setup references.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests`

**Dependencies:** Tasks 1-4

**Files likely touched:**
- `tests/AuthNet.Tests`
- `samples/AuthNet.SampleHost`

**Estimated scope:** Medium

## Task 6: Update PostgreSQL Migrations and Snapshot

**Description:** Keep existing PostgreSQL migrations compiled in the PostgreSQL provider package while referencing the moved provider-neutral DbContext/entity types.

**Acceptance criteria:**
- [x] PostgreSQL migration files compile after the model move.
- [x] `AuthNetDbContextModelSnapshot` references provider-neutral type names.
- [x] Existing table names remain unchanged.
- [x] No new migration is generated merely for the namespace move unless EF requires an intentional snapshot update.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests`

**Dependencies:** Tasks 1-5

**Files likely touched:**
- `src/AuthNet.Persistence.Postgres/Migrations/*.cs`

**Estimated scope:** Medium

## Task 7: Update Docs and Context

**Description:** Update current docs and compact memory to reflect the new provider-neutral EF package and the unchanged PostgreSQL provider role.

**Acceptance criteria:**
- [x] `docs/architecture-context.md` records the provider-neutral EF package.
- [x] `docs/next-iteration-context.md` records Slice 25 when complete.
- [x] `context.md` records the new package shape.
- [x] User/developer docs distinguish shared EF model package from PostgreSQL migration/provider package.
- [x] Pack command docs include the new package.

**Verification:**
- [x] Manual read-through confirms docs do not say shared EF model types live in `AuthNet.Persistence.Postgres`.

**Dependencies:** Tasks 1-6

**Files likely touched:**
- `README.md`
- `docs/users/getting-started.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** Medium

## Task 8: Update SQL Server Follow-Up Plan

**Description:** Update SQL Server future artifacts to start from the completed provider-neutral EF split instead of treating it as future prerequisite work.

**Acceptance criteria:**
- [x] `tasks/future-sqlserver-provider-plan.md` no longer lists the EF split as in-scope for SQL Server.
- [x] `tasks/future-sqlserver-provider-todo.md` starts with adding the SQL Server provider package.
- [x] `docs/slice-24/unified-database-provider-api.md` links to or reflects Slice 25 completion when finished.
- [x] SQL Server runtime implementation remains deferred.

**Verification:**
- [x] Manual review confirms future SQL Server plan does not duplicate Slice 25 tasks.

**Dependencies:** Task 7

**Files likely touched:**
- `tasks/future-sqlserver-provider-plan.md`
- `tasks/future-sqlserver-provider-todo.md`
- `docs/slice-24/unified-database-provider-api.md`

**Estimated scope:** Small

## Task 9: Run Focused and Full Verification

**Description:** Run focused provider-boundary tests, package checks, full local verification, final diff review, and commit.

**Acceptance criteria:**
- [x] Solution builds.
- [x] Focused database builder tests pass.
- [x] Focused startup tests pass.
- [x] Focused invitation and audit tests pass.
- [x] Package metadata verification passes.
- [x] Package-consumer verification passes.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 25 changes plus pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests`
- [x] `.\scripts\verify-package-metadata.ps1`
- [x] `.\scripts\verify-package-consumer.ps1`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1-8

**Files likely touched:**
- All active Slice 25 files

**Estimated scope:** Small

## Checkpoint: Project Boundary

- [x] Tasks 1-3 complete.
- [x] Solution restores and builds.
- [x] Package manifest includes provider-neutral EF package.

## Checkpoint: Behavior Preserved

- [x] Tasks 4-6 complete.
- [x] Runtime code uses provider-neutral EF model types.
- [x] PostgreSQL migrations still compile.
- [x] Focused persistence/startup/invitation/audit tests pass.

## Checkpoint: Docs Current

- [x] Tasks 7-8 complete.
- [x] Context and docs describe the new package shape.
- [x] SQL Server follow-up plan is narrowed to provider implementation.

## Checkpoint: Complete

- [x] Task 9 complete.
- [x] Full verification passes.
- [x] Slice 25 can be committed independently from unrelated working-tree changes.
