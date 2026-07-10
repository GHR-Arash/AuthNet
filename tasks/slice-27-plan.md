# Implementation Plan: Remove Legacy PostgreSQL Option

## Overview

Slice 27 removes the legacy `AuthNetOptions.PostgresConnectionString` compatibility path. After this slice, every host must configure persistence through the unified database builder:

```csharp
builder.Services.AddAuthNet(
    options => builder.Configuration.GetSection("AuthNet").Bind(options),
    db => db.UsePostgres(builder.Configuration.GetConnectionString("AuthNet")));
```

Supported database builder choices remain:

- `db.UsePostgres(connectionString)`
- `db.UseSqlServer(connectionString)`
- `db.UseInMemory(databaseName)` for development/test only
- `db.ConfigureDbContext(...)` for custom provider experiments

## Scope

In scope:

- Delete `AuthNetOptions.PostgresConnectionString`.
- Delete fallback startup registration that reads the legacy option.
- Remove obsolete compatibility tests.
- Update missing-database error messages to point only to the database builder.
- Update docs and compact context.

Out of scope:

- Removing PostgreSQL provider support.
- Moving provider methods into provider packages.
- Changing sample-host config shape beyond existing database builder registration.
- Adding live database integration tests.

## Verification Commands

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests
.\scripts\verify.ps1
git diff --check
```
