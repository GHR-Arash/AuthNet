# Functional Requirements

These requirements describe MVP slice 1 unless a requirement is explicitly marked as future scope.

## Foundation

Priority: Must Have

- The system shall target .NET 10.
- The system shall use ASP.NET Core Identity as the identity foundation.
- The system shall use standard ASP.NET Core authentication and authorization integration points.
- The system shall use EF Core for default persistence.
- The system shall use PostgreSQL as the default database provider.

Acceptance criteria:

- Given a .NET 10 ASP.NET application, when AuthNet is installed and configured, then the host application can register AuthNet services.
- Given PostgreSQL configuration is supplied, when the application starts, then AuthNet can use EF Core Identity stores.

## Account Registration

Priority: Must Have

- The system shall allow a user to register an account.
- The system shall validate required registration fields.
- The system shall prevent duplicate accounts using the same unique identifier.
- The system shall support public registration being enabled or disabled.
- The system shall disable public registration by default.
- The system shall create an unverified account when email verification is required.

Acceptance criteria:

- Given registration is enabled, when a user submits valid information, then the system creates an account.
- Given an email already exists, when a user registers with that email, then the system rejects the request.
- Given registration is disabled, when a user attempts to register, then the system rejects the request.

## Email Verification

Priority: Must Have

- The system shall generate an email verification token.
- The system shall send an email verification message.
- The system shall verify an account when a valid token is submitted.
- The system shall reject invalid or expired verification tokens.
- The system shall allow resending verification messages.

Acceptance criteria:

- Given a newly registered user, when they submit a valid verification token, then the account email is verified.
- Given an expired token, when verification is attempted, then the system rejects the request.

## Sign In

Priority: Must Have

- The system shall authenticate users using local credentials.
- The system shall allow local password sign-in by email address or username.
- The system shall reject invalid credentials.
- The system shall enforce account lockout policy.
- The system shall enforce email verification policy.
- The system shall issue a cookie session.
- The system shall support Razor Pages and MVC host applications through cookie authentication.

Acceptance criteria:

- Given valid email or username credentials, when a user signs in, then the system authenticates the user.
- Given invalid credentials, when sign in is attempted, then the system rejects the request.
- Given a locked account, when sign in is attempted, then the system denies access.

## Sign Out

Priority: Must Have

- The system shall allow an authenticated user to sign out.
- The system shall invalidate cookie sessions where applicable.

Acceptance criteria:

- Given an authenticated user, when they sign out, then the current session or token flow is ended.
- Given a signed-out user, when they access a protected resource, then access is denied.

## Password Recovery

Priority: Must Have

- The system shall allow a user to request password recovery.
- The system shall generate a password reset token.
- The system shall send password reset instructions by email.
- The system shall allow password reset with a valid token.
- The system shall reject invalid or expired reset tokens.

Acceptance criteria:

- Given a registered email, when password recovery is requested, then the system sends reset instructions.
- Given a valid reset token, when the user submits a valid new password, then the password is changed.

## Profile Management

Priority: Must Have

- The system shall allow authenticated users to view their profile.
- The system shall allow authenticated users to update allowed profile fields.
- The system shall prevent users from updating restricted fields.
- The system shall support email change verification when email is changed.

Acceptance criteria:

- Given an authenticated user, when they request their profile, then the system returns profile information.
- Given valid profile changes, when the user submits the update, then the system saves the changes.

## Password Change

Priority: Must Have

- The system shall allow authenticated users to change password.
- The system shall require the current password.
- The system shall validate the new password against password policy.
- The system shall invalidate sessions or tokens based on configuration.

Acceptance criteria:

- Given a correct current password and valid new password, when the user changes password, then the system updates the password.
- Given an incorrect current password, when password change is attempted, then the system rejects the request.

## JWT and Token Authentication

Priority: Future

- The system shall support JWT access tokens.
- The system shall support refresh tokens.
- The system shall allow token expiration to be configured.
- The system shall allow token signing settings to be configured.
- The system shall support refresh token rotation.
- The system shall support refresh token revocation.

Acceptance criteria:

- Given JWT authentication is enabled, when a user signs in successfully, then the system issues an access token.
- Given a valid refresh token, when token refresh is requested, then the system issues a new access token.
- Given a revoked refresh token, when token refresh is requested, then the system rejects the request.

## Cookie Authentication

Priority: Must Have

- The system shall support cookie authentication for MVC and Razor applications.
- The system shall allow cookie expiration to be configured.
- The system shall allow sliding expiration to be configured.

Acceptance criteria:

- Given cookie authentication is enabled, when a user signs in, then the system creates an authenticated browser session.
- Given an expired session, when a protected page is requested, then the system requires sign in.

## External Identity Providers

Priority: Must Have

