# Implementation Plan: Provider-Neutral EF Persistence Split

## Overview

Slice 25 prepares AuthNet for multiple relational database providers by moving shared EF Core Identity model types out of `AuthNet.Persistence.Postgres` into a provider-neutral package. PostgreSQL remains the only runtime relational provider after this slice. SQL Server support is deliberately left for the next slice so the package-boundary change can be implemented and verified independently.

Target package shape after Slice 25:

```text
AuthNet.Persistence.EntityFrameworkCore
  Shared AuthNet EF model:
  - AuthNetDbContext
  - AuthNetUser
  - AuthNetInvitation
  - AuthNetInvitationToken
  - AuthNetAuditEvent

AuthNet.Persistence.Postgres
  PostgreSQL provider package:
  - Npgsql dependency
  - PostgreSQL migrations
  - PostgreSQL migration snapshot
```

Target dependency direction:

```text
AuthNet.AspNetCore
AuthNet.Api
AuthNet.UI.Razor
AuthNet.Tests
samples/AuthNet.SampleHost
        -> AuthNet.Persistence.EntityFrameworkCore

AuthNet.Persistence.Postgres
        -> AuthNet.Persistence.EntityFrameworkCore
        -> AuthNet.Core
```

## Architecture Decisions

- Add a new packable project named `AuthNet.Persistence.EntityFrameworkCore`.
- Move shared EF entities and `AuthNetDbContext` into the new project.
- Use namespace `AuthNet.Persistence.EntityFrameworkCore` for moved shared types. This is a source-level namespace change, but package publication is still pre-public and the modular boundary matters more than keeping a PostgreSQL namespace on provider-neutral types.
- Keep PostgreSQL migrations in `AuthNet.Persistence.Postgres`.
- Update PostgreSQL migration designer files and model snapshot to reference the provider-neutral `AuthNetDbContext` and entity type names.
- Keep `db.UsePostgres(...)` in `AuthNet.AspNetCore` for now. Moving provider extension methods into provider packages can happen with the SQL Server provider slice.
- Keep `db.UseInMemory(...)` in `AuthNet.AspNetCore` for development/test paths.
- Do not add SQL Server runtime code, package references, migrations, or docs in this slice.

## Public/Package Impact

New package:

- `AuthNet.Persistence.EntityFrameworkCore`

Existing package changes:

- `AuthNet.Persistence.Postgres` depends on `AuthNet.Persistence.EntityFrameworkCore`.
- `AuthNet.AspNetCore` should depend on `AuthNet.Persistence.EntityFrameworkCore` and `AuthNet.Persistence.Postgres` while `db.UsePostgres(...)` remains in `AuthNet.AspNetCore`.
- `AuthNet.Api` and `AuthNet.UI.Razor` should depend on `AuthNet.Persistence.EntityFrameworkCore`, not `AuthNet.Persistence.Postgres`.

Package verification should expect one additional package artifact:

```text
AuthNet.Persistence.EntityFrameworkCore.0.1.0.nupkg
```

## Task List

### Phase 1: Project Boundary

- [ ] Task 1: Add `AuthNet.Persistence.EntityFrameworkCore` project and package metadata.
- [ ] Task 2: Move shared EF model types into the provider-neutral project.
- [ ] Task 3: Update project references and package manifest.

### Checkpoint: Boundary Compiles

- [ ] Solution restores.
- [ ] Solution builds.
- [ ] Package manifest expects the new provider-neutral package.

### Phase 2: Source Reference Migration

- [ ] Task 4: Update runtime code usings and references.
- [ ] Task 5: Update test and sample code usings and references.
- [ ] Task 6: Update PostgreSQL migrations and snapshot to compile against the moved model.

### Checkpoint: Behavior Preserved

- [ ] Existing route/account/admin tests pass.
- [ ] Database builder tests pass.
- [ ] PostgreSQL migration assembly still builds.

### Phase 3: Docs and Future SQL Server Readiness

- [ ] Task 7: Update user/developer docs and compact context.
- [ ] Task 8: Update Slice 24/SQL Server future docs to reflect the completed provider-neutral split.

### Checkpoint: Documentation Current

- [ ] Architecture docs show the new package shape.
- [ ] SQL Server plan now starts from an existing provider-neutral EF package.
- [ ] No docs imply shared model types still live in `AuthNet.Persistence.Postgres`.

### Phase 4: Verification and Commit

- [ ] Task 9: Run focused and full verification, review, and commit.

### Checkpoint: Complete

- [ ] Full verification passes.
- [ ] Package output includes the new provider-neutral package.
- [ ] SQL Server remains deferred to a separate provider slice.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Moving EF model namespaces causes EF to detect a large model diff later. | High | Update the PostgreSQL model snapshot in the same slice and run focused migration/build checks. |
| Runtime packages accidentally keep depending on PostgreSQL for shared types. | High | Use `rg "AuthNet.Persistence.Postgres"` after source updates and allow it only in PostgreSQL provider/migration docs. |
| Package-consumer restore misses the new package artifact. | Medium | Update `scripts/package-manifest.ps1` and run `.\scripts\verify.ps1`. |
| Source compatibility breaks for consumers using PostgreSQL namespace types. | Medium | Accept this before first public release; document the namespace move in developer context. |
| Scope expands into SQL Server implementation. | High | Keep `UseSqlServer`, SQL Server package, and SQL Server migrations out of Slice 25. |

## Open Questions

- Should moved types use namespace `AuthNet.Persistence.EntityFrameworkCore` exactly, or a shorter provider-neutral namespace such as `AuthNet.Persistence`?
- Should the SQL Server slice move `UsePostgres(...)` out of `AuthNet.AspNetCore` into a provider-package extension at the same time it adds `UseSqlServer(...)`?
- Do we want compatibility type-forwarding or aliases before public package publication, or is the namespace break acceptable because this is still pre-publication?

## Verification Commands

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests
.\scripts\verify-package-metadata.ps1
.\scripts\verify-package-consumer.ps1
.\scripts\verify.ps1
git diff --check
git status --short
```
