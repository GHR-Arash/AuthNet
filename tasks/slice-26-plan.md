# Implementation Plan: SQL Server Provider

## Overview

Slice 26 adds first-party SQL Server persistence support to the unified AuthNet database provider API. The target consumer API is:

```csharp
builder.Services.AddAuthNet(
    options => builder.Configuration.GetSection("AuthNet").Bind(options),
    db => db.UseSqlServer(builder.Configuration.GetConnectionString("AuthNet")));
```

This slice builds on Slice 25: shared EF Core Identity model types already live in `AuthNet.Persistence.EntityFrameworkCore`, while provider-specific dependencies and migrations live in provider packages. PostgreSQL remains supported through `db.UsePostgres(connectionString)`. SQL Server becomes a second relational provider through a new `AuthNet.Persistence.SqlServer` package.

## Scope

In scope:

- Add `AuthNet.Persistence.SqlServer`.
- Add `db.UseSqlServer(connectionString)` to the unified database builder API.
- Add SQL Server EF Core provider dependency and provider-specific migration assembly marker.
- Add SQL Server migrations for the existing provider-neutral AuthNet EF model.
- Pin SQL Server migrations to the SQL Server provider assembly.
- Remove the old direct `AddAuthNet(..., Action<DbContextOptionsBuilder>)` overload from the public overload set because it makes `db.UseSqlServer(...)` ambiguous with EF Core's `DbContextOptionsBuilder.UseSqlServer(...)`. Custom provider experiments remain available through the single database API via `db.ConfigureDbContext(...)`.
- Add focused tests for SQL Server provider registration, validation, duplicate provider rejection, and migration assembly selection.
- Add package manifest and package metadata verification support.
- Update user/developer docs, compact context, and future SQL Server artifacts.

Out of scope:

- Removing `AuthNetOptions.PostgresConnectionString`; this is Slice 27.
- Moving `db.UsePostgres(...)` or `db.UseSqlServer(...)` out of `AuthNet.AspNetCore` into provider-package extension methods.
- Runtime database provider switching without explicit `db.UseX(...)`.
- SQL Server-specific sample host mode.
- SQL Server integration tests that require a live external database.
- JWT, refresh tokens, custom Identity stores, or multi-tenancy.

## Architecture Decisions

- `AuthNet.Persistence.SqlServer` mirrors the PostgreSQL provider package shape:
  - references `AuthNet.Persistence.EntityFrameworkCore`;
  - owns `Microsoft.EntityFrameworkCore.SqlServer`;
  - owns SQL Server migrations and model snapshot;
  - exposes a small public marker type used by `AuthNet.AspNetCore` to pin migrations.
- `AuthNet.AspNetCore` remains the public database-provider API owner for Slice 26, matching the current `UsePostgres` and `UseInMemory` design.
- SQL Server configuration should use a shared helper similar to the current PostgreSQL helper:
  - validate non-empty connection strings in `AuthNetDatabaseBuilder.UseSqlServer`;
  - call `UseSqlServer(connectionString, sql => sql.MigrationsAssembly(...))`;
  - preserve one-provider-only behavior through the existing `SetProvider(...)` path.
- SQL Server migrations should represent the current provider-neutral EF model, not a schema redesign.
- Documentation should show PostgreSQL and SQL Server as peer production providers, with InMemory still framed as development/test only.

## Public/Package Impact

New package:

- `AuthNet.Persistence.SqlServer`

Existing package changes:

- `AuthNet.AspNetCore` references `AuthNet.Persistence.SqlServer` so it can implement `db.UseSqlServer(...)` and pin SQL Server migrations.
- The old direct `AddAuthNet(..., Action<DbContextOptionsBuilder>)` overload is removed; callers should use `AddAuthNet(..., db => db.ConfigureDbContext(...))` so database selection has one public API shape.
- `scripts/package-manifest.ps1` includes `AuthNet.Persistence.SqlServer` after `AuthNet.Persistence.EntityFrameworkCore` and before packages that depend on provider registration.
- User docs show install/configuration examples for both PostgreSQL and SQL Server.

Expected package artifacts after Slice 26:

