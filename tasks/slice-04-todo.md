# Slice 04 Todo: Development InMemory Persistence

## Task 1: Add sample-host InMemory dependency and development configuration

**Description:** Add EF Core InMemory to the sample host only and introduce an explicit development setting, likely `AuthNet:UseInMemoryDatabase`, in `appsettings.Development.json`.

**Acceptance criteria:**
- [x] `samples/AuthNet.SampleHost` references `Microsoft.EntityFrameworkCore.InMemory`.
- [x] Development configuration enables InMemory explicitly.
- [x] Base `appsettings.json` keeps PostgreSQL configuration as the default path.

**Verification:**
- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `samples/AuthNet.SampleHost/AuthNet.SampleHost.csproj`
- `samples/AuthNet.SampleHost/appsettings.Development.json`
- `samples/AuthNet.SampleHost/appsettings.json`

**Estimated scope:** S

## Task 2: Wire development-only InMemory DbContext selection

**Description:** Update the sample host startup to call `AddAuthNet` with `db.UseInMemoryDatabase(...)` only when the environment is Development and the explicit setting is enabled.

**Acceptance criteria:**
- [x] Development InMemory mode does not require `PostgresConnectionString`.
- [x] PostgreSQL remains the path when InMemory mode is disabled.
- [x] The implementation uses the existing `configureDbContext` seam.

**Verification:**
- [x] Sample host starts in Development with InMemory enabled and no live PostgreSQL dependency.

**Dependencies:** Task 1

**Files likely touched:**
- `samples/AuthNet.SampleHost/Program.cs`

**Estimated scope:** S

## Task 3: Add production and migration guardrails

**Description:** Ensure InMemory mode fails outside Development and startup migrations are skipped when InMemory is active.

**Acceptance criteria:**
- [x] `UseInMemoryDatabase=true` outside Development fails fast with a clear message.
- [x] `AuthNet:ApplyMigrations=true` does not call `Database.Migrate()` when InMemory is active.
- [x] PostgreSQL migration behavior is unchanged when InMemory is disabled.

**Verification:**
- [x] Focused tests and manual startup checks cover guardrails.

**Dependencies:** Task 2

**Files likely touched:**
- `samples/AuthNet.SampleHost/Program.cs`
- `tests/AuthNet.Tests/Integration/*`

**Estimated scope:** M

## Task 4: Cover development InMemory startup behavior

**Description:** Add tests that build or run a sample-host-equivalent configuration in Development with InMemory enabled and prove account routes can resolve without PostgreSQL.

**Acceptance criteria:**
- [x] Test host uses InMemory through the same startup branch or an extracted helper.
- [x] Login route or a minimal AuthNet route succeeds without PostgreSQL.
- [x] Existing integration tests still use isolated InMemory databases.

**Verification:**
- [x] Focused integration tests pass.

**Dependencies:** Tasks 2 and 3

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/*`
- Possibly `samples/AuthNet.SampleHost/Program.cs` if startup helpers are extracted

**Estimated scope:** M

## Task 5: Cover production rejection and migration skip rules

**Description:** Add tests for the safety rules around InMemory mode and migration application.

**Acceptance criteria:**
- [x] Production with InMemory enabled is rejected.
- [x] InMemory plus `ApplyMigrations=true` does not attempt relational migrations.
- [x] PostgreSQL mode still allows migration application when configured.

**Verification:**
- [x] Focused tests pass.

**Dependencies:** Task 3

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/*`
- `tests/AuthNet.Tests/*`

**Estimated scope:** M

## Task 6: Update developer and user docs

**Description:** Document how development InMemory mode works, when to use it, and when PostgreSQL is still required.

**Acceptance criteria:**
- [x] Developer quick start says the sample host can run in Development without PostgreSQL.
- [x] User docs still present PostgreSQL as the production/default persistence path.
- [x] Docs warn that EF InMemory is not production-like PostgreSQL testing.

**Verification:**
- [x] Manual doc review confirms development and production guidance are not conflated.

**Dependencies:** Tasks 1 through 5

**Files likely touched:**
- `docs/developer/quick-start.md`
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/slice-04/development-inmemory.md`

**Estimated scope:** S

## Task 7: Update architecture, next-iteration context, and `context.md`

**Description:** Record the persistence strategy change compactly in the project memory docs.

**Acceptance criteria:**
- [x] Architecture docs distinguish PostgreSQL default from sample-host development InMemory.
- [x] Next iteration context records Slice 04 status and verification when complete.
- [x] `context.md` reflects the current persistence modes.

**Verification:**
- [x] Manual read-through confirms context docs are compact and synchronized.

**Dependencies:** Task 6

**Files likely touched:**
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 8: Final verification and local review

**Description:** Run the full verification path and review for persistence-scope drift.

**Acceptance criteria:**
- [x] Restore passes.
- [x] Build passes.
- [x] Tests pass.
- [x] Diff does not add InMemory dependency to packable AuthNet packages.
- [x] Docs clearly keep PostgreSQL as production/default persistence.

**Verification:**
- [x] `.\.dotnet\dotnet.exe restore AuthNet.slnx`
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] `.\.dotnet\dotnet.exe test AuthNet.slnx --no-build`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 7

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 9: Commit Slice 04

**Description:** Commit the completed development InMemory slice.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 04 changes.
- [x] Commit message describes development InMemory support.
- [x] Working tree is clean except unrelated user-owned changes.

**Verification:**
- [x] `git status --short`
- [x] `git log --oneline -1`

**Dependencies:** Task 8

**Files likely touched:** Git metadata only

**Estimated scope:** XS
