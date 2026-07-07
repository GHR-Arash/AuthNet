# Slice 04 Plan: Development InMemory Persistence

## Overview

Slice 04 adds an explicit development-only InMemory persistence path for the sample host so contributors can run account flows without a local PostgreSQL server. PostgreSQL remains the default production/runtime persistence path for AuthNet packages. The implementation should use the existing `AddAuthNet(..., configureDbContext)` seam instead of broadening AuthNet's production persistence abstraction.

## Scope

In scope:

- Add a sample-host development setting to opt into EF Core InMemory.
- Wire the sample host to use InMemory only when the host environment is Development and the setting is enabled.
- Keep PostgreSQL as the default outside development and when InMemory is disabled.
- Prevent migration execution when InMemory is active.
- Add integration or configuration tests that prove development InMemory behavior and production guardrails.
- Update user/developer docs and compact context docs.

Out of scope:

- Making InMemory a supported production persistence provider.
- Adding a new `AuthNet.Persistence.InMemory` package.
- Replacing PostgreSQL migrations or Npgsql defaults.
- Changing API/JWT/SPA scope.

## Architecture Decisions

- Prefer sample-host wiring over a new package-level provider because InMemory is for local development convenience, not a supported deployment target.
- Use EF Core InMemory through `configureDbContext` so `AuthNet.AspNetCore` does not gain a direct dependency on `Microsoft.EntityFrameworkCore.InMemory`.
- Make the switch explicit, for example `AuthNet:UseInMemoryDatabase`, and set it in `appsettings.Development.json`.
- Add a startup guard so `UseInMemoryDatabase=true` outside Development fails fast.
- Skip `Database.Migrate()` when InMemory is selected because EF InMemory is not relational and migrations do not apply.

## Task List

### Phase 1: Sample-Host Persistence Toggle

- [x] Task 1: Add sample-host InMemory dependency and development configuration.
- [x] Task 2: Wire development-only InMemory DbContext selection.
- [x] Task 3: Add guardrails for production and migration behavior.

### Checkpoint: Sample Host

- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] Sample host starts in Development without PostgreSQL when InMemory is enabled.
- [x] Sample host still uses PostgreSQL when InMemory is disabled.

### Phase 2: Tests

- [x] Task 4: Cover development InMemory startup behavior.
- [x] Task 5: Cover production rejection and migration skip rules.

### Checkpoint: Tests

- [x] Focused tests pass.
- [x] Full test suite passes.

### Phase 3: Documentation and Closeout

- [x] Task 6: Update developer and user docs.
- [x] Task 7: Update architecture, next-iteration context, and `context.md`.
- [x] Task 8: Final verification and local review.
- [x] Task 9: Commit Slice 04.

### Checkpoint: Complete

- [x] Restore, build, and test pass.
- [x] Docs clearly distinguish development InMemory from production PostgreSQL.
- [x] Slice 04 artifacts are complete and named with `slice-04`.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| InMemory accidentally appears as a supported production provider | High | Keep it sample-host only and document PostgreSQL as the default package path. |
| InMemory hides relational PostgreSQL behavior differences | Medium | State that InMemory is only for smoke/local account flows; PostgreSQL remains required for production-like testing. |
| Startup migrations run against InMemory | Medium | Branch migration application around the selected persistence mode. |
| New package dependency leaks into packable AuthNet packages | Medium | Add `Microsoft.EntityFrameworkCore.InMemory` only to sample/test projects, not `src/` packages. |
| Configuration becomes ambiguous | Low | Use one explicit setting and fail fast outside Development. |

## Open Questions

- Should development InMemory be enabled by default in `appsettings.Development.json`, or should it require an explicit local edit?
- Should the sample seed a default test user when InMemory is enabled, or keep registration as the entry path?
- Should the sample host display/log the active persistence mode on startup?

## Recommended Default

Enable InMemory by default only in the sample host's `appsettings.Development.json`. This makes the sample easy to run for contributors while leaving `appsettings.json`, production, package consumers, and docs centered on PostgreSQL.
