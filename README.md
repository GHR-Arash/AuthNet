# AuthNet

AuthNet is a drop-in identity module for ASP.NET Core apps. Install one package and get a polished, server-rendered account experience backed by ASP.NET Core Identity, PostgreSQL, cookie authentication, MFA, invitations, roles, admin screens, audit events, and same-origin JSON endpoints for SPA shells.

It is built for teams who want real authentication and access management without spending the first month rebuilding login, password reset, MFA setup, admin user management, and audit screens from scratch.

## What You Get

- Built-in Razor Pages UI for sign in, sign out, registration, email confirmation, password reset, profile, password change, MFA, and invitation acceptance.
- Polished default AuthNet home page and navigation under `/auth`.
- ASP.NET Core Identity users, roles, claims, password policy, lockout, security stamps, token providers, and cookies.
- PostgreSQL persistence through EF Core and Npgsql.
- Authenticator-app MFA with recovery codes.
- Admin UI for users, roles, built-in AuthNet permissions, invitations, and audit events.
- Admin audit events for successful administrative mutations.
- Account invitations that work even when public registration is disabled.
- Generic OpenID Connect external login.
- Same-origin JSON account endpoints and `/auth/api/openapi.json` for SPA/BFF-style browser clients using the existing Identity cookie.
- MIT license and published NuGet packages.

AuthNet deliberately does not ship JWT/refresh-token flows, SMS OTP, passkeys, multi-tenancy, or custom Identity stores yet. The first release is focused on a clean, dependable Identity module for ASP.NET Core applications.

## Install

```powershell
dotnet add package AuthNet.AspNetCore
```

`AuthNet.AspNetCore` brings in the current package set:

- `AuthNet.Core`
- `AuthNet.Persistence.Postgres`
- `AuthNet.UI.Razor`
- `AuthNet.ExternalProviders`
- `AuthNet.Api`

## Development Quick Start

Use this setup for local development with a local PostgreSQL database and the development email sender. This is the best default because it exercises the same relational provider you will use in production.

### 1. Configure `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "AuthNet": "Host=localhost;Port=5432;Database=my_app_identity_dev;Username=postgres;Password=postgres"
  },
  "AuthNet": {
    "ApplicationName": "My App",
    "AccountRoutePrefix": "/auth",
    "EnablePublicRegistration": true,
    "UseDevelopmentEmailSender": true,
    "RequireConfirmedEmail": true,
    "ApplyMigrations": true
  }
}
```

`UseDevelopmentEmailSender` is for local development only. It lets you inspect generated confirmation, reset, and invitation messages without wiring an email provider.

### 2. Register AuthNet

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

Order matters: `UseRouting()`, then `UseAuthentication()`, then `UseAuthorization()`, then `MapAuthNet()`.

### Development InMemory Option

For quick throwaway smoke tests, you can use EF Core InMemory instead of PostgreSQL by passing AuthNet a custom DbContext registration. This is useful when you want to click through the UI without running a database server.

```csharp
using AuthNet.AspNetCore;
using Microsoft.EntityFrameworkCore;

builder.Services.AddRazorPages();

builder.Services.AddAuthNet(
    options =>
    {
        builder.Configuration.GetSection("AuthNet").Bind(options);
        options.UseDevelopmentEmailSender = true;
    },
    db => db.UseInMemoryDatabase("AuthNet.Dev"));
```

Install the EF InMemory provider in your app if you use this path:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

Do not use InMemory for production or for final persistence testing. It does not behave like PostgreSQL for relational constraints, migrations, transactions, or SQL translation.

### 3. Open the UI

With the default route prefix:

```text
/auth
/auth/login
/auth/profile
/auth/mfa
/auth/admin/users
/auth/admin/roles
/auth/admin/invitations
/auth/admin/audit
/auth/api/openapi.json
```

The admin routes require a signed-in user in the `Administrator` role or a role with the matching AuthNet permission.

### 4. Create Your First Admin

AuthNet packages do not create production default credentials. For development, add a small bootstrap in your app or use your own seed process.

Hardcoded local-only example:

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

var admin = await userManager.FindByEmailAsync("admin@example.test");
if (admin is null)
{
    admin = new AuthNetUser
    {
        UserName = "admin",
        Email = "admin@example.test",
        EmailConfirmed = true,
        LockoutEnabled = true
    };

    var create = await userManager.CreateAsync(admin, "ChangeMe1!");
    if (!create.Succeeded)
    {
        throw new InvalidOperationException(
            string.Join(" ", create.Errors.Select(error => error.Description)));
    }
}

if (!await userManager.IsInRoleAsync(admin, "Administrator"))
{
    await userManager.AddToRoleAsync(admin, "Administrator");
}
```

Use secrets or environment variables for real deployments. Do not hardcode production passwords.

Appsettings-driven example:

```json
{
  "AdminBootstrap": {
    "Enabled": true,
    "UserName": "admin",
    "Email": "admin@example.test",
    "Password": "ChangeMe1!"
  }
}
```

