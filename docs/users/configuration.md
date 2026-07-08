# AuthNet Configuration Reference

This page describes the current AuthNet MVP configuration surface.

Package consumers should reference `AuthNet.AspNetCore`; the Razor UI, PostgreSQL persistence, external provider, and core packages are resolved through package dependencies in the current MVP package set.

PostgreSQL is the default and production persistence path. The sample host also has a Development-only `AuthNet:UseInMemoryDatabase` convenience setting, but that setting is sample-host behavior rather than a supported production persistence provider.

## Connection String

```json
"ConnectionStrings": {
  "AuthNet": "Host=localhost;Port=5432;Database=my_app_identity;Username=postgres;Password=postgres"
}
```

`PostgresConnectionString` is required for MVP slice 1.

## AuthNet Options

```json
"AuthNet": {
  "ApplicationName": "My App",
  "AccountRoutePrefix": "/auth",
  "LayoutPath": "_Layout",
  "BrandLogoUrl": "/images/logo.svg",
  "EnablePublicRegistration": false,
  "UseDevelopmentEmailSender": false,
  "RequireConfirmedEmail": true,
  "ApplyMigrations": false,
  "Invitations": {
    "Expiration": "7.00:00:00"
  }
}
```

### `ApplicationName`

Display name used by the built-in account UI.

Default:

```text
AuthNet
```

### `AccountRoutePrefix`

Route prefix for built-in account pages.

Default:

```text
/auth
```

### `LayoutPath`

Razor layout used by the built-in account UI.

Default:

```text
_Layout
```

### `BrandLogoUrl`

Optional logo URL rendered by the built-in account UI.

### `EnablePublicRegistration`

Controls whether users can create accounts through the registration page.

Default:

```text
false
```

This is intentionally closed by default.

### `UseDevelopmentEmailSender`

Registers the development email sender.

Default:

```text
false
```

Only use this in development. Production validation rejects the development sender.

### `RequireConfirmedEmail`

Requires users to confirm email before sign-in.

Default:

```text
true
```

### `ApplyMigrations`

Sample-host convenience setting for applying EF migrations at startup.

Default:

```text
false
```

Prefer explicit migration deployment outside local development.

When the sample host runs with Development-only InMemory mode, migrations are skipped because EF Core InMemory does not use relational migrations.

### `Invitations:Expiration`

Controls how long account invitation links remain valid.

Default:

```text
7.00:00:00
```

Invitation tokens are single-use and stored only as hashes.

## Password Options

```json
"AuthNet": {
  "Password": {
    "RequiredLength": 8,
    "RequireDigit": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireNonAlphanumeric": false
  }
}
```

These map to ASP.NET Core Identity password options.

## Lockout Options

```json
"AuthNet": {
  "Lockout": {
    "MaxFailedAccessAttempts": 5,
    "DefaultLockoutTimeSpan": "00:15:00"
  }
}
```

These map to ASP.NET Core Identity lockout options.

## Cookie Options

```json
"AuthNet": {
  "Cookie": {
    "ExpireTimeSpan": "08:00:00",
    "SlidingExpiration": true
  }
}
```

AuthNet configures the application cookie paths based on `AccountRoutePrefix`.

## OpenID Connect Options

```json
"AuthNet": {
  "OpenIdConnect": {
    "Enabled": true,
    "Scheme": "AuthNetOidc",
    "DisplayName": "OpenID Connect",
    "Authority": "https://identity.example.com",
    "ClientId": "client-id",
    "ClientSecret": "client-secret",
    "CallbackPath": "/signin-authnet-oidc"
  }
}
```

Required when enabled:

- `Authority`
- `ClientId`

Optional:

- `ClientSecret`
- `Scheme`
- `DisplayName`
- `CallbackPath`

## Email Sender

Production applications must register an implementation of:

```csharp
using AuthNet.Core.Email;

public sealed class MyEmailSender : IAuthNetEmailSender
{
    public Task SendAsync(AuthNetEmailMessage message, CancellationToken cancellationToken = default)
    {
        // Send through your email provider.
        return Task.CompletedTask;
    }
}
```

Register it before or after `AddAuthNet`:

```csharp
builder.Services.AddSingleton<IAuthNetEmailSender, MyEmailSender>();
```

Development can use:

```json
"UseDevelopmentEmailSender": true
```

The repository sample host also includes a sample SMTP implementation under `AuthNet:Email:Smtp` for production-like manual testing:

```json
"AuthNet": {
  "UseDevelopmentEmailSender": false,
  "Email": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.example.com",
      "Port": 587,
      "UserName": "",
      "Password": "",
      "FromEmail": "no-reply@example.com",
      "FromName": "AuthNet Sample",
      "EnableSsl": true
    }
  }
}
```

Keep SMTP passwords out of committed JSON. In local testing, prefer environment variables such as `AuthNet__Email__Smtp__Password`.

This SMTP sender is sample-host behavior. Package consumers still own their production `IAuthNetEmailSender` implementation.
