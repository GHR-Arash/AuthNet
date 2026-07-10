# Slice 26 Todo: SQL Server Provider

## Task 1: Add SQL Server Provider Project

**Description:** Add `AuthNet.Persistence.SqlServer` as a new packable provider project.

**Acceptance criteria:**
- [x] `src/AuthNet.Persistence.SqlServer/AuthNet.Persistence.SqlServer.csproj` exists.
- [x] Project targets `net10.0`.
- [x] Project references `AuthNet.Persistence.EntityFrameworkCore`.
- [x] Project has package metadata consistent with existing AuthNet packages.
- [x] Project is included in `AuthNet.slnx`.

**Verification:**
- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `AuthNet.slnx`
- `src/AuthNet.Persistence.SqlServer/AuthNet.Persistence.SqlServer.csproj`

**Estimated scope:** Small

## Task 2: Add SQL Server Provider Dependencies and Marker

**Description:** Add the EF Core SQL Server provider dependency and a public marker type for migration assembly selection.

**Acceptance criteria:**
- [x] SQL Server project references `Microsoft.EntityFrameworkCore.SqlServer`.
- [x] SQL Server project has a public marker type, for example `AuthNetSqlServerMigrationsAssembly`.
- [x] No shared EF model types are duplicated in the SQL Server project.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Persistence.SqlServer/AuthNet.Persistence.SqlServer.csproj`
- `src/AuthNet.Persistence.SqlServer/AuthNetSqlServerMigrationsAssembly.cs`

**Estimated scope:** Small

## Task 3: Update Package Manifest and Package Docs

**Description:** Add `AuthNet.Persistence.SqlServer` to package verification and package documentation.

**Acceptance criteria:**
- [x] `scripts/package-manifest.ps1` includes `AuthNet.Persistence.SqlServer`.
- [x] SQL Server package is ordered after `AuthNet.Persistence.EntityFrameworkCore`.
- [x] Developer package docs list the SQL Server package.

**Verification:**
- [x] `.\scripts\verify-package-metadata.ps1` after package artifacts exist.
- [x] `.\scripts\verify.ps1` at final verification.

**Dependencies:** Tasks 1-2

**Files likely touched:**
- `scripts/package-manifest.ps1`
- `README.md`
- `docs/developer/quick-start.md`
- `docs/developer/onboarding.md`

**Estimated scope:** Small

## Task 4: Add SQL Server DbContext Options Helper

**Description:** Add an internal AspNetCore helper that configures SQL Server with the correct migrations assembly.

**Acceptance criteria:**
- [x] Helper calls `UseSqlServer(connectionString, sql => sql.MigrationsAssembly(...))`.
- [x] Migrations assembly points to `AuthNet.Persistence.SqlServer`.
- [x] Helper mirrors the current PostgreSQL helper style.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Tasks 1-2

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNet.AspNetCore.csproj`
- `src/AuthNet.AspNetCore/AuthNetSqlServerDbContextOptions.cs`

**Estimated scope:** Small

## Task 5: Add `db.UseSqlServer(...)`

**Description:** Add SQL Server to `AuthNetDatabaseBuilder` as a peer to `UsePostgres` and `UseInMemory`.

**Acceptance criteria:**
- [x] Host code can call `db.UseSqlServer(connectionString)`.
- [x] Empty or whitespace connection string throws `AuthNetConfigurationException`.
- [x] Duplicate provider configuration still fails through the existing one-provider-only guard.
- [x] Existing `UsePostgres`, `UseInMemory`, and `ConfigureDbContext` behavior is unchanged.
- [x] The old direct `AddAuthNet(..., Action<DbContextOptionsBuilder>)` overload is removed so `db.UseSqlServer(...)` resolves to the database builder API; custom providers remain available through `db.ConfigureDbContext(...)`.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`

**Dependencies:** Task 4

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetDatabaseBuilder.cs`
- `tests/AuthNet.Tests/AuthNetDatabaseBuilderTests.cs`

**Estimated scope:** Small

## Task 6: Add Focused SQL Server Builder Tests

**Description:** Cover SQL Server provider behavior without requiring a live SQL Server instance.

