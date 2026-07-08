# AuthNet Account Pages

AuthNet provides built-in Razor Pages for common account flows.

## Route Prefix

Routes are controlled by:

```json
"AuthNet": {
  "AccountRoutePrefix": "/auth"
}
```

With the default `/auth` prefix, routes are:

| Route | Purpose |
|---|---|
| `/auth/login` | Sign in with email or username plus password, or use an external provider. |
| `/auth/logout` | Sign out. |
| `/auth/register` | Create account when public registration is enabled. |
| `/auth/forgot-password` | Request password reset email. |
| `/auth/reset-password` | Complete password reset. |
| `/auth/confirm-email` | Confirm account email. |
| `/auth/resend-confirmation` | Request a new email confirmation message. |
| `/auth/profile` | View and update profile. |
| `/auth/change-password` | Change password for signed-in users. |
| `/auth/access-denied` | Access denied page. |
| `/auth/external-login` | External login callback flow. |
| `/auth/mfa` | View MFA status. Requires authentication. |
| `/auth/mfa/setup` | Set up authenticator-app MFA. Requires authentication. |
| `/auth/mfa/recovery-codes` | View recovery-code remaining count. Requires authentication. |
| `/auth/mfa/disable` | Disable MFA. Requires authentication. |
| `/auth/login/mfa` | Complete sign-in with an authenticator code. |
| `/auth/login/recovery-code` | Complete sign-in with a recovery code. |
| `/auth/invitations/accept` | Accept an account invitation. |

Admin routes are also mapped under the same prefix:

| Route | Purpose |
|---|---|
| `/auth/admin/users` | List and search users. Requires the `Administrator` role. |
| `/auth/admin/users/new` | Directly create a local user. Requires the `Administrator` role. |
| `/auth/admin/users/{id}` | View user state, run safe account-state actions, and manage fixed administrator access. Requires the `Administrator` role. |
| `/auth/admin/invitations` | List account invitations. Requires the `Administrator` role. |
| `/auth/admin/invitations/new` | Create and send an account invitation. Requires the `Administrator` role. |

## Registration

Public registration is disabled by default.

Enable it explicitly:

```json
"AuthNet": {
  "EnablePublicRegistration": true
}
```

When email confirmation is required, new users receive a confirmation email before they can sign in.

## Email Confirmation

Email confirmation uses the configured `IAuthNetEmailSender`.

In development, the development sender logs/stores generated email messages.

In production, provide a real sender.

Users can request a new confirmation message at:

```text
/auth/resend-confirmation
```

The response does not reveal whether the email address belongs to an account.

## Password Reset

Users request a reset link at:

```text
/auth/forgot-password
```

AuthNet sends the reset link through `IAuthNetEmailSender`.

## Profile and Password Change

The following routes require authentication:

- `/auth/profile`
- `/auth/change-password`
- `/auth/mfa`

Profile editing currently supports:

- Email address change with confirmation sent to the new address.
- Display name.
- Phone number.

The stored email address is not changed until the user opens the confirmation link sent to the new address.

## Multi-Factor Authentication

AuthNet supports authenticator-app MFA for local password sign-in.

Users can set up MFA from:

```text
/auth/mfa
```

The setup page shows a manual authenticator key and otpauth URI. After a valid authenticator code is submitted, AuthNet enables MFA through ASP.NET Core Identity and shows recovery codes.

When MFA is enabled, password sign-in redirects to:

```text
/auth/login/mfa
```

Users can sign in with a recovery code at:

```text
/auth/login/recovery-code
```

This slice does not include SMS OTP, email OTP, passkeys, generated QR-code images, remember-this-browser, admin-managed MFA reset, or a global required-MFA policy.

## Admin User Management

The admin user-management UI is server-rendered and role-based.

It requires a signed-in user in the standard ASP.NET Core Identity `Administrator` role. AuthNet does not seed a default administrator account, username, or password.

The first admin slice supports:

- List and search users by email or display name.
- Directly create a local user with username, email, optional display name, password, email confirmation state, and optional fixed administrator access.
- View email, username, display name, phone number, email confirmation state, lockout state, access failed count, external login count, and roles.
- Grant or remove the fixed `Administrator` role.
- Prevent removing the last administrator.
- Confirm or unconfirm email.
- Lock or unlock users.
- Reset access failed count.

## Account Invitations

Administrators can invite users without enabling public registration.

Use invitations when the user should set their own password without the administrator knowing it. Use direct admin creation when an administrator intentionally creates a local account with an initial password.

The invitation flow uses:

- `/auth/admin/invitations` to view invitation status.
- `/auth/admin/invitations/new` to create an invitation by email.
- `/auth/invitations/accept` for the invited user to set username, display name, and password.

Invitation links are sent through `IAuthNetEmailSender`. AuthNet stores only a hash of the raw invitation token.

When an invitation is accepted, AuthNet creates a local Identity user, marks the invited email confirmed, marks the invitation accepted, and signs in the new user. Invitations are single-use and expire after the configured invitation lifetime.

Deferred admin features include arbitrary role management, deletion, impersonation, audit events, invitation resend/cancel, bulk invitations, API endpoints, and fine-grained permissions.

## External Login

When generic OpenID Connect is enabled, the login page displays an external sign-in button.

External login behavior:

- AuthNet signs in the linked account if one exists.
- Authenticated users can link an external login from the profile page.
- New external accounts are provisioned only when the provider returns a verified email address.
- AuthNet does not implicitly link an external login to an existing local account by matching email alone.
