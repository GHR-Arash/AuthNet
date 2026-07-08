# Product Decisions

This document records the confirmed product decisions for AuthNet.

## Confirmed Direction

AuthNet is a reusable ASP.NET Identity and Access Management middleware/component for new and existing ASP.NET applications.

The first release focuses on a small, shippable server-rendered identity module rather than the full long-term platform.

## Decisions

### Foundation

AuthNet v1 shall be built on top of ASP.NET Core Identity.

AuthNet shall not implement password hashing, lockout, role storage, claim handling, external login primitives, or security stamp behavior from scratch.

### Target Framework

AuthNet v1 shall target .NET 10.

Backward compatibility and multi-targeting are deferred until there is a proven consumer need.

### First MVP Slice

The first MVP slice shall provide:

- Razor Pages account UI.
- Cookie authentication.
- ASP.NET Core Identity integration.
- EF Core persistence.
- PostgreSQL default database support.
- Generic OpenID Connect external login.
- Email sender integration contract.
- Role infrastructure.
- Server-rendered admin user-management UI protected by the `Administrator` role.
- Server-rendered fixed administrator role assignment UI with last-admin protection.
- Authenticator-app MFA for local password sign-in, using ASP.NET Core Identity primitives.

### Deferred Scope

The following are deferred beyond the first MVP slice:

- API/JWT authentication.
- Refresh tokens.
- SPA-specific authentication flows.
- Arbitrary role management, invitation, deletion, impersonation, and audit events.
- SMS/email OTP, passkeys, required-MFA policy, and admin-managed MFA reset.
- Fine-grained permissions.
- Full Razor Page override.
- Custom Identity stores.
- Provider-specific Google and Microsoft helper packages.
- Multi-tenancy.

### Registration Default

Public registration shall be disabled by default.

Host applications must explicitly enable public registration.

### Email

Production use shall require an email sender integration.

Development mode may use a fake or logging email sender.

Email verification and password recovery remain part of the first MVP slice.

### UI Customization

The first MVP slice shall support basic UI configuration only.

Supported configuration should include:

- Account route prefix.
- Application display name.
- Layout integration.
- Basic branding hooks.

Full Razor Page override is deferred until the default UI and routing model are stable.
