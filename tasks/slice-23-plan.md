# Implementation Plan: Fluent Startup Bootstrap API

## Overview

Slice 23 replaces sample-host-specific admin bootstrap and migration boilerplate with a package-owned fluent startup API. The goal is for a package consumer to configure AuthNet startup behavior without creating scopes, resolving `UserManager` or `RoleManager`, or calling EF Core migration APIs directly.

Target consumer shape:

```csharp
builder.Services.AddAuthNet(options =>
{
    options.PostgresConnectionString = builder.Configuration.GetConnectionString("AuthNet");
});

await app.UseAuthNet(authNet => authNet
    .ApplyMigrations()
    .InitialAdministrator(
        username: "admin",
        password: "Password1!",
        email: "admin@example.com"));
```

Configuration-driven variant:

```csharp
await app.UseAuthNet(authNet => authNet
    .ApplyMigrations()
    .InitialAdministrator(builder.Configuration.GetSection("AuthNet:InitialAdministrator")));
```

## Architecture Decisions

- Make `UseAuthNet(...)` the fluent application-startup surface. It should validate configuration, optionally apply migrations, optionally bootstrap the initial administrator, and map AuthNet endpoints.
- Keep `AddAuthNet(...)` focused on service registration and existing option configuration.
- Preserve `MapAuthNet()` for endpoint-only mapping and tests, but document `UseAuthNet(...)` as the easiest full startup path for application developers.
- Move first-admin creation from `samples/AuthNet.SampleHost` into `AuthNet.AspNetCore` so package consumers do not need Identity bootstrap code.
- Keep first-admin bootstrap explicit. AuthNet must not ship hardcoded production credentials or create an admin unless the host asks for it.
- Make bootstrap idempotent. Existing users should not have passwords reset; the bootstrap should only create a missing user and ensure administrator access.
- Make migrations explicit. `ApplyMigrations()` should be opt-in and should no-op for EF Core InMemory while applying `Database.MigrateAsync()` for relational providers.

## Public API Contract

Add a startup builder type in `AuthNet.AspNetCore`:

```csharp
public sealed class AuthNetStartupBuilder
{
    public AuthNetStartupBuilder ApplyMigrations(bool enabled = true);

    public AuthNetStartupBuilder InitialAdministrator(
        string username,
        string password,
        string email);

    public AuthNetStartupBuilder InitialAdministrator(
        IConfiguration configurationSection);
}
```

Add a WebApplication extension:

```csharp
public static Task<WebApplication> UseAuthNet(
    this WebApplication app,
    Action<AuthNetStartupBuilder>? configure = null);
```

Expected behavior:

- `UseAuthNet()` with no configuration preserves current compatibility behavior and maps AuthNet endpoints.
- `UseAuthNet(configure)` runs configured startup tasks before mapping endpoints.
- `InitialAdministrator(...)` creates the `Administrator` role if missing.
- `InitialAdministrator(...)` creates the user if missing, confirms email by default, enables lockout, and assigns `Administrator`.
- If the user already exists by email, bootstrap only ensures `Administrator` role membership.
- Password is required only when creating a missing admin user.
- Invalid Identity results throw an `AuthNetConfigurationException` or `InvalidOperationException` with actionable error text and no secret values.
- Raw passwords are never logged, persisted outside Identity, or included in exception messages.

## Task List

### Phase 1: Public Contract

- [ ] Task 1: Define startup options and fluent builder.
- [ ] Task 2: Add `UseAuthNet(...)` overload that executes startup tasks and maps AuthNet endpoints.

### Checkpoint: Contract

- [ ] The new API compiles.
- [ ] Existing `MapAuthNet()` callers still compile.
- [ ] Existing no-argument `UseAuthNet()` behavior remains available.

### Phase 2: Startup Services

- [ ] Task 3: Implement provider-aware migration runner.
- [ ] Task 4: Implement initial administrator seeder.
- [ ] Task 5: Wire startup task execution into `UseAuthNet(...)`.

### Checkpoint: Behavior

- [ ] Migration opt-in behavior is covered by tests.
- [ ] Admin bootstrap create/promote/idempotency behavior is covered by tests.
- [ ] Existing account and admin route tests still pass.

### Phase 3: Consumer Experience

- [ ] Task 6: Replace sample-host migration and admin bootstrap boilerplate with fluent AuthNet startup.
- [ ] Task 7: Update README and consumer docs to show development and production startup modes.
- [ ] Task 8: Update architecture/context docs to reflect package-owned startup bootstrap.

### Checkpoint: Complete

- [ ] Build succeeds.
- [ ] Focused startup/bootstrap tests pass.
- [ ] Package consumer sample still compiles.
- [ ] Full verification passes when practical.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Changing `UseAuthNet()` semantics could surprise existing users. | Medium | Preserve no-argument behavior and add startup work only when fluent options are supplied. |
| Applying migrations at web startup is not acceptable for every production team. | Medium | Keep `ApplyMigrations()` explicit and document deployment-job migration as an alternative. |
| Admin bootstrap could reset or weaken existing accounts. | High | Never reset existing passwords by default; only create missing users and ensure role membership. |
| InMemory provider does not support relational migrations. | Medium | Detect relational provider before `MigrateAsync()` and no-op for InMemory with clear logging. |
| Exceptions might leak configured passwords. | High | Centralize error formatting and never include password values. |

## Open Questions

- Should the config section property be `UserName` or `Username`? Prefer `UserName` to match ASP.NET Core Identity naming.
- Should the config-based bootstrap be named `InitialAdministrator(...)` only, or should there also be an explicit `InitialAdministratorFromConfiguration(...)` alias for readability?
- Should failed bootstrap use `AuthNetConfigurationException` consistently, or reserve it for configuration validation and use `InvalidOperationException` for Identity failures?

## Verification Commands

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetStartup
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter SampleHost
.\scripts\verify.ps1
```
