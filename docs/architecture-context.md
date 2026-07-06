# Architecture Context

Compact context for development sessions. Keep this file current, short, and linked to deeper docs instead of duplicating them.

## Product Shape

AuthNet is a reusable .NET 10 ASP.NET Identity and Access Management component.

It plugs into host applications through ASP.NET service registration and middleware, providing account management over ASP.NET Core Identity rather than a custom identity system.

## MVP Slice 1

Build first:

- .NET 10 target.
- ASP.NET Core Identity foundation.
- EF Core persistence.
- PostgreSQL via Npgsql as the default database path.
- Razor Pages account UI.
- Cookie authentication.
- Registration disabled by default.
- Login, logout, email verification, forgot/reset password, profile, change password.
- Email sender contract, with production sender required and development logging/fake sender allowed.
- Role infrastructure using ASP.NET Core Identity roles.
- Generic OpenID Connect external login.
- Basic UI configuration: route prefix, app display name, layout/branding hooks.

Deferred:

- API/JWT and refresh tokens.
- SPA authentication flows.
- Admin user-management UI.
- Fine-grained permissions.
- Full Razor Page override.
- Custom Identity stores.
- Provider-specific Google/Microsoft helpers.
- MFA, multi-tenancy, SAML, passkeys/passwordless.

## Architecture Principles

- Prefer ASP.NET Core Identity behavior over custom security logic.
- Keep AuthNet as packaging, configuration, UI, middleware, and extension points around Identity.
- Use secure defaults; require explicit opt-in for public registration.
- Do not make PostgreSQL abstractions broader than needed for MVP.
- Defer flexibility until a real requirement exists.
- Expose identity through standard ASP.NET authentication and authorization mechanisms.

## Proposed Package Shape

Initial package/project names:

- `AuthNet.Core`: options, shared contracts, account service boundaries.
- `AuthNet.AspNetCore`: service registration, middleware, auth integration.
- `AuthNet.UI.Razor`: Razor Pages account UI.
- `AuthNet.Persistence.Postgres`: EF Core/Npgsql Identity store setup.
- `AuthNet.ExternalProviders`: generic OpenID Connect integration.

Future package:

- `AuthNet.Api`: API/JWT/SPA flows after MVP slice 1.

## Key Integration Points

Host app should be able to configure:

- Public registration enabled/disabled.
- Cookie settings.
- Password, lockout, and email verification policy.
- PostgreSQL connection/DbContext integration.
- Email sender implementation.
- Generic OIDC provider settings.
- Account route prefix and basic UI branding/layout.

Conceptual setup:

```csharp
builder.Services.AddAuthNet(options =>
{
    options.EnablePublicRegistration = false;
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAuthNet();
```

Final API can change during implementation, but setup should remain configuration-driven and small.

## Current Canonical Docs

- Product decisions: `docs/product-decisions.md`
- Product requirements: `docs/prd.md`
- Functional requirements: `docs/functional-requirements.md`
- Integration requirements: `docs/integration-requirements.md`
- Roadmap: `docs/mvp-roadmap.md`

## Sync Rule

When implementation changes architecture, package boundaries, auth flow, persistence strategy, UI strategy, or MVP scope, update this file in the same change.

