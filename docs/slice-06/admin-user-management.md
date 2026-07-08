# Slice 06: Admin User Management UI

## Intent

Implement the first Should Have roadmap item: built-in admin user management UI.

## Implemented Slice

AuthNet now includes a minimal admin user-management surface under the existing AuthNet route prefix:

- `/auth/admin/users`
- `/auth/admin/users/{id}`

All routes require the standard ASP.NET Core `Administrator` role.

AuthNet does not seed a default administrator account, username, or password. Host applications own first-admin bootstrap.

## Initial Capabilities

List/search users:

- Email.
- Display name.
- Email confirmed state.
- Lockout state.

View one user:

- Email and username.
- Display name.
- Phone number.
- Email confirmed state.
- Lockout state.
- Access failed count.
- External login count.
- Roles.

Admin actions:

- Confirm email.
- Unconfirm email.
- Lock user.
- Unlock user.
- Reset access failed count.

## Non-Goals

- No user deletion.
- No impersonation.
- No invitation flow.
- No audit event storage.
- No role assignment UI in this first admin slice.
- No API endpoints.
- No fine-grained permission model.

## Security Model

Use ASP.NET Core Identity and authorization:

- `[Authorize(Roles = "Administrator")]`
- `UserManager<AuthNetUser>`
- `RoleManager<IdentityRole>`

The admin role name is fixed as `Administrator` for this slice.

## Verification Target

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests
.\scripts\verify.ps1
```

## Follow-On Candidates

- Account invitation flow.
- Audit events for admin actions.
- Session and device management.
