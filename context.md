# AuthNet Context

## Current Iteration

Slice 06 admin user management UI is implemented locally.

## Current Package Shape

Packable:

- `AuthNet.Core`
- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Persistence.Postgres`
- `AuthNet.ExternalProviders`

Not packable:

- `AuthNet.SampleHost`
- `AuthNet.Tests`

Deferred:

- `AuthNet.Api`

## Current Verification

Latest known package verification uses Release build plus per-project pack commands into ignored `artifacts/packages`.

Latest package-consumer smoke app is ignored at `artifacts/package-smoke` and compiles against `AuthNet.AspNetCore` `0.1.0` from the local package source.

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

Verify-only CI exists at `.github/workflows/ci.yml` for pushes and pull requests to `master`.

CI does not publish packages, upload package artifacts, or require secrets.

## Publication Blockers

Before public package publication, confirm:

- Public repository URL.
- License expression or license file.
- Final package owners/authors.
- XML documentation policy.

Publication work is paused for now.

## Current Admin UI

Admin user management UI is available under the configured AuthNet route prefix:

- `/auth/admin/users`
- `/auth/admin/users/{id}`

The UI requires the ASP.NET Core Identity `Administrator` role. AuthNet packages do not seed a default admin username or password.

The sample host has explicit Development-only admin bootstrap through:

- `AuthNet:DevelopmentAdmin:Enabled`
- `AuthNet:DevelopmentAdmin:Email`
- `AuthNet:DevelopmentAdmin:Password`

Implemented actions:

- List/search users.
- View user detail.
- Confirm/unconfirm email.
- Lock/unlock user.
- Reset access failed count.

Slice 06 files:

- `tasks/slice-06-plan.md`
- `tasks/slice-06-todo.md`
- `docs/slice-06/admin-user-management.md`

Likely next product slice:

- Admin role assignment UI.
- Or account invitation flow.
