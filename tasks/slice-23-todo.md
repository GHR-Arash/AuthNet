# Slice 23 Todo: Fluent Startup Bootstrap API

## Task 1: Define Startup Options and Fluent Builder

**Description:** Add package-owned types that capture startup behavior requested by the host application without exposing Identity or EF Core implementation details.

**Acceptance criteria:**
- [x] `AuthNetStartupBuilder` exists in `AuthNet.AspNetCore`.
- [x] Builder supports `ApplyMigrations(bool enabled = true)`.
- [x] Builder supports `InitialAdministrator(string username, string password, string email)`.
- [x] Builder supports `InitialAdministrator(IConfiguration configurationSection)`.
- [x] Configuration binding accepts `UserName`, `Email`, and `Password`.

**Verification:**
- [x] Build succeeds: `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] Unit tests cover direct values and configuration-section binding.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetStartupBuilder.cs`
- `src/AuthNet.AspNetCore/AuthNetStartupOptions.cs`
- `tests/AuthNet.Tests/AuthNetStartupBuilderTests.cs`

**Estimated scope:** Medium

## Task 2: Add Fluent UseAuthNet Startup API

**Description:** Add a `UseAuthNet(...)` overload that lets app authors opt into startup tasks and endpoint mapping through one fluent AuthNet call.

**Acceptance criteria:**
- [x] `await app.UseAuthNet(authNet => ...)` compiles.
- [x] `UseAuthNet()` with no arguments preserves current endpoint mapping behavior.
- [x] `MapAuthNet()` remains available for endpoint-only mapping.
- [x] Configuration validation still runs before endpoint mapping.

**Verification:**
- [x] Existing endpoint mapping compatibility tests pass.
- [x] New integration test verifies `UseAuthNet(...)` maps `/auth` and `/auth/api/openapi.json`.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetApplicationBuilderExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetEndpointMappingTests.cs`

**Estimated scope:** Small

## Task 3: Implement Provider-Aware Migration Runner

**Description:** Add package-owned migration execution so consumers can opt into schema application without writing `CreateScope()` and `Database.Migrate()` code.

**Acceptance criteria:**
- [x] `ApplyMigrations()` calls `AuthNetDbContext.Database.MigrateAsync()` for relational providers.
- [x] `ApplyMigrations(false)` disables migration execution.
- [x] EF Core InMemory is detected and skipped without failure.
- [x] Migration failures surface clear startup errors.

**Verification:**
- [x] Unit or integration tests cover disabled migration behavior.
- [x] InMemory integration test verifies `ApplyMigrations()` does not throw.
- [x] Relational execution path is covered with a mockable seam or narrowly scoped test.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetDatabaseInitializer.cs`
- `src/AuthNet.AspNetCore/AuthNetApplicationBuilderExtensions.cs`
- `tests/AuthNet.Tests/AuthNetDatabaseInitializerTests.cs`

**Estimated scope:** Medium

## Task 4: Implement Initial Administrator Seeder

**Description:** Move first-admin bootstrap into the package with secure, idempotent behavior.

**Acceptance criteria:**
- [x] Creates the `Administrator` role if missing.
- [x] Creates a missing admin user with configured username, email, and password.
- [x] Confirms the initial admin email by default.
- [x] Assigns the user to the `Administrator` role.
- [x] If a user already exists by email, does not reset the password and only ensures role membership.
- [x] Exceptions never include raw password values.

**Verification:**
- [x] Tests cover create missing admin.
- [x] Tests cover promoting existing user.
- [x] Tests cover idempotent repeated execution.
- [x] Tests cover missing password when the user does not exist.
- [x] Tests cover invalid password policy failure.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetInitialAdministratorSeeder.cs`
- `tests/AuthNet.Tests/AuthNetInitialAdministratorSeederTests.cs`

**Estimated scope:** Medium

## Task 5: Wire Startup Task Execution

**Description:** Connect the builder, migration runner, and administrator seeder so configured startup tasks run in deterministic order before AuthNet endpoints are mapped.

**Acceptance criteria:**
- [x] Startup execution order is validate configuration, apply migrations, seed administrator, map endpoints.
- [x] Startup services use an internal service scope.
- [x] Startup task execution is async all the way through the public `UseAuthNet(...)` API.
- [x] Multiple configured startup tasks run once per `UseAuthNet(...)` call.

**Verification:**
- [x] Integration test verifies `UseAuthNet(...InitialAdministrator...)` creates a sign-in-capable admin.
- [x] Integration test verifies `UseAuthNet(...ApplyMigrations...)` works in InMemory sample/test mode.

**Dependencies:** Tasks 2, 3, 4

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetApplicationBuilderExtensions.cs`
- `src/AuthNet.AspNetCore/AuthNetStartupRunner.cs`
- `tests/AuthNet.Tests/Integration/AuthNetStartupTests.cs`