- The system shall support generic OpenID Connect providers.
- The system shall allow users to link an external login to an existing account.
- The system shall prevent duplicate accounts for the same external provider identity.

Acceptance criteria:

- Given an OpenID Connect provider is configured, when a user completes external authentication, then the system signs in or provisions the user based on configuration.
- Given an external identity is already linked, when the same provider identity signs in again, then the system signs in the linked account.

## Razor Pages Account UI

Priority: Must Have

- The system shall provide built-in UI for login.
- The system shall provide built-in UI for registration.
- The system shall provide built-in UI for forgot password.
- The system shall provide built-in UI for reset password.
- The system shall provide built-in UI for email verification.
- The system shall provide built-in UI for profile management.
- The system shall provide built-in UI for password change.
- The system shall allow host applications to configure route prefix.
- The system shall allow host applications to configure application display name.
- The system shall allow host applications to configure layout integration.
- The system shall allow host applications to configure basic branding hooks.

Acceptance criteria:

- Given the UI package is enabled, when a user navigates to the login route, then the system renders the login form.
- Given a host application configures a route prefix, when account routes are registered, then the configured prefix is used.

## API Endpoints

Priority: Future

- The system shall expose API endpoints for registration.
- The system shall expose API endpoints for login.
- The system shall expose API endpoints for logout.
- The system shall expose API endpoints for token refresh.
- The system shall expose API endpoints for email verification.
- The system shall expose API endpoints for password recovery.
- The system shall expose API endpoints for profile management.
- The system shall expose API endpoints for external login initiation and callback handling.

Acceptance criteria:

- Given API mode is enabled, when a client calls the login endpoint with valid credentials, then the system returns the configured authentication result.
- Given an unauthenticated request to a protected endpoint, when authentication is required, then the system returns an unauthorized response.

## Role Authorization

Priority: Must Have

- The system shall support roles.
- The system shall expose roles to ASP.NET authorization.
- The system shall use ASP.NET Core Identity role infrastructure.

Acceptance criteria:

- Given a user has a required role, when they access a protected resource, then access is allowed.
- Given a user does not have a required role, when they access a protected resource, then access is denied.

## Account Invitations

Priority: Should Have

- The system shall allow administrators to create account invitations by email.
- The system shall send invitation links through the configured email sender.
- The system shall store invitation state and only persist a hash of the invitation token.
- The system shall reject expired, invalid, and already accepted invitations.
- The system shall prevent duplicate pending invitations for the same email.
- The system shall allow invited users to create local account credentials.
- The system shall mark the invited email confirmed after successful invitation acceptance.
- The system shall keep account invitations independent from public registration being enabled or disabled.

Acceptance criteria:

- Given an administrator creates an invitation for a new email, when the form is submitted, then the system stores a pending invitation and sends an invitation email.
- Given a valid pending invitation, when the invited user submits valid credentials, then the system creates an account, confirms the invited email, marks the invitation accepted, and signs in the user.
- Given an expired, invalid, or already accepted invitation, when acceptance is attempted, then the system rejects the request without creating a user.

## Admin Direct User Creation

Priority: Should Have

- The system shall allow administrators to directly create local users.
- The system shall create direct users through ASP.NET Core Identity.
- The system shall validate username, email, password, and duplicate account state.
- The system shall allow administrators to set the new user's email confirmation state.
- The system shall allow administrators to optionally grant the fixed `Administrator` role during creation.
- The system shall keep admin direct creation independent from public registration being enabled or disabled.

Acceptance criteria:

- Given an administrator submits valid local user details, when the form is submitted, then the system creates the user and redirects to that user's detail page.
- Given duplicate username, duplicate email, or invalid password input, when the form is submitted, then the system rejects the request without creating a user.
- Given the administrator role option is selected, when the user is created, then the system grants the fixed `Administrator` role.

## Persistence

Priority: Must Have

- The system shall provide PostgreSQL as the default persistence provider.
- The system shall persist users.
- The system shall persist roles.
- The system shall persist role assignments.
- The system shall persist external login mappings.
- The system shall persist email verification tokens.
- The system shall persist password reset tokens.
- The system shall use EF Core Identity stores for MVP slice 1.

Acceptance criteria:

- Given PostgreSQL persistence is configured, when account data changes, then the data is stored in PostgreSQL.
- Given PostgreSQL persistence is configured, when account data changes, then the system uses EF Core Identity stores.

## Email Sender

Priority: Must Have

- The system shall define an email sender integration contract.
- The system shall require email sender integration for production use.
- The system shall provide a fake or logging email sender for development.
- The system shall use the email sender for email verification.
- The system shall use the email sender for password recovery.

Acceptance criteria:

- Given a production configuration without an email sender, when the application validates AuthNet configuration, then the system reports a configuration error.
- Given a development logging email sender, when a verification email is requested, then the system records the email content through the configured development sender.
