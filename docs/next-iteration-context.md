# Next Iteration Context

Compact memory for future development sessions. Read this first, then `docs/architecture-context.md` only if deeper context is needed.

## Current State

- Repo is a Git repository on `master`.
- Latest known commits:
  - Current HEAD: Add package publication finalization
  - `b26d631 Add package consumer sample`
  - `453b0d1 Add SPA invitation acceptance JSON`
  - Add SPA external login JSON orchestration
  - `7d2d823 Add SPA MFA JSON workflows`
  - `69766bd Add SPA account workflow completion`
  - `d398cb7 Add OpenAPI document endpoint`
  - `328d103 Add SPA API workflow foundation`
  - `2e70e6f Add role management and permissions`
  - `28870a1 Add sample SMTP email sender`
  - `aab30ad Add admin audit events`
  - `865a9d9 Add admin direct user creation`
- Slice 01/MVP account flows are implemented and `docs/tasks.md` is checked off.
- Slice 02 integration hardening is implemented and tracked in `tasks/slice-02-plan.md` and `tasks/slice-02-todo.md`.
- Slice 03 package readiness is implemented and tracked in `tasks/slice-03-plan.md`, `tasks/slice-03-todo.md`, and `docs/slice-03/`.
- Slice 04 development InMemory sample persistence is implemented and tracked in `tasks/slice-04-plan.md`, `tasks/slice-04-todo.md`, and `docs/slice-04/`.
- Slice 05 CI and publication metadata readiness is implemented and tracked in `tasks/slice-05-plan.md`, `tasks/slice-05-todo.md`, and `docs/slice-05/`.
- Slice 06 admin user management UI is implemented and tracked in `tasks/slice-06-plan.md`, `tasks/slice-06-todo.md`, and `docs/slice-06/`.
- Slice 07 admin role assignment UI is implemented and tracked in `tasks/slice-07-plan.md`, `tasks/slice-07-todo.md`, and `docs/slice-07/`.
- Slice 08 MFA foundation is implemented and tracked in `tasks/slice-08-plan.md`, `tasks/slice-08-todo.md`, and `docs/slice-08/`.
- Slice 09 account invitation flow is implemented and tracked in `tasks/slice-09-plan.md`, `tasks/slice-09-todo.md`, and `docs/slice-09/`.
- Slice 10 admin direct user creation is implemented and tracked in `tasks/slice-10-plan.md` and `tasks/slice-10-todo.md`.
- Slice 11 admin audit events are implemented and tracked in `tasks/slice-11-plan.md` and `tasks/slice-11-todo.md`.
- Slice 12 real email sender sample is implemented and tracked in `tasks/slice-12-plan.md` and `tasks/slice-12-todo.md`.
- Slice 13 role management and permission enhancement is implemented and tracked in `tasks/slice-13-plan.md` and `tasks/slice-13-todo.md`.
- Slice 14 SPA workflow foundation is implemented and tracked in `tasks/slice-14-plan.md` and `tasks/slice-14-todo.md`.
- Slice 15 OpenAPI document endpoint is implemented and tracked in `tasks/slice-15-plan.md` and `tasks/slice-15-todo.md`.
- Slice 16 SPA account workflow completion is implemented and tracked in `tasks/slice-16-plan.md` and `tasks/slice-16-todo.md`.
- Slice 17 SPA MFA JSON workflows are implemented and tracked in `tasks/slice-17-plan.md` and `tasks/slice-17-todo.md`.
- Slice 18 SPA external-login JSON orchestration is implemented and tracked in `tasks/slice-18-plan.md` and `tasks/slice-18-todo.md`.
- Slice 19 SPA invitation acceptance JSON is implemented and tracked in `tasks/slice-19-plan.md` and `tasks/slice-19-todo.md`.
- Slice 20 committed package-consumer sample is implemented and tracked in `tasks/slice-20-plan.md` and `tasks/slice-20-todo.md`.
- Slice 21 package publication finalization is implemented and tracked in `tasks/slice-21-plan.md`, `tasks/slice-21-todo.md`, and `docs/slice-21/package-publication-finalization.md`.
- Slice 22 built-in UI polish is implemented and tracked in `tasks/slice-22-plan.md` and `tasks/slice-22-todo.md`.
- Slice 23 fluent startup bootstrap API is implemented and tracked in `tasks/slice-23-plan.md` and `tasks/slice-23-todo.md`.
- Slice 24 unified database provider API is implemented and tracked in `tasks/slice-24-plan.md`, `tasks/slice-24-todo.md`, and `docs/slice-24/unified-database-provider-api.md`.
- Slice 25 provider-neutral EF persistence split is implemented and tracked in `tasks/slice-25-plan.md` and `tasks/slice-25-todo.md`.
- Slice 26 SQL Server provider is implemented and tracked in `tasks/slice-26-plan.md` and `tasks/slice-26-todo.md`.

