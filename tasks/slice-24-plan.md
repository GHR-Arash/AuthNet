# Implementation Plan: Unified Database Provider API

## Overview

Slice 24 replaces AuthNet's mixed persistence configuration surface with one package-owned database builder API. The immediate goal is for hosts to configure PostgreSQL and development InMemory persistence through the same `AddAuthNet(..., db => ...)` shape. The follow-on goal is to make SQL Server a clean provider package instead of coupling the UI/API/runtime packages to PostgreSQL-specific types.

Target PostgreSQL consumer shape:

```csharp
builder.Services.AddAuthNet(
    options =>
    {
        options.ApplicationName = "My App";
        options.EnablePublicRegistration = false;
    },
    db => db.UsePostgres(builder.Configuration.GetConnectionString("AuthNet")));
```

Target development/test shape:

```csharp
builder.Services.AddAuthNet(
    options => options.ApplicationName = "My App",
    db => db.UseInMemory("AuthNet.Dev"));
```

Planned SQL Server shape:

```csharp
builder.Services.AddAuthNet(
    options => options.ApplicationName = "My App",
    db => db.UseSqlServer(builder.Configuration.GetConnectionString("AuthNet")));
```

## Architecture Decisions

- Add `AuthNetDatabaseBuilder` as the single public persistence configuration surface used by `AddAuthNet`.
- Keep PostgreSQL as the default production provider for the current release, but move the preferred API from `AuthNetOptions.PostgresConnectionString` to `db.UsePostgres(connectionString)`.
- Keep EF Core InMemory available for tests and development smoke paths through `db.UseInMemory(databaseName)`, with docs continuing to reject it for production persistence.
- Preserve compatibility for existing consumers during this slice. `PostgresConnectionString` should keep working but become legacy guidance.
- Keep the raw `DbContextOptionsBuilder` escape hatch only if needed for tests or advanced consumers, but do not make it the primary documented path.
- Treat SQL Server as a modular provider follow-up. True SQL Server support requires separating provider-neutral EF model types from `AuthNet.Persistence.Postgres`.
- Prefer a future `AuthNet.Persistence.EntityFrameworkCore` package for shared EF entities, `AuthNetDbContext`, and model configuration, with provider packages owning migrations and provider-specific extension methods.

## Public API Contract

Add a database builder type in `AuthNet.AspNetCore`:

```csharp
public sealed class AuthNetDatabaseBuilder
{
    public AuthNetDatabaseBuilder UsePostgres(string? connectionString);

    public AuthNetDatabaseBuilder UseInMemory(string databaseName);

    public AuthNetDatabaseBuilder ConfigureDbContext(
        Action<DbContextOptionsBuilder> configureDbContext);
}
```

Add service-registration overloads:

```csharp
public static IServiceCollection AddAuthNet(
    this IServiceCollection services,
    Action<AuthNetOptions> configureOptions,
    Action<AuthNetDatabaseBuilder> configureDatabase);

public static IServiceCollection AddAuthNet(
    this IServiceCollection services,
    Action<AuthNetDatabaseBuilder> configureDatabase);
```

Expected behavior:

- `db.UsePostgres(connectionString)` configures the AuthNet EF store with Npgsql.
- `db.UseInMemory(databaseName)` configures the AuthNet EF store with EF Core InMemory.
- Missing database configuration throws `AuthNetConfigurationException` with guidance to call `db.UsePostgres(...)` or another supported provider.
- Missing PostgreSQL connection string throws `AuthNetConfigurationException` with guidance to pass a non-empty connection string.
- Existing `AddAuthNet(options => options.PostgresConnectionString = ...)` remains functional for compatibility.
- If both legacy `PostgresConnectionString` and a database builder provider are supplied, the explicit database builder provider wins or the configuration fails with a clear duplicate-provider error. The implementation should choose one rule and test it.

Future SQL Server extension shape:

