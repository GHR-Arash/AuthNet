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

SQL Server support should not be added to `AuthNet.Persistence.Postgres`. Slice 25 moved shared EF Core model types into a provider-neutral package before adding `db.UseSqlServer(...)`.

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

## Coupling Untangled in Slice 25

These projects should reference `AuthNet.Persistence.EntityFrameworkCore` for common EF model types:

- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Api`
- `AuthNet.Tests`
- `AuthNet.SampleHost`

The split preserves PostgreSQL migrations in the PostgreSQL package.

## Compatibility Notes

- Slice 25 moved shared EF model types into `AuthNet.Persistence.EntityFrameworkCore`; consumers should reference that package/namespace for direct `AuthNetUser` or `AuthNetDbContext` access.
- Package dependency changes can affect `AuthNet.AspNetCore` transitive restore behavior.
- EF migration commands must stay provider-specific.
- SQL Server and PostgreSQL migrations must not share a migrations assembly.

## Recommended Follow-Up

Implement SQL Server as a separate slice on top of the provider-neutral EF package boundary. Keep JWT, custom Identity stores, multi-tenancy, and cross-origin APIs out of that provider slice.