**Estimated scope:** Medium

## Task 6: Update Sample Host

**Description:** Replace sample-host-specific migration and admin bootstrap code with the new AuthNet startup API.

**Acceptance criteria:**
- [x] `samples/AuthNet.SampleHost/Program.cs` uses `await app.UseAuthNet(...)`.
- [x] Manual migration scope code is removed from `Program.cs`.
- [x] `SampleHostAdminBootstrap` is removed or reduced to obsolete compatibility only if still needed by tests.
- [x] Sample still creates the development demo admin in code.
- [x] Sample still supports configuration-driven initial admin.

**Verification:**
- [x] Sample host builds.
- [x] Focused sample tests pass.
- [x] Manual development startup reaches `/auth`.

**Dependencies:** Task 5

**Files likely touched:**
- `samples/AuthNet.SampleHost/Program.cs`
- `samples/AuthNet.SampleHost/SampleHostAdminBootstrap.cs`
- `samples/AuthNet.SampleHost/appsettings*.json`
- `tests/AuthNet.Tests/SampleHostAdminBootstrapTests.cs`
- `tests/AuthNet.Tests/SampleHostAuthNetPersistenceTests.cs`

**Estimated scope:** Medium

## Task 7: Update README and User Documentation

**Description:** Reframe startup setup around the fluent package API and remove Identity/EF boilerplate from the main quickstart path.

**Acceptance criteria:**
- [x] Root README shows `await app.UseAuthNet(authNet => ...)`.
- [x] Quickstart includes development mode with InMemory caveat.
- [x] Quickstart includes production mode with PostgreSQL, real email, explicit migrations, and secret-backed initial admin.
- [x] Docs explain code-based and appsettings-based initial admin setup.
- [x] Docs still warn that AuthNet does not create credentials unless explicitly configured.

**Verification:**
- [x] README snippets match compileable API names.
- [x] No primary quickstart path contains `UserManager`, `RoleManager`, or `Database.Migrate()` boilerplate.

**Dependencies:** Tasks 1-6

**Files likely touched:**
- `README.md`
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`

**Estimated scope:** Medium

## Task 8: Update Architecture and Iteration Context

**Description:** Keep project memory aligned with the new package-owned startup abstraction.

**Acceptance criteria:**
- [x] `docs/architecture-context.md` says initial admin bootstrap and migration opt-in are package-owned fluent startup features.
- [x] `docs/next-iteration-context.md` records Slice 23 implementation and verification status.
- [x] `context.md` reflects the current public integration shape.
- [x] Obsolete sample-host-only admin bootstrap notes are removed or rewritten.

**Verification:**
- [x] Context docs are concise and do not duplicate the full README.
- [x] Documentation references the same canonical commands and API names.

**Dependencies:** Tasks 6, 7

**Files likely touched:**
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** Small

## Checkpoint: Public Contract

- [x] Tasks 1-2 complete.
- [x] Build succeeds.
- [x] Existing endpoint mapping tests pass.

## Checkpoint: Startup Behavior

- [x] Tasks 3-5 complete.
- [x] Admin bootstrap tests pass.
- [x] Migration opt-in tests pass.
- [x] Existing route/admin tests pass.

## Checkpoint: Consumer Readiness

- [x] Tasks 6-8 complete.
- [x] Sample host starts in Development.
- [x] Package-consumer sample compiles.
- [x] `.\scripts\verify.ps1` passes when practical.