```csharp
if (app.Configuration.GetValue<bool>("AdminBootstrap:Enabled"))
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();

    const string adminRole = "Administrator";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    var email = app.Configuration["AdminBootstrap:Email"];
    var userName = app.Configuration["AdminBootstrap:UserName"];
    var password = app.Configuration["AdminBootstrap:Password"];

    if (string.IsNullOrWhiteSpace(email))
    {
        throw new InvalidOperationException("AdminBootstrap:Email is required.");
    }

    var admin = await userManager.FindByEmailAsync(email);
    if (admin is null)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("AdminBootstrap:Password is required when creating an admin.");
        }

        admin = new AuthNetUser
        {
            UserName = string.IsNullOrWhiteSpace(userName) ? email : userName,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var create = await userManager.CreateAsync(admin, password);
        if (!create.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(" ", create.Errors.Select(error => error.Description)));
        }
    }

    if (!await userManager.IsInRoleAsync(admin, adminRole))
    {
        await userManager.AddToRoleAsync(admin, adminRole);
    }
}
```

For production, keep `AdminBootstrap:Password` in a secret manager or environment variable, not committed JSON. A common production pattern is to create the user through a controlled operational path, then run an idempotent bootstrap that only assigns the `Administrator` role by email.

## Production Quick Start

Production should be explicit: real PostgreSQL, real email, HTTPS, migrations under your release process, and no development email sender.

### 1. Configure Production

```json
{
  "ConnectionStrings": {
    "AuthNet": "Host=prod-db;Port=5432;Database=my_app_identity;Username=my_app;Password=<from-secret-store>"
  },
  "AuthNet": {
    "ApplicationName": "My App",
    "AccountRoutePrefix": "/auth",
    "EnablePublicRegistration": false,
    "UseDevelopmentEmailSender": false,
    "RequireConfirmedEmail": true,
    "ApplyMigrations": false,
    "Invitations": {
      "Expiration": "7.00:00:00"
    }
  }
}
```

Keep connection strings, email credentials, and admin bootstrap secrets in your deployment secret manager.

### 2. Register a Real Email Sender

```csharp
using AuthNet.Core.Email;

public sealed class MyEmailSender : IAuthNetEmailSender
{
    public Task SendAsync(AuthNetEmailMessage message, CancellationToken cancellationToken = default)
    {
        // Send message.To, message.Subject, and message.HtmlBody through your provider.
        return Task.CompletedTask;
    }
}
```

Register it:

```csharp
builder.Services.AddSingleton<IAuthNetEmailSender, MyEmailSender>();
```

Production validation rejects the development email sender.

### 3. Apply the Database Schema

Apply migrations during deployment, before serving traffic:

```powershell
dotnet tool install dotnet-ef --version 10.0.9 --tool-path .tools
.\.tools\dotnet-ef.exe database update `
  --project src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj `
  --startup-project path\to\your-app.csproj `
  --context AuthNetDbContext
```

You can also run migrations from your own deployment job. Avoid surprise production schema changes from normal web startup unless that is already your standard operating model.

### 4. Bootstrap an Administrator

Promote an existing verified user or run a one-time bootstrap with secrets supplied by your deployment environment. Remove or disable the bootstrap after the first admin exists unless you intentionally keep it idempotent.

AuthNet uses the standard ASP.NET Core Identity role name:

```text
Administrator
```

Once signed in as an administrator, use `/auth/admin/users`, `/auth/admin/roles`, `/auth/admin/invitations`, and `/auth/admin/audit` to manage the built-in identity surface.

## SPA / BFF-Friendly JSON Endpoints

AuthNet also maps same-origin JSON endpoints under `/auth/api` by default. They use the same Identity application cookie as the Razor UI:

- session
- login/logout
- registration
- forgot/reset password
- email confirmation
- profile read/update
- password change
- MFA setup/challenge/recovery code workflows
- external login discovery/challenge/callback/linking
- invitation acceptance
- OpenAPI document at `/auth/api/openapi.json`

These endpoints are intended for same-origin browser apps. JWT and refresh tokens are future scope.

## Customize the UI

The built-in UI works out of the box, including a fallback layout, AuthNet home page, navigation, and package-owned CSS. You can configure:

```json
{
  "AuthNet": {
    "ApplicationName": "My App",
    "AccountRoutePrefix": "/auth",
    "LayoutPath": "_Layout",
    "BrandLogoUrl": "/images/logo.svg"
  }
}
```

Use `LayoutPath` when you want AuthNet pages inside your application shell.

## Run the Repository Sample

The repository sample host creates a demo administrator in code at startup:

```text
Username: admin
Email: admin@admin.com
Password: Password1!
```

Run it:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Then open:

```text
http://127.0.0.1:5127/auth
```

The sample demo credentials are sample-host behavior only. They are not part of the AuthNet package behavior.

## Documentation

- [Use AuthNet in Your App](docs/users/getting-started.md)
- [Configuration Reference](docs/users/configuration.md)
- [Account Pages and Routes](docs/users/account-pages.md)
- [Developer Quick Start](docs/developer/quick-start.md)
- [NuGet Publishing Guide](docs/developer/nuget-publishing.md)
- [Architecture Context](docs/architecture-context.md)

## License

AuthNet is released under the [MIT License](LICENSE).
