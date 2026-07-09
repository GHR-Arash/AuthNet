# AuthNet Context

## Current Iteration

Slice 22 built-in UI polish is implemented and tracked in:

- `tasks/slice-22-plan.md`
- `tasks/slice-22-todo.md`

Slice 21 package publication finalization is implemented and tracked in:

- `tasks/slice-21-plan.md`
- `tasks/slice-21-todo.md`
- `docs/slice-21/package-publication-finalization.md`

Slice 20 committed package-consumer sample is implemented and tracked in:

- `tasks/slice-20-plan.md`
- `tasks/slice-20-todo.md`

Slice 19 SPA invitation acceptance JSON is implemented and tracked in:

- `tasks/slice-19-plan.md`
- `tasks/slice-19-todo.md`

Slice 18 SPA external-login JSON orchestration is implemented and tracked in:

- `tasks/slice-18-plan.md`
- `tasks/slice-18-todo.md`

Slice 17 SPA MFA JSON workflows are implemented and tracked in:

- `tasks/slice-17-plan.md`
- `tasks/slice-17-todo.md`

Slice 16 SPA account workflow completion is implemented and tracked in:

- `tasks/slice-16-plan.md`
- `tasks/slice-16-todo.md`

Slice 15 OpenAPI document endpoint is implemented and tracked in:

- `tasks/slice-15-plan.md`
- `tasks/slice-15-todo.md`

Slice 14 SPA workflow foundation is implemented and tracked in:

- `tasks/slice-14-plan.md`
- `tasks/slice-14-todo.md`

Slice 13 role management and permission enhancement is implemented and tracked in:

- `tasks/slice-13-plan.md`
- `tasks/slice-13-todo.md`

Slice 12 real email sender sample is implemented and tracked in:

- `tasks/slice-12-plan.md`
- `tasks/slice-12-todo.md`

Slice 11 admin audit events are implemented and tracked in:

- `tasks/slice-11-plan.md`
- `tasks/slice-11-todo.md`

Slice 10 admin direct user creation is implemented and tracked in:

- `tasks/slice-10-plan.md`
- `tasks/slice-10-todo.md`

Slice 09 account invitation flow is implemented and tracked in:

- `tasks/slice-09-plan.md`
- `tasks/slice-09-todo.md`
- `docs/slice-09/account-invitations.md`

Slice 08 MFA foundation is implemented and tracked in:

- `tasks/slice-08-plan.md`
- `tasks/slice-08-todo.md`
- `docs/slice-08/mfa-foundation.md`

## Current Package Shape

Packable:

- `AuthNet.Core`
- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Persistence.Postgres`
- `AuthNet.ExternalProviders`
- `AuthNet.Api`

Not packable:

- `AuthNet.SampleHost`
- `AuthNet.PackageConsumer`
- `AuthNet.Tests`

JWT and refresh-token authentication remain deferred.

## Current Verification

Latest focused UI polish verification:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetRouteTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetLoginTests
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetAdminUserTests
```

Manual sample-host HTTP check in Development verified `/auth` returns 200 and `_content/AuthNet.UI.Razor/authnet.css` returns 200.

Latest known package verification uses Release build plus per-project pack commands into ignored `artifacts/packages`.

Latest committed package-consumer sample is under `samples/AuthNet.PackageConsumer`, references `AuthNet.AspNetCore` `0.1.0` from local package artifacts, and is intentionally outside `AuthNet.slnx`.

Package verification shares `scripts/package-manifest.ps1` across output, metadata, and package-consumer checks.

Focused SPA API verification:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetSpaApiTests
```

Focused OpenAPI verification:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests
```

Focused SPA account workflow verification:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests
```

Focused SPA MFA API verification:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaMfaApiTests
```

Focused SPA external-login API verification:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaExternalLoginApiTests
```

Focused SPA invitation API verification:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaInvitationApiTests
```

Focused package-consumer verification:

```powershell
.\scripts\verify-package-consumer.ps1
```

Focused package metadata verification:

