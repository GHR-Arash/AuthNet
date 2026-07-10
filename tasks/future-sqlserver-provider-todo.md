# Future Todo: SQL Server Provider

## Task 1: Split Provider-Neutral EF Model

**Description:** Move shared AuthNet EF model types into a provider-neutral package.

**Acceptance criteria:**
- [ ] Shared `AuthNetDbContext` and entity types live outside `AuthNet.Persistence.Postgres`.
- [ ] `AuthNet.UI.Razor`, `AuthNet.Api`, and `AuthNet.AspNetCore` no longer reference PostgreSQL only to access shared model types.
- [ ] PostgreSQL migrations continue to work.

**Verification:**
- [ ] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [ ] Existing integration tests pass.

## Task 2: Add SQL Server Provider Package

**Description:** Add `AuthNet.Persistence.SqlServer` with the EF Core SQL Server provider dependency and package metadata.

**Acceptance criteria:**
- [ ] Project is included in the solution.
- [ ] Package metadata matches current AuthNet package conventions.
- [ ] Package references provider-neutral EF model package.

**Verification:**
- [ ] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

## Task 3: Add `db.UseSqlServer(...)`

**Description:** Add the SQL Server database builder extension.

**Acceptance criteria:**
- [ ] Host code can call `db.UseSqlServer(connectionString)`.
- [ ] Empty connection string fails fast.
- [ ] Duplicate provider selection fails fast.

**Verification:**
- [ ] Focused database builder tests pass.

## Task 4: Add SQL Server Migrations

**Description:** Generate SQL Server migrations for the shared AuthNet EF model.

**Acceptance criteria:**
- [ ] Initial SQL Server migration exists.
- [ ] Migration command is documented.
- [ ] PostgreSQL migrations remain unchanged.

**Verification:**
- [ ] SQL Server migration can be generated/applied against a test database when available.

## Task 5: Update Docs and Package Verification

**Description:** Document SQL Server setup and add the provider to package verification.

**Acceptance criteria:**
- [ ] User docs show `db.UseSqlServer(...)`.
- [ ] Developer docs include SQL Server migration command.
- [ ] Package manifest verification includes `AuthNet.Persistence.SqlServer`.

**Verification:**
- [ ] `.\scripts\verify-package-metadata.ps1`
- [ ] `.\scripts\verify.ps1`
