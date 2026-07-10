# Slice 24 Todo: Unified Database Provider API

## Task 1: Add Database Builder Contract

**Description:** Add the public database builder type and internal state needed to represent the selected AuthNet persistence provider without immediately registering EF Core services.

**Acceptance criteria:**
- [x] `AuthNetDatabaseBuilder` exists in `AuthNet.AspNetCore`.
- [x] Builder supports `UsePostgres(string? connectionString)`.
- [x] Builder supports `UseInMemory(string databaseName)`.
- [x] Builder supports a raw `ConfigureDbContext(Action<DbContextOptionsBuilder>)` escape hatch if needed for compatibility.
- [x] Builder prevents or clearly represents multiple provider selections.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] Unit tests cover builder state for PostgreSQL, InMemory, custom DbContext callback, and duplicate provider selection.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetDatabaseBuilder.cs`
- `tests/AuthNet.Tests/AuthNetDatabaseBuilderTests.cs`

**Estimated scope:** Medium

## Task 2: Wire AddAuthNet Overloads to Database Builder

**Description:** Add service-registration overloads that use `AuthNetDatabaseBuilder` and route AuthNet EF store registration through the selected provider.

**Acceptance criteria:**
- [x] `AddAuthNet(options => ..., db => db.UsePostgres(...))` compiles.
- [x] `AddAuthNet(options => ..., db => db.UseInMemory(...))` compiles.
- [x] Existing options-only `AddAuthNet(...)` remains available.
- [x] Existing legacy `PostgresConnectionString` path remains functional.
- [x] Missing database configuration throws `AuthNetConfigurationException` with guidance for the new database builder API.
- [x] Missing PostgreSQL connection string throws `AuthNetConfigurationException`.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] Existing route/startup tests still compile and pass once test host call sites are updated.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `src/AuthNet.Core/AuthNetOptions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** Medium

## Task 3: Add Focused Persistence Configuration Tests

**Description:** Add tests that lock down the new database registration behavior before updating samples and docs.

**Acceptance criteria:**
- [x] Test verifies PostgreSQL registration uses the new `db.UsePostgres(...)` API.
- [x] Test verifies InMemory registration uses the new `db.UseInMemory(...)` API.
- [x] Test verifies missing database configuration fails with the expected message.
- [x] Test verifies the legacy `PostgresConnectionString` path still works.
- [x] Test verifies duplicate provider behavior according to the selected rule.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostAuthNetPersistenceTests`

**Dependencies:** Tasks 1, 2

**Files likely touched:**
- `tests/AuthNet.Tests/AuthNetDatabaseBuilderTests.cs`
- `tests/AuthNet.Tests/SampleHostAuthNetPersistenceTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** Medium

## Task 4: Update Sample Host Persistence Registration

**Description:** Replace sample-host persistence wiring with the new database builder API while preserving the Development-only InMemory guard.

**Acceptance criteria:**
- [x] Development InMemory path calls `db.UseInMemory(...)`.
- [x] PostgreSQL path calls `db.UsePostgres(...)`.
- [x] `AuthNet:UseInMemoryDatabase=true` remains rejected outside Development.
- [x] Sample host startup behavior remains unchanged.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostAuthNetPersistenceTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests`

**Dependencies:** Tasks 1-3

**Files likely touched:**
- `samples/AuthNet.SampleHost/SampleHostAuthNetPersistence.cs`
- `samples/AuthNet.SampleHost/Program.cs`
- `tests/AuthNet.Tests/SampleHostAuthNetPersistenceTests.cs`

**Estimated scope:** Small

## Task 5: Update Package Consumer Sample

**Description:** Update the committed package-consumer sample to demonstrate `db.UsePostgres(...)` as the primary package-consumer database setup.

**Acceptance criteria:**
- [x] `samples/AuthNet.PackageConsumer/Program.cs` uses `AddAuthNet(..., db => db.UsePostgres(...))`.
- [x] Sample README mentions the unified database builder.
- [x] Package-consumer verification still restores and builds against local artifacts.

**Verification:**
- [x] `.\scripts\verify-package-consumer.ps1`

**Dependencies:** Tasks 1-3

**Files likely touched:**
- `samples/AuthNet.PackageConsumer/Program.cs`
- `samples/AuthNet.PackageConsumer/README.md`

**Estimated scope:** Small

## Task 6: Update README and User Documentation

**Description:** Reframe consumer-facing setup around the unified database builder and stop teaching `PostgresConnectionString` as the primary path.

**Acceptance criteria:**
- [x] Root README shows `db.UsePostgres(...)`.
- [x] Root README development InMemory section shows `db.UseInMemory(...)`.
- [x] User getting-started docs show PostgreSQL through the database builder.
- [x] User configuration docs mark `PostgresConnectionString` as legacy or compatibility-only if it remains.
- [x] Developer quick start uses the new sample-host and package-consumer shapes.
- [x] SQL Server is described as planned modular provider work, not currently shipped runtime support.

**Verification:**
- [x] Manual read-through confirms no primary quickstart path still uses `options.PostgresConnectionString`.
- [x] `rg -n "PostgresConnectionString|UseInMemoryDatabase|UsePostgres|UseInMemory" README.md docs\\users docs\\developer samples`

**Dependencies:** Tasks 1-5

**Files likely touched:**
- `README.md`
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`

