# AuthNet Configuration Reference

This page describes the current AuthNet MVP configuration surface.

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
  "ApplyMigrations": false
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
