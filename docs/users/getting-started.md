# AuthNet User Guide

This guide is for application developers who want to use AuthNet in an ASP.NET application.

AuthNet MVP slice 1 supports server-rendered ASP.NET applications and same-origin SPA shells using Razor Pages, JSON account endpoints, cookie authentication, ASP.NET Core Identity, EF Core, PostgreSQL, email-based account flows, role authorization, and generic OpenID Connect login.

## What You Get

After AuthNet is registered in your app, it provides:

- Login and logout.
- Registration, disabled by default unless enabled.
- Email confirmation.
- Forgot password and reset password.
- Profile management.
- Change password.
- Authenticator-app MFA with recovery codes.
- Admin user management UI protected by `Administrator` or AuthNet permissions.
- Role creation, role assignment, and built-in AuthNet permission assignment.
- Account invitation flow.
- Same-origin SPA JSON account endpoints.
- Cookie authentication.
- ASP.NET Core Identity roles.
- PostgreSQL-backed Identity persistence.
- Generic OpenID Connect external login.

Not included in this MVP:

- JWT/API authentication.
- Refresh tokens.
- Cross-origin SPA token flows.
- Role deletion, custom permission catalogs, invitation resend/cancel, and bulk invitation workflows.
- SMS/email OTP, passkeys, and global required-MFA policy.
- Multi-tenancy.

## Required Packages

For package-based consumption, reference the primary integration package:

```powershell
dotnet add package AuthNet.AspNetCore
```

`AuthNet.AspNetCore` depends on the current MVP support packages:

```text
AuthNet.UI.Razor
AuthNet.Persistence.Postgres
AuthNet.ExternalProviders
AuthNet.Api
```

If you are consuming local packages produced from this repository, add the local package source:

```powershell
dotnet add package AuthNet.AspNetCore --version 0.1.0 --source C:\Projects\AuthNet\artifacts\packages
```

While developing AuthNet itself from this repository, reference the projects directly, as shown in `samples/AuthNet.SampleHost`.

## Minimal Setup

In `Program.cs`:

```csharp
using AuthNet.AspNetCore;
using AuthNet.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthNet(options =>
{
    builder.Configuration.GetSection("AuthNet").Bind(options);
    options.PostgresConnectionString = builder.Configuration.GetConnectionString("AuthNet");
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue<bool>("AuthNet:ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
    db.Database.Migrate();
}

app.MapStaticAssets();
app.MapAuthNet();

app.Run();
```

Order matters:

1. `UseRouting()`
2. `UseAuthentication()`
3. `UseAuthorization()`
4. `MapAuthNet()`

`UseAuthNet()` remains as a compatibility wrapper, but new integrations should use `MapAuthNet()`.

## Minimal Configuration

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AuthNet": "Host=localhost;Port=5432;Database=my_app_identity;Username=postgres;Password=postgres"
  },
  "AuthNet": {
    "ApplicationName": "My App",
    "AccountRoutePrefix": "/auth",
    "EnablePublicRegistration": false,
    "UseDevelopmentEmailSender": true,
    "RequireConfirmedEmail": true,
    "ApplyMigrations": false,
    "OpenIdConnect": {
      "Enabled": false,
      "DisplayName": "OpenID Connect",
      "Authority": "",
      "ClientId": "",
      "ClientSecret": "",
      "CallbackPath": "/signin-authnet-oidc"
    }
  }
}
```

For development, `UseDevelopmentEmailSender` can be `true`.

For production, register a real `IAuthNetEmailSender` and set `UseDevelopmentEmailSender` to `false`.

The repository sample host includes a sample SMTP sender for production-like manual testing. Configure it with `AuthNet:Email:Smtp` or environment variables such as `AuthNet__Email__Smtp__Password`; do not commit SMTP passwords to JSON.

## Database Setup

AuthNet uses EF Core migrations in `AuthNet.Persistence.Postgres`.

PostgreSQL is the production/default persistence path. The repository sample host has a Development-only EF Core InMemory mode for local smoke testing, but that mode is not a production persistence provider and does not replace PostgreSQL migration or relational behavior testing.

Apply the schema with EF tooling:

```powershell
.\.tools\dotnet-ef.exe database update --project src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --startup-project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --context AuthNetDbContext
```

In your own application, use your app as the startup project.

Alternatively, for development only, set:

```json
"AuthNet": {
  "ApplyMigrations": true
}
```

## Default Account Routes

With `AccountRoutePrefix` set to `/auth`, AuthNet maps:

- `/auth/login`
- `/auth/logout`
- `/auth/register`
- `/auth/forgot-password`
- `/auth/reset-password`
- `/auth/confirm-email`
- `/auth/resend-confirmation`
- `/auth/profile`
- `/auth/change-password`
- `/auth/mfa`
- `/auth/mfa/setup`
- `/auth/mfa/recovery-codes`
- `/auth/mfa/disable`
- `/auth/login/mfa`
- `/auth/login/recovery-code`
- `/auth/invitations/accept`
- `/auth/access-denied`
- `/auth/external-login`

Admin routes are mapped under the same prefix:

- `/auth/admin/users`
- `/auth/admin/users/new`
- `/auth/admin/users/{id}`
- `/auth/admin/roles`
- `/auth/admin/roles/new`
- `/auth/admin/roles/{id}`
- `/auth/admin/audit`
- `/auth/admin/invitations`
- `/auth/admin/invitations/new`

These routes require a signed-in user in the `Administrator` role or a role with the matching AuthNet permission. AuthNet packages do not create a default admin user or default development password.

The login page accepts either the user's email address or username.

SPA JSON routes are mapped under `{AccountRoutePrefix}/api`. With the default prefix, AuthNet maps:

- `GET /auth/api/session`
- `GET /auth/api/profile`
- `POST /auth/api/login`
- `POST /auth/api/logout`
- `POST /auth/api/register`
- `POST /auth/api/forgot-password`
- `POST /auth/api/resend-confirmation`

These endpoints are intended for same-origin browser clients using the existing Identity application cookie. JWT access tokens, refresh tokens, cross-origin CORS policy management, MFA JSON endpoints, and admin JSON APIs are still deferred.

## Protecting Pages or Endpoints

Use standard ASP.NET Core authorization.

Require authentication:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public sealed class ProfileModel : PageModel;
```

