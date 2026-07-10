# Future Plan: SQL Server Provider

## Overview

Deferred future slice for SQL Server persistence support through a modular AuthNet provider package. The target host API is:

```csharp
builder.Services.AddAuthNet(
    options => builder.Configuration.GetSection("AuthNet").Bind(options),
    db => db.UseSqlServer(builder.Configuration.GetConnectionString("AuthNet")));
```

This work should happen after the provider-neutral EF Core model split described in `docs/slice-24/unified-database-provider-api.md`.

## Scope

In scope:

- Add `AuthNet.Persistence.EntityFrameworkCore` or equivalent provider-neutral package.
- Move shared AuthNet EF entities and DbContext/model configuration out of `AuthNet.Persistence.Postgres`.
- Add `AuthNet.Persistence.SqlServer`.
- Add `db.UseSqlServer(connectionString)`.
- Add SQL Server EF migrations.
- Add package metadata and package verification.
- Add focused tests for SQL Server provider registration.
- Update user and developer docs with SQL Server setup and migration commands.

Out of scope:

- JWT or refresh-token authentication.
- Cross-origin SPA APIs.
- Custom Identity stores.
- Multi-tenancy.
- Runtime provider switching without explicit `db.UseX(...)` setup.

## Architecture Decisions

- SQL Server support must live in its own provider package.
- PostgreSQL and SQL Server migrations remain provider-specific.
- Shared EF model types should live in a provider-neutral package to avoid making UI/API/runtime packages depend on PostgreSQL.
- The database builder remains the public configuration surface for all first-party providers.

## Verification Commands

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests
.\scripts\verify-package-metadata.ps1
.\scripts\verify-package-consumer.ps1
.\scripts\verify.ps1
```

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Shared EF type move breaks consumers. | High | Consider compatibility namespaces or type forwarding before moving public types. |
| Provider migrations conflict. | High | Keep separate migrations assemblies per provider. |
| `AuthNet.AspNetCore` becomes too heavy. | Medium | Keep provider dependencies in provider packages once the split is complete. |
| SQL Server behavior differs from PostgreSQL constraints. | Medium | Add provider-specific migration and smoke tests. |