```csharp
public static class AuthNetSqlServerDatabaseBuilderExtensions
{
    public static AuthNetDatabaseBuilder UseSqlServer(
        this AuthNetDatabaseBuilder builder,
        string? connectionString);
}
```

## Task List

### Phase 1: Unified Database Builder

- [ ] Task 1: Add database builder contract and internal provider state.
- [ ] Task 2: Wire `AddAuthNet` overloads to the database builder.
- [ ] Task 3: Add focused configuration tests for PostgreSQL, InMemory, missing configuration, and legacy compatibility.

### Checkpoint: Registration Contract

- [ ] The new `AddAuthNet(..., db => db.UsePostgres(...))` API compiles.
- [ ] The new `AddAuthNet(..., db => db.UseInMemory(...))` API compiles.
- [ ] Existing `PostgresConnectionString` registration still works.
- [ ] Focused configuration tests pass.

### Phase 2: Consumer Migration

- [ ] Task 4: Update sample host persistence registration.
- [ ] Task 5: Update package-consumer sample.
- [ ] Task 6: Update README and user/developer docs.
- [ ] Task 7: Update architecture and iteration context.

### Checkpoint: Consumer Experience

- [ ] Primary docs show `db.UsePostgres(...)`.
- [ ] Development docs show `db.UseInMemory(...)`.
- [ ] Sample host keeps its Development-only InMemory guard.
- [ ] Package-consumer sample builds against local package artifacts.

### Phase 3: Provider-Neutral Persistence Design

- [ ] Task 8: Document the provider-neutral EF package split.
- [ ] Task 9: Add a refactor plan for moving shared EF model types out of `AuthNet.Persistence.Postgres`.

### Checkpoint: SQL Server Readiness Plan

- [ ] The package split plan identifies all current PostgreSQL-coupled references.
- [ ] SQL Server support has a concrete package/migration strategy.
- [ ] No SQL Server runtime code is added before the provider-neutral boundary is agreed.

### Phase 4: Verification and Finalization

- [ ] Task 10: Run focused and full verification.

### Checkpoint: Complete

- [ ] Focused persistence tests pass.
- [ ] Startup tests pass.
- [ ] Package metadata and package-consumer checks pass.
- [ ] `.\scripts\verify.ps1` passes when practical.
- [ ] Slice 24 artifacts accurately distinguish implemented unified API work from future SQL Server provider work.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Public API overloads become ambiguous with existing `AddAuthNet` overloads. | High | Add compile-focused tests and choose signatures that are unambiguous for common call sites. |
| Legacy `PostgresConnectionString` and new database builder configuration conflict. | Medium | Define and test one deterministic rule before implementation. Prefer explicit builder config over legacy options or fail fast on duplicate providers. |
| InMemory becomes perceived as production-supported. | Medium | Keep Development-only sample guard and repeat docs caveat that InMemory is for tests/smoke use only. |
| SQL Server support is bolted onto PostgreSQL-specific packages. | High | Do not add `UseSqlServer` to the PostgreSQL package. First plan the provider-neutral EF boundary. |
| Moving shared persistence types later may be a breaking namespace/package change. | High | Make the split a deliberate future slice, with compatibility/type-forwarding strategy considered before code moves. |
| Migrations become confused across providers. | High | Keep provider-specific migrations in provider-specific packages and document separate EF commands. |

## Open Questions

- Should duplicate configuration, such as `PostgresConnectionString` plus `db.UseInMemory(...)`, fail fast or should the database builder always win?
- Should the preferred method name be `UsePostgres` exactly, or `UsePostgreSql` for more formal provider naming?
- Should `ConfigureDbContext(...)` remain public long term, or should it be internal/test-only after first-party providers exist?
- Should SQL Server implementation be Slice 25 after the provider-neutral refactor, or should the refactor and SQL Server provider be separate slices?

## Verification Commands

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostAuthNetPersistenceTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests
.\scripts\verify-package-metadata.ps1
.\scripts\verify-package-consumer.ps1
.\scripts\verify.ps1
```
