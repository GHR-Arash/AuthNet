# Integration Requirements

## Integration Goal

AuthNet must be easy to add to a .NET 10 ASP.NET application while allowing the host application to configure account UI, cookie authentication, OpenID Connect login, email delivery, and PostgreSQL persistence.

## Supported Application Types

AuthNet shall support:

- ASP.NET Core MVC applications.
- ASP.NET Core Razor Pages applications.

API-only applications and SPA authentication are future scope.

## Middleware Integration

The host application shall be able to register AuthNet during application startup.

Conceptual usage:

```csharp
builder.Services.AddAuthNet(options =>
{
    options.EnablePublicRegistration = false;
});

app.UseAuthentication();
app.UseAuthorization();
app.MapAuthNet();
```

`UseAuthNet()` is a compatibility wrapper; the endpoint mapping API is `MapAuthNet()`.

## Authentication Modes

AuthNet shall support:

- Cookie-only mode.

Cookie mode is intended for MVC/Razor applications.

JWT and mixed modes are future scope.

## UI Integration

AuthNet shall provide Razor Pages account UI.

The host application shall be able to:

- Enable default account UI.
- Configure account route prefixes.
- Configure layout or branding extension points.

Full page override is future scope.

Required default UI routes:

- Login
- Register
- Logout
- Forgot password
- Reset password
- Verify email
- Profile
- Change password
- Access denied
- External login callback
- Resend email confirmation

## API Integration

API integration is future scope.

Expected future API groups:

- Registration
- Authentication
- Token refresh
- Logout
- Email verification
- Password recovery
- Profile
- Password change
- External login

The host application shall be able to configure the API route prefix.

Example route prefix:

```text
/auth
```

## External Provider Integration

AuthNet shall support provider-based configuration for:

- Generic OpenID Connect

Provider-specific Google and Microsoft helpers are future scope.

The host application shall provide provider credentials and callback settings.

AuthNet shall handle:

- Challenge initiation.
- Callback processing.
- User provisioning.
- Account linking.
- Duplicate identity detection.

## Persistence Integration

PostgreSQL is the default persistence provider for MVP slice 1.

AuthNet shall use ASP.NET Core Identity with EF Core and Npgsql.

Custom Identity stores and additional database providers are future scope.

Required persistence capabilities:

- User store.
- Role store.
- Permission/claim store.
- Credential store.
- Verification token store.
- Password reset token store.
- External login store.

## Configuration Requirements

The host application shall be able to configure:

- Authentication mode.
- Cookie expiration.
- Password policy.
- Registration policy.
- Email verification requirement.
- Lockout policy.
- External identity providers.
- Persistence provider.
- UI enablement.
- API endpoint prefix.

## Package Structure

Recommended package structure:

```text
AuthNet.Core
AuthNet.AspNetCore
AuthNet.UI.Razor
AuthNet.Api
AuthNet.Persistence.Postgres
AuthNet.ExternalProviders
```

For MVP slice 1, `AuthNet.Api` should not be implemented unless needed as a placeholder for package planning because API/JWT is deferred.

## Non-Functional Integration Requirements

- AuthNet shall not require the host application to use a specific UI framework.
- AuthNet MVP slice 1 shall use PostgreSQL through EF Core/Npgsql.
- AuthNet shall not prevent the host application from adding its own authorization policies.
- AuthNet shall expose authenticated user identity through standard ASP.NET authentication and authorization mechanisms.
- AuthNet shall provide secure defaults for authentication and token behavior.