**Acceptance criteria:**
- [x] Test verifies `UseSqlServer` registers the SQL Server provider.
- [x] Test verifies `UseSqlServer` pins migrations to `AuthNet.Persistence.SqlServer`.
- [x] Test verifies empty SQL Server connection strings fail fast.
- [x] Test verifies SQL Server participates in duplicate provider rejection.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`

**Dependencies:** Tasks 4-5

**Files likely touched:**
- `tests/AuthNet.Tests/AuthNetDatabaseBuilderTests.cs`
- `tests/AuthNet.Tests/AuthNet.Tests.csproj`

**Estimated scope:** Small

## Task 7: Generate SQL Server Initial Migrations

**Description:** Generate SQL Server migrations for the existing provider-neutral AuthNet EF model.

**Acceptance criteria:**
- [x] SQL Server migration files exist under `src/AuthNet.Persistence.SqlServer/Migrations`.
- [x] SQL Server model snapshot exists in the SQL Server provider package.
- [x] Migration namespace is provider-specific.
- [x] Migration references `AuthNet.Persistence.EntityFrameworkCore` model types.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Tasks 1-6

**Files likely touched:**
- `src/AuthNet.Persistence.SqlServer/Migrations/*.cs`

**Estimated scope:** Medium

## Task 8: Review Migration Schema

**Description:** Review generated SQL Server migrations for expected AuthNet tables and provider-specific annotations.

**Acceptance criteria:**
- [x] Identity tables are present.
- [x] Invitation tables are present.
- [x] Audit event table is present.
- [x] SQL Server annotations/types are expected.
- [x] PostgreSQL migrations are not modified by SQL Server migration generation.

**Verification:**
- [x] Manual migration diff review.
- [x] `git diff -- src/AuthNet.Persistence.Postgres/Migrations` shows no unintended changes.

**Dependencies:** Task 7

**Files likely touched:**
- `src/AuthNet.Persistence.SqlServer/Migrations/*.cs`

**Estimated scope:** Small

## Task 9: Document SQL Server Migration Commands

**Description:** Add developer documentation for generating/applying SQL Server migrations.

**Acceptance criteria:**
- [x] Developer docs include SQL Server EF migration command.
- [x] User docs include SQL Server setup example with `db.UseSqlServer(...)`.
- [x] Docs make clear InMemory is development/test only.

**Verification:**
- [x] Manual read-through of changed docs.

**Dependencies:** Tasks 5-8

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`

**Estimated scope:** Medium

## Task 10: Update Compact Context

**Description:** Update compact project memory to record SQL Server support and the new package shape.

**Acceptance criteria:**
- [x] `context.md` records Slice 26 as implemented when complete.
- [x] `docs/next-iteration-context.md` records SQL Server package and API support.
- [x] `docs/architecture-context.md` lists SQL Server as a supported production provider.
- [x] Verification count/commands are current after final test run.

**Verification:**
- [x] Manual read-through confirms context files are compact and current.

**Dependencies:** Tasks 1-9

**Files likely touched:**
- `context.md`
- `docs/next-iteration-context.md`
- `docs/architecture-context.md`

**Estimated scope:** Small

## Task 11: Queue Slice 27 Compatibility Removal

**Description:** Make the post-Slice-26 breaking cleanup explicit without implementing it in Slice 26.

**Acceptance criteria:**
- [x] Slice 26 docs state that `AuthNetOptions.PostgresConnectionString` removal is Slice 27.
- [x] Future SQL Server artifacts no longer describe SQL Server as future after Slice 26 is complete.
- [x] A future Slice 27 note or task artifact exists for removing the legacy PostgreSQL option.

**Verification:**
- [x] Manual review confirms Slice 26 does not remove the compatibility option.

**Dependencies:** Task 10

**Files likely touched:**
- `tasks/slice-26-plan.md`
- `tasks/future-sqlserver-provider-plan.md`
- `tasks/future-sqlserver-provider-todo.md`
- Optional: `tasks/slice-27-plan.md`
- Optional: `tasks/slice-27-todo.md`

**Estimated scope:** Small

## Task 12: Run Verification and Commit

**Description:** Run focused SQL Server provider tests, package checks, full verification, final diff review, and commit Slice 26.

**Acceptance criteria:**
- [x] Solution restores.
- [x] Solution builds.
- [x] Focused database builder tests pass.
- [x] Focused startup tests pass.
- [x] Package metadata verification passes.
- [x] Package-consumer verification passes.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 26 changes plus unrelated pre-existing changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests`
- [x] `.\scripts\verify-package-metadata.ps1`
- [x] `.\scripts\verify-package-consumer.ps1`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1-11

**Files likely touched:**
- All active Slice 26 files

**Estimated scope:** Small

## Checkpoint: Provider Package

- [x] Tasks 1-3 complete.
- [x] Solution restores and builds.
- [x] Package manifest includes SQL Server package.

## Checkpoint: Database API

- [x] Tasks 4-6 complete.
- [x] `db.UseSqlServer(...)` works through the unified database builder.
- [x] SQL Server migrations assembly is explicitly pinned.
- [x] Focused database builder tests pass.

## Checkpoint: Migrations

- [x] Tasks 7-9 complete.
- [x] SQL Server migrations compile.
- [x] PostgreSQL migrations remain unchanged.
- [x] Docs include SQL Server setup and migration commands.

## Checkpoint: Complete

- [x] Tasks 10-12 complete.
- [x] Compact context is current.
- [x] Full verification passes.
- [x] Slice 27 compatibility removal is queued but not implemented.