## Implemented Product Surface

- .NET 10 solution: `AuthNet.slnx`.
- Project-local SDK: `.dotnet\dotnet.exe` (ignored).
- Projects:
  - `src/AuthNet.Core`
  - `src/AuthNet.AspNetCore`
  - `src/AuthNet.Persistence.EntityFrameworkCore`
  - `src/AuthNet.Persistence.Postgres`
  - `src/AuthNet.Persistence.SqlServer`
  - `src/AuthNet.UI.Razor`
  - `src/AuthNet.ExternalProviders`
  - `src/AuthNet.Api`
  - `samples/AuthNet.SampleHost`
  - `samples/AuthNet.PackageConsumer`
  - `tests/AuthNet.Tests`
- Built-in Razor Pages account UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account`.
- Built-in AuthNet home page is mapped under the configured account route prefix, `/auth` by default.
- Built-in fallback UI ships a package-owned navigation shell and CSS at `_content/AuthNet.UI.Razor/authnet.css`.
- Account UI includes login by email or username, logout, registration, confirm/resend email, forgot/reset password, profile, verified email change, change password, and external login/linking.
- Account UI includes authenticator-app MFA setup, MFA login challenge, recovery-code login, recovery-code count display, and user-owned MFA disable.
- Built-in Razor Pages admin user-management UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users`.
- Built-in Razor Pages admin audit UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Audit`.
- Built-in Razor Pages admin invitation UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations`.
- Admin user routes are `/auth/admin/users`, `/auth/admin/users/new`, and `/auth/admin/users/{id}` by default, protected by the ASP.NET Core Identity `Administrator` role or AuthNet user permissions.
- Admin role routes are `/auth/admin/roles`, `/auth/admin/roles/new`, and `/auth/admin/roles/{id}` by default, protected by the ASP.NET Core Identity `Administrator` role or AuthNet role permissions.
- Same-origin SPA JSON routes are mapped under `/auth/api` by default: session, profile read/update, login, logout, register, forgot-password, reset-password, resend-confirmation, confirm-email, change-password, MFA status/setup/disable, recovery-code count/regeneration, MFA challenge sign-in, recovery-code sign-in, external-provider discovery, external-login challenge/callback, signed-in external-login link challenge/callback, and invitation acceptance status/completion.
- AuthNet SPA OpenAPI JSON is mapped at `/auth/api/openapi.json` by default.
- Admin audit route is `/auth/admin/audit` by default, protected by the ASP.NET Core Identity `Administrator` role or `authnet.audit.view`.
- Admin invitation routes are `/auth/admin/invitations` and `/auth/admin/invitations/new` by default, protected by the ASP.NET Core Identity `Administrator` role or `authnet.invitations.manage`.
- Invitation acceptance route is `/auth/invitations/accept`.
- Admin UI supports user list/search, direct user creation, user detail, confirm/unconfirm email, lock/unlock, reset access failed count, and role assignment with `Administrator` last-admin protection.
- Admin UI supports role list/create/detail and built-in AuthNet permission assignment on roles.
- Admin audit UI supports recent audit event listing and filters by action, actor, target, and date range.
- Admin invitation UI supports invitation list/create, duplicate pending invite rejection, existing-user rejection, and email-delivered single-use acceptance links.
- Invitation acceptance creates a local Identity user, confirms the invited email, marks the invitation accepted, and signs in the user.
- AuthNet packages do not create default admin credentials unless the host explicitly configures `UseAuthNet(...InitialAdministrator(...))`.
- ASP.NET Core Identity user/context in `AuthNet.Persistence.EntityFrameworkCore`.
- Persisted account invitation and admin audit EF model in `AuthNet.Persistence.EntityFrameworkCore`.
- PostgreSQL provider dependencies and migrations in `AuthNet.Persistence.Postgres`.
- SQL Server provider dependencies and migrations in `AuthNet.Persistence.SqlServer`.
- Initial PostgreSQL Identity migration exists in `src/AuthNet.Persistence.Postgres/Migrations`.
- Initial SQL Server migration exists in `src/AuthNet.Persistence.SqlServer/Migrations`.
- Generic OpenID Connect extension exists in `AuthNet.ExternalProviders`.
- External login signs in already linked accounts, lets authenticated users link from profile, and no longer links existing local accounts by email alone.
- Sample host wires `AddAuthNet`, `UseAuthentication`, `UseAuthorization`, and `MapAuthNet`.
- Sample host home page, shared navigation, and protected `/Admin` page link to the built-in admin user list, direct user creation, role management, invitation pages, and SPA smoke page.
- Sample host home page and shared navigation link to the built-in AuthNet home page.
- Sample host SPA/home pages link to `/auth/api/openapi.json`; the sample SPA page can manually inspect and accept invitation tokens.
- Sample host home page, shared navigation, and protected `/Admin` page link to the built-in admin audit page.
- `UseAuthNet()` remains as a compatibility wrapper.
- AuthNet UI ships fallback shared `_Layout.cshtml`, `_ValidationScriptsPartial.cshtml`, and `_AuthNetBrand.cshtml` so built-in pages render in a bare host.
- `AuthNet.Tests` has an in-memory integration test host covering routes, registration, confirm/resend email, forgot/reset password, profile update, change password, verified email change, external-login safety, endpoint mapping compatibility, and admin user management.
- `AuthNet.Tests` covers Razor and SPA authenticator-app MFA setup, MFA login challenge, recovery-code login, recovery-code count/regeneration, and disable flows.
- `AuthNet.Tests` covers Razor and SPA invitation creation, email delivery, acceptance, expired invitations, reused invitations, invalid tokens, duplicate pending invitations, existing-user rejection, and route protection.
- MVP packable packages are `AuthNet.Core`, `AuthNet.AspNetCore`, `AuthNet.UI.Razor`, `AuthNet.Persistence.EntityFrameworkCore`, `AuthNet.Persistence.Postgres`, `AuthNet.Persistence.SqlServer`, `AuthNet.ExternalProviders`, and `AuthNet.Api`.
- Package metadata is centralized in `Directory.Build.props`; local packages output to ignored `artifacts/packages`.
- Committed package-consumer sample at `samples/AuthNet.PackageConsumer` references `AuthNet.AspNetCore` `0.1.0` from local package artifacts and is intentionally outside `AuthNet.slnx`.
- Package verification shares `scripts/package-manifest.ps1` across output, metadata, and package-consumer checks.
- Package metadata verification is available through `scripts/verify-package-metadata.ps1`; strict public-publication metadata mode validates repository URL and packaged MIT `LICENSE`.
- Sample host supports Development-only EF Core InMemory via `AuthNet:UseInMemoryDatabase=true`; PostgreSQL and SQL Server are production/package persistence paths.
- AuthNet service registration supports a unified database builder: `db.UsePostgres(connectionString)` for PostgreSQL, `db.UseSqlServer(connectionString)` for SQL Server, and `db.UseInMemory(databaseName)` for development/test InMemory.
- `AuthNetOptions.PostgresConnectionString` remains as a legacy compatibility path through Slice 26 and should be removed in Slice 27.
- Sample host creates a demo admin user through the package fluent startup API and supports config-driven initial administrator setup through `AuthNet:InitialAdministrator:{Enabled,UserName,Email,Password}`.
- `UseAuthNet(Action<AuthNetStartupBuilder>)` validates AuthNet configuration, optionally applies migrations, optionally creates/promotes an initial administrator, and maps AuthNet endpoints.
- Sample host supports a sample SMTP email sender through `AuthNet:Email:Smtp` when `AuthNet:UseDevelopmentEmailSender=false`; development email remains the default local sender.
- `samples/AuthNet.SampleHost/appsettings.SmtpSample.json` shows SMTP settings without committed secrets.
- Local verification is centralized in `scripts/verify.ps1`.
- GitHub Actions verify-only CI exists at `.github/workflows/ci.yml`; it does not publish or upload packages.
- GitHub Actions NuGet release workflow exists at `.github/workflows/nuget-release.yml`; it runs after pushes or merges to `master` and publishes generated packages with the `NUGET_API_KEY` repository secret.

