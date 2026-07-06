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
| `/auth/login` | Sign in with email/password or external provider. |
| `/auth/logout` | Sign out. |
| `/auth/register` | Create account when public registration is enabled. |
| `/auth/forgot-password` | Request password reset email. |
| `/auth/reset-password` | Complete password reset. |
| `/auth/confirm-email` | Confirm account email. |
| `/auth/profile` | View and update profile. |
| `/auth/change-password` | Change password for signed-in users. |
| `/auth/access-denied` | Access denied page. |
| `/auth/external-login` | External login callback flow. |

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

Profile editing currently supports:

- Display name.
- Phone number.

Email change verification is not implemented in MVP slice 1.

## External Login

When generic OpenID Connect is enabled, the login page displays an external sign-in button.

If the external provider returns an email address:

- AuthNet signs in the linked account if one exists.
- AuthNet creates or links an account through ASP.NET Core Identity if needed.

