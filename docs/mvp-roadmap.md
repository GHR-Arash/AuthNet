# MVP Roadmap

## MVP Objective

Deliver the smallest useful AuthNet version that allows an ASP.NET developer to plug identity into a .NET 10 server-rendered ASP.NET application and get working account management with Razor Pages UI, cookie authentication, PostgreSQL persistence, and generic OpenID Connect login.

## MVP Features

### Middleware

- Register AuthNet services in ASP.NET dependency injection.
- Add AuthNet middleware to the ASP.NET request pipeline.
- Integrate with standard ASP.NET authentication and authorization.
- Build on ASP.NET Core Identity.

### Authentication

- Local username/email and password sign in.
- Cookie authentication.
- Logout.

### Account Management

- Registration.
- Email verification.
- Forgot password.
- Reset password.
- Profile view.
- Profile update.
- Change password.

### UI

- Built-in Razor Pages login form.
- Built-in registration form.
- Built-in forgot password form.
- Built-in reset password form.
- Built-in email verification flow.
- Built-in profile page.
- Built-in change password page.
- Built-in admin user-management pages.
- Basic route, layout, and branding configuration.

### API

Deferred until slice 2.

### Authorization

- Roles.
- Integration with ASP.NET authorization.

### External Identity Providers

- Generic OpenID Connect.

### Persistence

- PostgreSQL persistence provider.
- EF Core Identity stores.

### Configuration

- Authentication mode.
- Password policy.
- Registration policy.
- Email verification policy.
- Lockout policy.
- Cookie settings.
- External provider settings.
- Persistence settings.
- Email sender settings.

## Suggested Delivery Phases

### Phase 1: Core Foundation

Deliver:

- Domain model.
- Configuration model.
- ASP.NET Core Identity integration.
- EF Core/PostgreSQL provider setup.
- ASP.NET service registration.
- Email sender contract.

Outcome:

The host app can register AuthNet and resolve core services using .NET 10, ASP.NET Core Identity, EF Core, and PostgreSQL.

### Phase 2: Local Account Flow

Deliver:

- Registration.
- Email verification.
- Login.
- Logout.
- Forgot password.
- Reset password.
- Change password.
- Profile management.

Outcome:

A user can complete the full local account lifecycle.

### Phase 3: Cookie Authentication and UI

Deliver:

- Cookie authentication.
- Razor Pages account UI.
- Route prefix configuration.
- Layout and branding configuration.

Outcome:

MVC/Razor apps can authenticate users through built-in account pages.

### Phase 4: Authorization

Deliver:

- Role infrastructure.
- ASP.NET authorization integration.

Outcome:

Host apps can protect resources using roles.

### Phase 5: External Provider

Deliver:

- Generic OpenID Connect.
- External login.
- Account linking.

Outcome:

Users can sign in through a configured OpenID Connect identity provider.

### Phase 6: Hardening and Documentation

Deliver:

- Configuration validation.
- Sample host application.
- Setup documentation.
- Security defaults review.

Outcome:

The MVP is usable by developers outside the core team.

## Future Enhancements

Should Have:

- Admin user management UI. Implemented in Slice 06 as list/detail plus reversible account-state actions.
- MFA. Implemented in Slice 08 as authenticator-app TOTP plus recovery codes.
- Account invitation flow. Implemented in Slice 09 as admin-created, email-delivered, single-use local account invitations.
- Audit events.
- Session and device management.
- SQL Server persistence provider.
- SQLite persistence provider.
- API/JWT authentication.
- Refresh tokens.
- SPA authentication flow.
- Full Razor Page override.
- Fine-grained permissions.
- Google and Microsoft helper packages.

Could Have:

- Multi-tenancy.
- Passwordless login.
- Magic link login.
- Passkey/WebAuthn support.
- SAML support.
- Risk-based authentication.
- Localization.
- White-label theming.

## MVP Exit Criteria

The MVP is complete when:

- AuthNet can be installed into an ASP.NET host application.
- The host application can enable middleware.
- MVC/Razor users can use built-in account UI.
- Cookie authentication works.
- External login works through generic OpenID Connect.
- PostgreSQL persistence works.
- Roles can protect host application resources.
- Administrators can use the built-in user list/detail UI when assigned the `Administrator` role.
- Production email sender integration is required and development email logging is available.