## Current Verification

Known passing commands:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetRouteTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetLoginTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetAdminUserTests
.\scripts\verify.ps1
.\scripts\verify-package-metadata.ps1
.\scripts\verify-package-consumer.ps1
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetDatabaseBuilderTests
```

Latest full test count: 182 passing tests.

Slice 14 focused SPA API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetSpaApiTests
```

Slice 15 focused OpenAPI tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests
```

Slice 16 focused SPA account workflow tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests
```

Slice 17 focused SPA MFA API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaMfaApiTests
```

Slice 18 focused SPA external-login API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaExternalLoginApiTests
```

Slice 19 focused SPA invitation API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaInvitationApiTests
```

Login regression focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetLoginTests
```

Slice 04 focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter SampleHostAuthNetPersistenceTests
```

Slice 06 focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests
```

Slice 07, Slice 10, and Slice 13 user-role focused tests are covered by the same admin test class:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests
```

Slice 08 focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetMfaTests
```

Slice 09 focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests
```

Slice 11 focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests
```

Slice 13 focused role tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetRoleTests
```

Slice 13 focused permission tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetPermissionTests
```

Sample host admin bootstrap focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests
```

Slice 12 focused sample email sender tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostEmailSenderTests
```

Known passing package commands:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore
.\.dotnet\dotnet.exe pack src\AuthNet.Core\AuthNet.Core.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.EntityFrameworkCore\AuthNet.Persistence.EntityFrameworkCore.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.SqlServer\AuthNet.Persistence.SqlServer.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Api\AuthNet.Api.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj --configuration Release --no-build --output .\artifacts\packages
```

Known passing committed package-consumer smoke:

```powershell
.\scripts\verify-package-consumer.ps1
```

Known passing package metadata verification:

```powershell
.\scripts\verify-package-metadata.ps1
```

Manual NuGet publish command:

```powershell
.\scripts\publish-nuget.ps1 -ApiKey $env:NUGET_API_KEY -SkipDuplicate
```

Known passing sample startup:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --no-build --no-launch-profile --urls http://127.0.0.1:5127
```

