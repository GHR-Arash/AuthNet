# AuthNet User Guide

This guide is for application developers who want to use AuthNet in an ASP.NET application.

AuthNet MVP slice 1 supports server-rendered ASP.NET applications using Razor Pages, cookie authentication, ASP.NET Core Identity, EF Core, PostgreSQL, email-based account flows, role authorization, and generic OpenID Connect login.

## What You Get

After AuthNet is registered in your app, it provides:

- Login and logout.
- Registration, disabled by default unless enabled.
- Email confirmation.
- Forgot password and reset password.
- Profile management.
- Change password.
- Cookie authentication.
- ASP.NET Core Identity roles.
- PostgreSQL-backed Identity persistence.
- Generic OpenID Connect external login.

Not included in this MVP:

- JWT/API authentication.
- Refresh tokens.
- SPA flows.
- Admin user management UI.
- MFA.
- Multi-tenancy.

## Required Packages

When AuthNet is packaged, a typical app will reference:

```text
AuthNet.AspNetCore
AuthNet.UI.Razor
AuthNet.Persistence.Postgres
AuthNet.ExternalProviders
```

While developing from this repository, reference the projects directly, as shown in `samples/AuthNet.SampleHost`.

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
app.UseAuthNet();

app.Run();
```

Order matters:

1. `UseRouting()`
2. `UseAuthentication()`
3. `UseAuthorization()`
4. `UseAuthNet()`

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

## Database Setup

AuthNet uses EF Core migrations in `AuthNet.Persistence.Postgres`.

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
- `/auth/access-denied`
- `/auth/external-login`

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

AuthNet does not introduce a custom permission model in MVP slice 1.

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
- Keep `UseDevelopmentEmailSender` set to `false`.
- Keep `EnablePublicRegistration` set to `false` unless public signup is intended.
- Review cookie, password, lockout, and email-confirmation settings before launch.
