# Slice 24: Unified Database Provider API

## Current Scope

Slice 24 introduces one public database configuration surface for AuthNet service registration:

```csharp
builder.Services.AddAuthNet(
    options => builder.Configuration.GetSection("AuthNet").Bind(options),
    db => db.UsePostgres(builder.Configuration.GetConnectionString("AuthNet")));
```

Development and test hosts can use:

```csharp
builder.Services.AddAuthNet(
    options => builder.Configuration.GetSection("AuthNet").Bind(options),
    db => db.UseInMemory("AuthNet.Dev"));
```

`AuthNetOptions.PostgresConnectionString` remains as a legacy compatibility path for now. New integrations should use `db.UsePostgres(...)`.

## Provider-Neutral EF Split

SQL Server support should not be added to `AuthNet.Persistence.Postgres`. Before adding `db.UseSqlServer(...)`, AuthNet should split shared EF Core model types from provider-specific packages.

Proposed package shape:

- `AuthNet.Persistence.EntityFrameworkCore`
  - `AuthNetDbContext`
  - `AuthNetUser`
  - `AuthNetInvitation`
  - `AuthNetInvitationToken`
  - `AuthNetAuditEvent`
  - shared EF model configuration
- `AuthNet.Persistence.Postgres`
  - Npgsql dependency
  - PostgreSQL migrations
  - `UsePostgres(...)` provider extension if moved out of `AuthNet.AspNetCore`
- `AuthNet.Persistence.SqlServer`
  - SQL Server EF provider dependency
  - SQL Server migrations
  - `UseSqlServer(...)` provider extension

## Current Coupling to Untangle

These projects currently reference `AuthNet.Persistence.Postgres` because common EF model types live there:

- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Api`
- `AuthNet.Tests`
- `AuthNet.SampleHost`

The split should move shared model references to the provider-neutral package while preserving PostgreSQL migrations in the PostgreSQL package.

## Compatibility Risks

- Namespace changes can break package consumers who reference `AuthNet.Persistence.Postgres.AuthNetUser` or `AuthNetDbContext` directly.
- Package dependency changes can affect `AuthNet.AspNetCore` transitive restore behavior.
- EF migration commands must stay provider-specific.
- SQL Server and PostgreSQL migrations must not share a migrations assembly.

## Recommended Follow-Up

Implement SQL Server as a separate slice after the provider-neutral EF package boundary is agreed. Keep JWT, custom Identity stores, multi-tenancy, and cross-origin APIs out of that provider slice.
