# Product Requirements Document

## Product Name

AuthNet

## Product Summary

AuthNet is a reusable Identity and Access Management component for ASP.NET applications.

It is installed into a host ASP.NET application and configured as middleware. Once enabled, it provides common identity capabilities such as registration, login, logout, password recovery, email verification, profile management, role infrastructure, generic OpenID Connect login, built-in Razor Pages account UI, same-origin JSON account APIs with OpenAPI documentation, account invitations, and a minimal admin user-management UI.

The first MVP slice targets .NET 10 server-rendered ASP.NET applications using Razor Pages UI and cookie authentication. It also includes same-origin SPA JSON workflows backed by the existing Identity application cookie. API/JWT token authentication remains later scope.

## Problem

Most ASP.NET applications require the same identity capabilities:

- Sign up
- Sign in
- Sign out
- Password reset
- Email verification
- Profile management
- Role checks
- External identity provider login

Teams repeatedly rebuild these features for every application. This increases development time, creates inconsistent security behavior, and makes maintenance harder.

## Goal

Build a reusable ASP.NET component that can be plugged into new or existing ASP.NET applications to provide secure, configurable account and access management.

The first release shall be built on ASP.NET Core Identity rather than a custom identity system.

## Target Users

### Application Developer

Developers use AuthNet to add identity capabilities to an ASP.NET application without building account management from scratch.

Responsibilities:

- Install the AuthNet package.
- Configure middleware.
- Configure cookie authentication.
- Configure persistence.
- Enable Razor Pages account UI.
- Protect application routes, controllers, pages, and APIs.

### Application User

End users use AuthNet indirectly through the host application.

Responsibilities:

- Register an account.
- Sign in and sign out.
- Verify email.
- Recover account access.
- Manage profile.
- Use external provider login.

### Application Administrator

Administrators manage users and access inside the host application.

Responsibilities:

- View users.
- Lock or unlock users.
- Confirm or unconfirm email.
- Initiate password recovery.
- Invite users by email.

Invitation resend/cancel, deletion, impersonation, and audit events are later admin slices.

### Application Owner

Owners define identity policies for the host application.

Responsibilities:

- Decide registration policy.
- Decide password policy.
- Decide token/session policy.
- Decide external provider policy.
- Decide whether MFA is required.

## Product Scope

AuthNet shall provide:

- ASP.NET middleware integration.
- Built-in Razor/MVC account UI.
- Built-in admin user-management UI.
- Cookie authentication for MVC/Razor apps.
- Same-origin SPA JSON account, authenticator-app MFA, and external-login orchestration endpoints using the existing application cookie.
- OpenAPI JSON document for the same-origin SPA account endpoints.
- External identity provider support.
- PostgreSQL as the default persistence provider.
- Replaceable persistence abstraction.
- Integration with the host application's database.
- Role-based authorization.
- Configurable identity policies.

JWT authentication and refresh tokens remain future scope.

## MVP Scope

AuthNet MVP slice 1 shall provide:

- .NET 10 support.
- ASP.NET Core Identity foundation.
- EF Core persistence.
- PostgreSQL default persistence.
- Middleware/service registration.
- Razor Pages account UI.
- Cookie authentication.
- Registration disabled by default.
- Login and logout.
- Email verification.
- Forgot password and reset password.
- Profile management.
- Change password.
- Role infrastructure.
- Authenticator-app MFA with recovery codes.
- Generic OpenID Connect external login.
- Email sender integration contract.
- Development fake/logging email sender.
- Basic UI configuration.
- Admin user list/detail UI protected by the `Administrator` role.
- Admin-created account invitations.

## Out of Scope for MVP

- Full admin management UI beyond list/detail and reversible account-state actions.
- Invitation resend/cancel and bulk invitation management.
- Multi-tenancy.
- SAML support.
- Passwordless login.
- Passkey/WebAuthn login.
- Advanced risk-based authentication.
- Billing or subscription management.
- API/JWT token authentication.
- Refresh tokens.
- SPA token authentication flows beyond the same-origin cookie-based SPA workflow.
- Fine-grained permissions.
- Full Razor Page override.
- Custom Identity stores.
- Provider-specific Google and Microsoft helper packages.
- SMS/email OTP, passkeys, and advanced MFA policy.

## Success Criteria

AuthNet is successful when:

- A developer can add it to a new ASP.NET application with minimal configuration.
- MVC/Razor applications can use built-in account pages.
- The host application can protect resources using AuthNet authentication and authorization.
- PostgreSQL works out of the box.
- The design leaves extension points for future API/JWT, custom persistence, and provider-specific packages.
