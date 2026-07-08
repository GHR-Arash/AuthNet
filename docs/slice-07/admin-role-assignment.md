# Slice 07: Admin Role Assignment UI

## Intent

Extend the built-in admin user-management UI with narrowly scoped administrator role assignment.

## Implemented Slice

AuthNet now lets administrators manage the fixed ASP.NET Core Identity `Administrator` role from the user detail page:

- `/auth/admin/users/{id}`

All role assignment actions require a signed-in user in the `Administrator` role.

## Capabilities

On the user detail page, administrators can:

- See whether the viewed user has administrator access.
- Grant the fixed `Administrator` role.
- Remove the fixed `Administrator` role.
- Keep the existing role list visible.

Removal is blocked when the target user is the last remaining administrator.

## Non-Goals

- No arbitrary role management.
- No configurable admin role name.
- No fine-grained permission model.
- No user invitation flow.
- No user deletion.
- No impersonation.
- No audit event storage.
- No API endpoints.

## Security Model

Use ASP.NET Core Identity and authorization:

- `[Authorize(Roles = "Administrator")]`
- `UserManager<AuthNetUser>`
- `RoleManager<IdentityRole>`

The admin role name remains fixed as `Administrator`.

## Verification Target

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests
.\scripts\verify.ps1
```

## Follow-On Candidates

- Account invitation flow.
- Audit events for admin actions.
- Session and device management.