Expected startup signal:

```text
Now listening on: http://127.0.0.1:5127
Application started.
```

## Important Constraints

- Keep MVP slice 1 server-rendered and cookie-based.
- Do not add JWT or refresh-token flows unless explicitly re-scoped.
- Do not replace ASP.NET Core Identity primitives.
- PostgreSQL and SQL Server EF Core providers are production/package persistence paths.
- Configure PostgreSQL through `db.UsePostgres(connectionString)` for new integrations.
- Configure SQL Server through `db.UseSqlServer(connectionString)` for new integrations.
- Integration tests and sample-host Development mode can use EF Core InMemory; this is not a production persistence provider.
- Configure development/test InMemory through `db.UseInMemory(databaseName)`.
- Admin UI uses `Administrator` as a superuser role and a bounded AuthNet built-in UI permission catalog backed by Identity role claims.
- Do not add host-defined custom permission catalogs, tenant-scoped permissions, role deletion, or API/JWT permission flows unless explicitly re-scoped.
- AuthNet packages must not ship hardcoded default admin credentials; sample-host demo admin creation is local sample behavior only.
- Production must use a real `IAuthNetEmailSender`; development sender is rejected in production. The repository sample host can demonstrate this with its SMTP sender, but package consumers still own their production sender.
- Public registration remains disabled by default.
- External auto-provisioning requires a verified provider email claim.
- Keep `docs/architecture-context.md` compact and synchronized when architecture changes.