Require a role:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Roles = "Administrator")]
public sealed class AdminModel : PageModel;
```

AuthNet also registers built-in permission policies for its admin UI. Permissions are stored as ASP.NET Core Identity role claims with claim type `authnet.permission`, and `Administrator` satisfies every AuthNet permission.

## Admin User Management

The built-in admin UI uses the standard `Administrator` role as the superuser role:

```csharp
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;

using var scope = app.Services.CreateScope();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();

if (!await roleManager.RoleExistsAsync("Administrator"))
{
    await roleManager.CreateAsync(new IdentityRole("Administrator"));
}

var user = await userManager.FindByEmailAsync("admin@example.com");
if (user is not null)
{
    await userManager.AddToRoleAsync(user, "Administrator");
}
```

Use your own bootstrap policy for creating the first administrator. Do not ship a hardcoded admin password.

After the first administrator exists, administrators can create roles, assign built-in AuthNet permissions to roles, and assign roles to users. AuthNet prevents removing the last remaining administrator.

Built-in AuthNet permissions:

- `authnet.users.view`
- `authnet.users.manage`
- `authnet.roles.view`
- `authnet.roles.manage`
- `authnet.invitations.manage`
- `authnet.audit.view`

Permission changes apply through the normal ASP.NET Core Identity claims principal. A signed-in user may need to sign out and sign in again before newly assigned role permissions are reflected in their current session.

Administrators can also directly create local users at `/auth/admin/users/new`. The form creates the account through ASP.NET Core Identity with username, email, optional display name, password, email confirmation state, and optional fixed administrator access. Use invitations when the user should set their own password.

Administrators can review successful admin mutation events at `/auth/admin/audit`. Audit coverage includes direct user creation, invitation creation, role creation, role assignment/removal, role permission assignment/removal, administrator role grant/remove, email confirm/unconfirm, lock/unlock, and access failure reset.

The repository sample host includes an explicit admin bootstrap that uses the same configuration in Development and Production:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:AuthNet__AdminBootstrap__Enabled='true'
$env:AuthNet__AdminBootstrap__UserName='admin'
$env:AuthNet__AdminBootstrap__Email='admin@example.test'
$env:AuthNet__AdminBootstrap__Password='Password1!'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

With that configuration, sign in at `/auth/login` using username `admin` and password `Password1!`.

In the repository sample host, the home page, navbar, and protected `/Admin` page link to the user list, direct user creation, role management, audit, and invitation pages so the admin workflows are discoverable after sign-in.

This sample-host bootstrap is not package behavior and does not provide default credentials.

## Account Invitations

Administrators can onboard users while public registration remains disabled.

Use the admin invitation pages:

```text
/auth/admin/invitations
/auth/admin/invitations/new
```

AuthNet sends an invitation link through `IAuthNetEmailSender`. The invited user opens the link, sets a username, optional display name, and password, then AuthNet creates the local account with the invited email confirmed.

Invitation tokens are single-use, expire after `AuthNet:Invitations:Expiration`, and are stored only as hashes.

## External Login

Enable generic OpenID Connect in configuration:

```json
"AuthNet": {
  "OpenIdConnect": {
    "Enabled": true,
    "DisplayName": "Company SSO",
    "Authority": "https://identity.example.com",
    "ClientId": "my-client-id",
    "ClientSecret": "my-client-secret",
    "CallbackPath": "/signin-authnet-oidc"
  }
}
```

When enabled, the login page shows the configured external provider.

For security, automatic external account provisioning requires the provider to return a verified email claim. Existing local accounts are linked from the signed-in user's profile page, not by matching an unauthenticated external email claim.

## Production Notes

- Use HTTPS.
- Use a real PostgreSQL connection string.
- Register a production `IAuthNetEmailSender`.
- For the repository sample host, configure `AuthNet:Email:Smtp` when `UseDevelopmentEmailSender=false`.
- Keep `UseDevelopmentEmailSender` set to `false`.
- Keep `EnablePublicRegistration` set to `false` unless public signup is intended.
- Review cookie, password, lockout, and email-confirmation settings before launch.
- `AuthNet.Api` uses the existing application cookie for same-origin SPA workflows. JWT and refresh-token authentication remain future scope.