**Estimated scope:** Medium

## Task 7: Update Architecture and Iteration Context

**Description:** Update compact project memory to record the unified database provider API and keep SQL Server as a planned provider package.

**Acceptance criteria:**
- [x] `docs/architecture-context.md` records `db.UsePostgres(...)` and `db.UseInMemory(...)`.
- [x] `docs/next-iteration-context.md` records Slice 24 implementation and verification status when complete.
- [x] `context.md` records the current persistence integration shape.
- [x] Deferred SQL Server support is described as modular provider work.
- [x] Obsolete guidance that PostgreSQL is configured only through `PostgresConnectionString` is removed.

**Verification:**
- [x] Context docs are concise and match the implemented API names.

**Dependencies:** Tasks 4-6

**Files likely touched:**
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** Small

## Task 8: Document Provider-Neutral EF Package Split

**Description:** Add a focused design note for splitting shared EF persistence types out of `AuthNet.Persistence.Postgres` before implementing SQL Server.

**Acceptance criteria:**
- [x] Design note proposes `AuthNet.Persistence.EntityFrameworkCore` or an equivalent provider-neutral package.
- [x] Design note lists which types move into the provider-neutral package.
- [x] Design note states PostgreSQL and SQL Server packages own provider dependencies, extension methods, and migrations.
- [x] Design note calls out compatibility risks around namespaces, package dependencies, and migration commands.

**Verification:**
- [x] Manual review confirms SQL Server is not planned as a dependency inside `AuthNet.Persistence.Postgres`.

**Dependencies:** None

**Files likely touched:**
- `docs/slice-24/unified-database-provider-api.md`

**Estimated scope:** Small

## Task 9: Add SQL Server Provider Follow-Up Plan

**Description:** Create a future plan and task list for SQL Server after the provider-neutral EF boundary is agreed.

**Acceptance criteria:**
- [x] Future SQL Server plan includes `AuthNet.Persistence.SqlServer`.
- [x] Future SQL Server plan includes `db.UseSqlServer(connectionString)`.
- [x] Future SQL Server plan includes SQL Server migrations and package metadata.
- [x] Future SQL Server tasks include tests, docs, package verification, and sample guidance.
- [x] Plan explicitly excludes JWT, cross-origin APIs, and custom Identity stores.

**Verification:**
- [x] Manual review confirms the future SQL Server work can be promoted to a slice without re-discovering scope.

**Dependencies:** Task 8

**Files likely touched:**
- `tasks/future-sqlserver-provider-plan.md`
- `tasks/future-sqlserver-provider-todo.md`

**Estimated scope:** Small

## Task 10: Run Focused and Full Verification

**Description:** Run the focused persistence/startup/package checks, full local verification, final diff review, and prepare the slice for commit.

**Acceptance criteria:**
- [x] Focused database builder tests pass.
- [x] Sample-host persistence tests pass.
- [x] Startup tests pass.
- [x] Package metadata verification passes.
- [x] Package-consumer verification passes.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 24 changes plus pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostAuthNetPersistenceTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests`
- [x] `.\scripts\verify-package-metadata.ps1`
- [x] `.\scripts\verify-package-consumer.ps1`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1-9

**Files likely touched:**
- All active Slice 24 files

**Estimated scope:** Small

## Checkpoint: Registration Contract

- [x] Tasks 1-3 complete.
- [x] Build succeeds.
- [x] Focused database configuration tests pass.
- [x] Existing legacy PostgreSQL configuration still works.

## Checkpoint: Consumer Experience

- [x] Tasks 4-7 complete.
- [x] Sample host uses the database builder.
- [x] Package-consumer sample uses the database builder.
- [x] README and primary user docs use `db.UsePostgres(...)` and `db.UseInMemory(...)`.

## Checkpoint: SQL Server Readiness

- [x] Tasks 8-9 complete.
- [x] Provider-neutral persistence split is documented.
- [x] Future SQL Server provider scope is taskified.

## Checkpoint: Complete

- [x] Task 10 complete.
- [x] Full verification passes when practical.
- [x] Slice 24 can be committed independently from unrelated working-tree changes.