```text
AuthNet.Core.0.1.0.nupkg
AuthNet.ExternalProviders.0.1.0.nupkg
AuthNet.Persistence.EntityFrameworkCore.0.1.0.nupkg
AuthNet.Persistence.Postgres.0.1.0.nupkg
AuthNet.Persistence.SqlServer.0.1.0.nupkg
AuthNet.UI.Razor.0.1.0.nupkg
AuthNet.Api.0.1.0.nupkg
AuthNet.AspNetCore.0.1.0.nupkg
```

## Task List

### Phase 1: Provider Package Foundation

- [ ] Task 1: Add `AuthNet.Persistence.SqlServer` project.
- [ ] Task 2: Add SQL Server provider marker and package references.
- [ ] Task 3: Wire solution, package manifest, and package metadata.

### Checkpoint: Provider Package Builds

- [ ] Solution restores.
- [ ] Solution builds.
- [ ] Package manifest expects the SQL Server package.

### Phase 2: Unified Database API

- [ ] Task 4: Add SQL Server DbContext options helper.
- [ ] Task 5: Add `AuthNetDatabaseBuilder.UseSqlServer(...)`.
- [ ] Task 6: Add focused database builder tests.

### Checkpoint: API Behavior Verified

- [ ] `db.UseSqlServer(connectionString)` registers the SQL Server provider.
- [ ] Empty SQL Server connection string fails fast.
- [ ] SQL Server participates in duplicate provider rejection.
- [ ] SQL Server migrations assembly is pinned to `AuthNet.Persistence.SqlServer`.

### Phase 3: SQL Server Migrations

- [ ] Task 7: Generate initial SQL Server migrations.
- [ ] Task 8: Review generated schema for expected Identity, invitation, and audit tables.
- [ ] Task 9: Add migration command docs.

### Checkpoint: Provider Migrations Isolated

- [ ] SQL Server migrations compile in `AuthNet.Persistence.SqlServer`.
- [ ] PostgreSQL migrations remain unchanged.
- [ ] Provider-neutral EF model remains unchanged unless a real model issue is discovered.

### Phase 4: Docs, Context, and Verification

- [ ] Task 10: Update user/developer docs and compact context.
- [ ] Task 11: Update future provider artifacts and mark compatibility removal as Slice 27.
- [ ] Task 12: Run focused and full verification, review, and commit.

### Checkpoint: Complete

- [ ] Full verification passes.
- [ ] Package output includes SQL Server provider package.
- [ ] Docs show both production providers.
- [ ] Slice 27 removal of `AuthNetOptions.PostgresConnectionString` is explicitly queued.

## Slice 27 Follow-Up

After Slice 26, remove the compatibility path:

- Delete `AuthNetOptions.PostgresConnectionString`.
- Delete fallback registration that reads `options.PostgresConnectionString`.
- Require every host to specify the database through `AddAuthNet(..., db => db.UsePostgres(...))`, `db.UseSqlServer(...)`, `db.UseInMemory(...)`, or `db.ConfigureDbContext(...)`.
- Remove obsolete tests and docs that mention the legacy option.
- Update error messages so missing database configuration points only to the database builder API.

This is intentionally not part of Slice 26 so SQL Server support can be reviewed independently from the breaking API cleanup.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| SQL Server migrations accidentally alter provider-neutral model expectations. | High | Generate provider-specific migrations only; review snapshot and schema output before commit. |
| `AuthNet.AspNetCore` grows direct references to multiple providers. | Medium | Accept for Slice 26 to keep API simple; defer provider-package extension-method split until the public API stabilizes. |
| SQL Server package is missed by package verification. | Medium | Update `scripts/package-manifest.ps1` and run full `.\scripts\verify.ps1`. |
| Migration assembly defaults to provider-neutral EF package. | High | Add a marker type and focused test for SQL Server migrations assembly, mirroring the PostgreSQL fix. |
| Live SQL Server tests are unavailable locally/CI. | Medium | Use provider option tests and migration compile/package verification; leave live DB smoke tests for a future environment-backed slice. |

## Open Questions

- Should SQL Server docs include a Docker/localdb quickstart now, or only provider configuration and EF migration commands?
- Should provider extension methods eventually move into each provider package to remove direct provider dependencies from `AuthNet.AspNetCore`?
- Do we want a sample host config switch for SQL Server later, or should the sample stay PostgreSQL/InMemory only?

## Verification Commands

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests
.\scripts\verify-package-metadata.ps1
.\scripts\verify-package-consumer.ps1
.\scripts\verify.ps1
git diff --check
git status --short
```