```powershell
.\scripts\verify-package-metadata.ps1
```

Focused sample email sender verification:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostEmailSenderTests
```

## Current Persistence Modes

PostgreSQL remains the default production/package persistence path.

Development-only InMemory is implemented for the sample host through `AuthNet:UseInMemoryDatabase=true` in `appsettings.Development.json`.

Slice 04 files:

- `tasks/slice-04-plan.md`
- `tasks/slice-04-todo.md`
- `docs/slice-04/development-inmemory.md`

## Current Verification

Canonical local verification:

```powershell
.\scripts\verify.ps1
```

Latest full verification: 160 passing tests.

Package-consumer verification is integrated into `.\scripts\verify.ps1` after package packing.

Package metadata verification is integrated into `.\scripts\verify.ps1` after package output verification.

Verify-only CI exists at `.github/workflows/ci.yml` for pushes and pull requests to `master`.

CI does not publish packages, upload package artifacts, or require secrets.

## Publication Blockers

Before public package publication, confirm:

- Public repository URL.
- License expression or license file.
- Final package owners/authors.
- XML documentation policy.
- Package signing policy.
- CI tag-publish or trusted-publishing strategy.

Publication work is paused for now.

## Current Admin UI

Built-in fallback UI includes a package-owned home page at `/auth` by default, plus a navigation shell and package-owned CSS for account, MFA, users, roles, invitations, and audit workflows. Hosts can still replace the fallback shell with `AuthNetOptions.LayoutPath`.

Admin user management UI is available under the configured AuthNet route prefix:

- `/auth/admin/users`
- `/auth/admin/users/{id}`

The UI requires the ASP.NET Core Identity `Administrator` role. AuthNet packages do not seed a default admin username or password.

The sample host has explicit admin bootstrap in any environment through:

- `AuthNet:AdminBootstrap:Enabled`
- `AuthNet:AdminBootstrap:UserName`
- `AuthNet:AdminBootstrap:Email`
- `AuthNet:AdminBootstrap:Password`

The sample host exposes admin workflow links from the home page, shared navigation, and protected `/Admin` page:

- `/auth/admin/users`
- `/auth/admin/users/new`
- `/auth/admin/audit`
- `/auth/admin/invitations`
- `/auth/admin/invitations/new`

Implemented actions:

- List/search users.
- Directly create local users.
- View user detail.
- Confirm/unconfirm email.
- Lock/unlock user.
- Reset access failed count.
- Grant/remove the fixed `Administrator` role.
- Prevent removing the last administrator.
- Review persisted audit events for successful admin mutations.

## Current Audit Scope

Implemented Slice 11 scope:

- Persisted audit events in `AuthNet.Persistence.Postgres`.
- Admin-only audit list page at `/auth/admin/audit`.
- Filters by action, actor, target, and date range.
- Successful admin mutation coverage for direct user creation, invitation creation, fixed administrator grant/remove, email confirm/unconfirm, lock/unlock, and access failure reset.
- Audit metadata excludes passwords, raw invitation tokens, and invitation acceptance URLs.

Out of scope:

- Audit export.
- Audit retention policy.
- Tamper-proof audit signing.
- SIEM/webhook integration.
- Failed admin attempts.

Slice 06 files:

- `tasks/slice-06-plan.md`
- `tasks/slice-06-todo.md`
- `docs/slice-06/admin-user-management.md`

Slice 07 files:

- `tasks/slice-07-plan.md`
- `tasks/slice-07-todo.md`
- `docs/slice-07/admin-role-assignment.md`

## Current MFA Scope

Implemented Slice 08 scope:

- Authenticator-app TOTP setup and verification using ASP.NET Core Identity.
- MFA login challenge for local password sign-in.
- Recovery-code display and recovery-code login.
- User-owned MFA disable flow.
- Same-origin SPA JSON endpoints for MFA status, setup start/verify, MFA disable, recovery-code count/regeneration, MFA challenge sign-in, and recovery-code sign-in.
- No SMS/email OTP, passkeys, API/JWT, admin MFA reset, or global required-MFA policy.

## Current Login Scope

- Local password sign-in accepts either email address or username.
- Sample-host admin bootstrap can create `UserName=admin` and `Email=admin@admin.com`; the user can sign in with username `admin`.

## Current Invitation Scope

Implemented Slice 09 scope:

- Admin-only invitation list and create pages.
- Anonymous invitation acceptance page with secure token.
- Persisted invitation records in `AuthNet.Persistence.Postgres`.
- Invitation token hashes only; raw tokens are sent in email links and are not stored.
- Invitation email delivery through `IAuthNetEmailSender`.
- Identity user creation with invited email confirmed after successful acceptance.
- Single-use and expiration handling.
- Same-origin SPA JSON endpoints for invitation token inspection and invitation acceptance.

Routes:

- `/auth/admin/invitations`
- `/auth/admin/invitations/new`
- `/auth/invitations/accept`
- `/auth/api/invitations/accept`

Out of scope:

- Bulk invitations.
- Role assignment during invite.
- Invitation resend/cancel.
- Organization/team membership.
- Admin invitation JSON APIs and API/JWT invitation endpoints.

## Current SPA Workflow Scope

Implemented Slice 14 through Slice 19 scope:

- `AuthNet.Api` package/project for JSON browser account endpoints.
- Same-origin SPA/BFF-style cookie workflow using the existing Identity application cookie.
- JSON routes under the configured account route prefix, `/auth/api` by default.
- Session, login, logout, registration, forgot-password, reset-password, resend-confirmation, confirm-email, profile read/update, change-password, MFA status/setup/disable, recovery-code count/regeneration, MFA challenge sign-in, recovery-code sign-in, external-provider discovery, external-login challenge/callback, signed-in external-login link challenge/callback, and invitation acceptance status/completion JSON endpoints.
- OpenAPI document endpoint at `/auth/api/openapi.json`, scoped to AuthNet SPA JSON endpoints.
- Sample SPA smoke page in the sample host at `/Spa` exercises session, login/logout, profile update, password change, reset completion, confirm-email completion, MFA workflows, external-login provider/challenge/link calls, invitation token inspection/acceptance calls, and OpenAPI discovery.
- Keep JWT access tokens, refresh tokens, admin JSON, cross-origin SPA auth, and provider-specific helper packages deferred to separate slices.

## Current Sample Email Scope

Implemented Slice 12 scope:

- Sample-host-only SMTP email sender implementing `IAuthNetEmailSender`.
- SMTP settings bind from `AuthNet:Email:Smtp`.
- Development sample path keeps `UseDevelopmentEmailSender=true`.
- Production-like sample path sets `UseDevelopmentEmailSender=false` and requires valid SMTP settings.
- Sample SMTP configuration is documented in `samples/AuthNet.SampleHost/appsettings.SmtpSample.json`.
- SMTP passwords are not committed; docs use environment variables for secret values.

Out of scope:

- Provider-specific email packages.
- Background queueing, retries, or delivery audit.
- Changing AuthNet package email abstractions.

Likely product slice after Slice 12:

- Execute Slice 13 role management and permission enhancement.

## Current Role and Permission Scope

Implemented Slice 13 scope:

- Role list/create/detail admin pages at `/auth/admin/roles`, `/auth/admin/roles/new`, and `/auth/admin/roles/{id}`.
- Arbitrary user role assignment on user detail while preserving last-administrator protection.
- Bounded AuthNet permission catalog for built-in UI operations.
- Permissions stored as ASP.NET Core Identity role claims with claim type `authnet.permission`.
- `Administrator` remains the superuser role and satisfies every AuthNet permission policy.
- Audit events for role creation, role assignment/removal, and role permission assignment/removal.
- Sample host home, navbar, and `/Admin` page link to role management.

Out of scope:

- Custom role or permission store.
- Role deletion.
- Tenant-scoped authorization.
- Host-defined custom permission catalog.
- API/JWT permission flows.
