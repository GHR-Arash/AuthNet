# Next Iteration Context

Compact memory for future development sessions. Read this first, then `docs/architecture-context.md` only if deeper context is needed.

## Current State

- Repo is a Git repository on `master`.
- Latest known commits:
  - Current HEAD: Add SPA API workflow foundation
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

## Implemented Product Surface

- .NET 10 solution: `AuthNet.slnx`.
- Project-local SDK: `.dotnet\dotnet.exe` (ignored).
- Projects:
  - `src/AuthNet.Core`
  - `src/AuthNet.AspNetCore`
  - `src/AuthNet.Persistence.Postgres`
  - `src/AuthNet.UI.Razor`
  - `src/AuthNet.ExternalProviders`
  - `src/AuthNet.Api`
  - `samples/AuthNet.SampleHost`
  - `tests/AuthNet.Tests`
- Built-in Razor Pages account UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account`.
- Account UI includes login by email or username, logout, registration, confirm/resend email, forgot/reset password, profile, verified email change, change password, and external login/linking.
- Account UI includes authenticator-app MFA setup, MFA login challenge, recovery-code login, recovery-code count display, and user-owned MFA disable.
- Built-in Razor Pages admin user-management UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users`.
- Built-in Razor Pages admin audit UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Audit`.
- Built-in Razor Pages admin invitation UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations`.
- Admin user routes are `/auth/admin/users`, `/auth/admin/users/new`, and `/auth/admin/users/{id}` by default, protected by the ASP.NET Core Identity `Administrator` role or AuthNet user permissions.
- Admin role routes are `/auth/admin/roles`, `/auth/admin/roles/new`, and `/auth/admin/roles/{id}` by default, protected by the ASP.NET Core Identity `Administrator` role or AuthNet role permissions.
- Same-origin SPA JSON routes are mapped under `/auth/api` by default: session, profile, login, logout, register, forgot-password, and resend-confirmation.
- Admin audit route is `/auth/admin/audit` by default, protected by the ASP.NET Core Identity `Administrator` role or `authnet.audit.view`.
- Admin invitation routes are `/auth/admin/invitations` and `/auth/admin/invitations/new` by default, protected by the ASP.NET Core Identity `Administrator` role or `authnet.invitations.manage`.
- Invitation acceptance route is `/auth/invitations/accept`.
- Admin UI supports user list/search, direct user creation, user detail, confirm/unconfirm email, lock/unlock, reset access failed count, and role assignment with `Administrator` last-admin protection.
- Admin UI supports role list/create/detail and built-in AuthNet permission assignment on roles.
- Admin audit UI supports recent audit event listing and filters by action, actor, target, and date range.
- Admin invitation UI supports invitation list/create, duplicate pending invite rejection, existing-user rejection, and email-delivered single-use acceptance links.
- Invitation acceptance creates a local Identity user, confirms the invited email, marks the invitation accepted, and signs in the user.
- AuthNet packages do not seed a default admin username or password; host applications own first-admin bootstrap.
- ASP.NET Core Identity user/context in `AuthNet.Persistence.Postgres`.
- Persisted account invitations in `AuthNet.Persistence.Postgres`.
- Persisted admin audit events in `AuthNet.Persistence.Postgres`.
- Initial PostgreSQL Identity migration exists in `src/AuthNet.Persistence.Postgres/Migrations`.
- Generic OpenID Connect extension exists in `AuthNet.ExternalProviders`.
- External login signs in already linked accounts, lets authenticated users link from profile, and no longer links existing local accounts by email alone.
- Sample host wires `AddAuthNet`, `UseAuthentication`, `UseAuthorization`, and `MapAuthNet`.
- Sample host home page, shared navigation, and protected `/Admin` page link to the built-in admin user list, direct user creation, role management, invitation pages, and SPA smoke page.
- Sample host home page, shared navigation, and protected `/Admin` page link to the built-in admin audit page.
- `UseAuthNet()` remains as a compatibility wrapper.
- AuthNet UI ships fallback shared `_Layout.cshtml`, `_ValidationScriptsPartial.cshtml`, and `_AuthNetBrand.cshtml` so built-in pages render in a bare host.
- `AuthNet.Tests` has an in-memory integration test host covering routes, registration, confirm/resend email, forgot password, profile update, verified email change, external-login safety, endpoint mapping compatibility, and admin user management.
- `AuthNet.Tests` covers authenticator-app MFA setup, MFA login challenge, recovery-code login, and disable flows.
- `AuthNet.Tests` covers invitation creation, email delivery, acceptance, expired invitations, reused invitations, invalid tokens, duplicate pending invitations, existing-user rejection, and route protection.
- MVP packable packages are `AuthNet.Core`, `AuthNet.AspNetCore`, `AuthNet.UI.Razor`, `AuthNet.Persistence.Postgres`, `AuthNet.ExternalProviders`, and `AuthNet.Api`.
- Package metadata is centralized in `Directory.Build.props`; local packages output to ignored `artifacts/packages`.
- Sample host supports Development-only EF Core InMemory via `AuthNet:UseInMemoryDatabase=true`; PostgreSQL remains the default production/package persistence path.
- Sample host supports explicit admin bootstrap in any environment through `AuthNet:AdminBootstrap:{Enabled,UserName,Email,Password}`.
- Sample host supports a sample SMTP email sender through `AuthNet:Email:Smtp` when `AuthNet:UseDevelopmentEmailSender=false`; development email remains the default local sender.
- `samples/AuthNet.SampleHost/appsettings.SmtpSample.json` shows SMTP settings without committed secrets.
- Local verification is centralized in `scripts/verify.ps1`.
- GitHub Actions verify-only CI exists at `.github/workflows/ci.yml`; it does not publish or upload packages.

## Current Verification

Known passing commands:

```powershell
.\scripts\verify.ps1
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
```

Latest full test count: 126 passing tests.

Slice 14 focused SPA API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetSpaApiTests
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
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostAdminBootstrapTests
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
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Api\AuthNet.Api.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj --configuration Release --no-build --output .\artifacts\packages
```

Known passing package-consumer smoke:

```powershell
.\.dotnet\dotnet.exe build artifacts\package-smoke\AuthNet.PackageSmoke.csproj --no-restore
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
- PostgreSQL/EF Core is the only persistence path for now.
- PostgreSQL/EF Core is the production/default persistence path.
- Integration tests and sample-host Development mode can use EF Core InMemory; this is not a production persistence provider.
- Admin UI uses `Administrator` as a superuser role and a bounded AuthNet built-in UI permission catalog backed by Identity role claims.
- Do not add host-defined custom permission catalogs, tenant-scoped permissions, role deletion, or API/JWT permission flows unless explicitly re-scoped.
- AuthNet packages must not ship hardcoded default admin credentials; sample-host admin bootstrap requires explicit config.
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

Publication decisions are intentionally paused for now.

Recommended next product slice:

- Add the next SPA workflow endpoints: password reset completion, email confirmation completion, profile update, change password, and MFA JSON flows.

Other candidates:

- Add a committed package-consumer sample if local smoke coverage should be permanent.

Before starting any next feature, check whether it belongs to MVP slice 1 or deferred scope.
