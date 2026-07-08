# Architecture Context

Compact architecture context for development sessions. For the shortest current-state memory, read `docs/next-iteration-context.md` first.

## Product Shape

AuthNet is a reusable .NET 10 ASP.NET Identity and Access Management component.

It plugs into host applications through ASP.NET service registration and endpoint mapping, providing account management over ASP.NET Core Identity rather than a custom identity system.

## MVP Slice 1

Build first:

- .NET 10 target.
- ASP.NET Core Identity foundation.
- EF Core persistence.
- PostgreSQL via Npgsql as the default database path.
- Razor Pages account UI.
- Cookie authentication.
- Registration disabled by default.
- Login, logout, email verification/resend, forgot/reset password, profile, verified email change, change password.
- Authenticator-app MFA for local password sign-in, with recovery codes and user-owned disable flow.
- Email sender contract, with production sender required and development logging/fake sender allowed.
- Role infrastructure using ASP.NET Core Identity roles.
- Built-in admin user-management UI under the AuthNet route prefix, protected by the `Administrator` role.
- Fixed `Administrator` role assignment from the built-in admin user detail page, with last-admin removal protection.
- Built-in account invitation flow with persisted invitations, admin create/list pages, email-delivered single-use acceptance links, and invited-user account creation.
- Generic OpenID Connect external login.
- Basic UI configuration: route prefix, app display name, layout/branding hooks.
- External login does not link to existing local accounts by email alone; account linking is initiated by an authenticated user.
- Integration tests use EF Core InMemory through an explicit test DbContext registration; production/default registration remains PostgreSQL.
- The sample host can use EF Core InMemory only in Development via `AuthNet:UseInMemoryDatabase`; this is a local convenience, not a production persistence provider.
- The sample host can explicitly bootstrap an admin user via `AuthNet:AdminBootstrap` in any environment; this is local sample behavior, not package behavior.

Deferred:

- API/JWT and refresh tokens.
- SPA authentication flows.
- Fine-grained permissions.
- Arbitrary role management, deletion, impersonation, and audit events.
- Full Razor Page override.
- Custom Identity stores.
- Provider-specific Google/Microsoft helpers.
- SMS/email OTP, remember-this-browser, admin MFA reset, required-MFA policy, multi-tenancy, SAML, passkeys/passwordless.

## Architecture Principles

- Prefer ASP.NET Core Identity behavior over custom security logic.
- Keep AuthNet as packaging, configuration, UI, middleware, and extension points around Identity.
- Use secure defaults; require explicit opt-in for public registration.
- Do not make PostgreSQL abstractions broader than needed for MVP.
- Defer flexibility until a real requirement exists.
- Expose identity through standard ASP.NET authentication and authorization mechanisms.

## Package Shape

Current MVP package/project names:

- `AuthNet.Core`: options, shared contracts, account service boundaries.
- `AuthNet.AspNetCore`: service registration, middleware, auth integration.
- `AuthNet.UI.Razor`: Razor Pages account and admin user-management UI.
- `AuthNet.Persistence.Postgres`: EF Core/Npgsql Identity store setup.
- `AuthNet.ExternalProviders`: generic OpenID Connect integration.
- `AuthNet.SampleHost`: sample Razor Pages host app.
- `AuthNet.Tests`: configuration and development seam tests.

Packable packages:

- `AuthNet.Core`
- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Persistence.Postgres`
- `AuthNet.ExternalProviders`

Non-packable projects:

- `AuthNet.SampleHost`
- `AuthNet.Tests`

Future package:

- `AuthNet.Api`: API/JWT/SPA flows after MVP slice 1.

## Canonical Commands

Use the project-local .NET 10 SDK:

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx
.\.dotnet\dotnet.exe test AuthNet.slnx
```

Canonical local verification:

```powershell
.\scripts\verify.ps1
```

Pack local package artifacts:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore
.\.dotnet\dotnet.exe pack src\AuthNet.Core\AuthNet.Core.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj --configuration Release --no-build --output .\artifacts\packages
```

Run the sample host:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Apply PostgreSQL schema:

```powershell
.\.dotnet\dotnet.exe tool install dotnet-ef --version 10.0.9 --tool-path .tools
.\.tools\dotnet-ef.exe database update --project src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --startup-project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --context AuthNetDbContext
```

## Key Integration Points

Host app should be able to configure:

- Public registration enabled/disabled.
- Cookie settings.
- Password, lockout, and email verification policy.
- PostgreSQL connection/DbContext integration.
- Email sender implementation.
- Generic OIDC provider settings.
- Account route prefix and basic UI branding/layout.
- Authenticator-app MFA account routes under the account route prefix.
- Admin user-management routes under the account route prefix, guarded by the `Administrator` role.
- Fixed `Administrator` role assignment on the admin user detail page.
- Admin invitation routes under the account route prefix, guarded by the `Administrator` role.
- Invitation acceptance route under the account route prefix.
- Invitation expiration through `AuthNetOptions.Invitations.Expiration`.

Sample-host-only development persistence:

- `AuthNet:UseInMemoryDatabase=true` in Development uses EF Core InMemory through the existing `AddAuthNet` DbContext configuration seam.
- The setting is rejected outside Development.
- Startup migrations are skipped while InMemory mode is active.

Sample-host-only admin bootstrap:

- `AuthNet:AdminBootstrap:Enabled=true` creates the `Administrator` role.
- `AuthNet:AdminBootstrap:Email` identifies the user to promote or create.
- `AuthNet:AdminBootstrap:UserName` optionally sets the username for a newly created admin.
- `AuthNet:AdminBootstrap:Password` is required only when creating a missing user.
- The bootstrap uses the same explicit configuration in Development and Production and does not change package behavior.

Conceptual setup:

```csharp
builder.Services.AddAuthNet(options =>
{
    options.EnablePublicRegistration = false;
});

app.UseAuthentication();
app.UseAuthorization();
app.MapAuthNet();
```

`UseAuthNet()` remains as a compatibility wrapper. New integrations should use `MapAuthNet()`.

## Current Canonical Docs

- Next iteration memory: `docs/next-iteration-context.md`
- Product decisions: `docs/product-decisions.md`
- Product requirements: `docs/prd.md`
- Functional requirements: `docs/functional-requirements.md`
- Integration requirements: `docs/integration-requirements.md`
- Slice 03 package readiness: `docs/slice-03/package-readiness.md`
- Slice 05 CI and publication readiness: `docs/slice-05/ci-and-publication-readiness.md`
- Slice 06 admin user management: `docs/slice-06/admin-user-management.md`
- Slice 07 admin role assignment: `docs/slice-07/admin-role-assignment.md`
- Slice 08 MFA foundation: `docs/slice-08/mfa-foundation.md`
- Slice 09 account invitation plan: `tasks/slice-09-plan.md`
- Slice 09 account invitation todo: `tasks/slice-09-todo.md`
- Slice 09 account invitations: `docs/slice-09/account-invitations.md`
- Roadmap: `docs/mvp-roadmap.md`
- Local tasks: `docs/tasks.md`

## Sync Rule

When implementation changes architecture, package boundaries, auth flow, persistence strategy, UI strategy, or MVP scope, update this file in the same change.

When finishing a meaningful milestone, update `docs/next-iteration-context.md` with the current state, verification status, and likely next work.
