# Slice 27 Todo: Remove Legacy PostgreSQL Option

## Task 1: Remove Legacy Option

**Acceptance criteria:**
- [ ] `AuthNetOptions.PostgresConnectionString` is deleted.
- [ ] No code reads `PostgresConnectionString`.
- [ ] Missing database configuration requires database builder usage.

**Verification:**
- [ ] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

## Task 2: Remove Compatibility Tests and Docs

**Acceptance criteria:**
- [ ] Legacy PostgreSQL option tests are removed.
- [ ] Docs no longer mention `AuthNetOptions.PostgresConnectionString`.
- [ ] Slice 24 docs point to the database builder as the only configuration API.

**Verification:**
- [ ] `rg "PostgresConnectionString" src tests docs README.md context.md tasks`

## Task 3: Verify and Commit

**Acceptance criteria:**
- [ ] Focused database builder tests pass.
- [ ] Focused startup tests pass.
- [ ] Full verification passes.
- [ ] Slice 27 is committed independently.

**Verification:**
- [ ] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests`
- [ ] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests`
- [ ] `.\scripts\verify.ps1`
- [ ] `git diff --check`