## Documentation Map

For library consumers:

- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/users/account-pages.md`

For contributors:

- `docs/developer/onboarding.md`
- `docs/developer/quick-start.md`

For product/architecture:

- `docs/architecture-context.md`
- `docs/product-decisions.md`
- `tasks/slice-02-plan.md`
- `tasks/slice-02-todo.md`
- `tasks/slice-03-plan.md`
- `tasks/slice-03-todo.md`
- `tasks/slice-04-plan.md`
- `tasks/slice-04-todo.md`
- `tasks/slice-05-plan.md`
- `tasks/slice-05-todo.md`
- `tasks/slice-06-plan.md`
- `tasks/slice-06-todo.md`
- `tasks/slice-07-plan.md`
- `tasks/slice-07-todo.md`
- `tasks/slice-08-plan.md`
- `tasks/slice-08-todo.md`
- `tasks/slice-09-plan.md`
- `tasks/slice-09-todo.md`
- `tasks/slice-10-plan.md`
- `tasks/slice-10-todo.md`
- `tasks/slice-11-plan.md`
- `tasks/slice-11-todo.md`
- `tasks/slice-12-plan.md`
- `tasks/slice-12-todo.md`
- `tasks/slice-13-plan.md`
- `tasks/slice-13-todo.md`
- `tasks/slice-14-plan.md`
- `tasks/slice-14-todo.md`
- `tasks/slice-15-plan.md`
- `tasks/slice-15-todo.md`
- `tasks/slice-16-plan.md`
- `tasks/slice-16-todo.md`
- `tasks/slice-17-plan.md`
- `tasks/slice-17-todo.md`
- `tasks/slice-18-plan.md`
- `tasks/slice-18-todo.md`
- `tasks/slice-19-plan.md`
- `tasks/slice-19-todo.md`
- `tasks/slice-20-plan.md`
- `tasks/slice-20-todo.md`
- `tasks/slice-21-plan.md`
- `tasks/slice-21-todo.md`
- `docs/slice-21/package-publication-finalization.md`
- `docs/slice-03/package-readiness.md`
- `docs/slice-03/package-consumption-smoke.md`
- `docs/slice-04/development-inmemory.md`
- `docs/slice-05/ci-and-publication-readiness.md`
- `docs/slice-06/admin-user-management.md`
- `docs/slice-07/admin-role-assignment.md`
- `docs/slice-08/mfa-foundation.md`
- `docs/slice-09/account-invitations.md`
- `docs/tasks.md`
- `docs/prd.md`

## Likely Next Work

Public package publication and admin JSON APIs are intentionally paused for now.

Other candidates:

- Confirm XML documentation, signing, and trusted-publishing decisions before changing the current NuGet publication policy.
- Remove `AuthNetOptions.PostgresConnectionString` compatibility in Slice 27 so all database selection goes through the database builder API.
- Pick up the future admin JSON API plan only if admin automation becomes the priority.

Before starting any next feature, check whether it belongs to MVP slice 1 or deferred scope.
