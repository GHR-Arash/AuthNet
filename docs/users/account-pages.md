# AuthNet Account Pages

AuthNet provides built-in Razor Pages for common account flows.

The package fallback layout includes a built-in navigation shell and package-owned CSS from the Razor Class Library. Hosts can still replace the layout through `AuthNetOptions.LayoutPath`; when they do, they own the surrounding navigation and styling.

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
| `/auth` | Built-in AuthNet home page with account, security, and admin navigation. |
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
| `/auth/admin/users` | List and search users. Requires `Administrator` or `authnet.users.view`. |
| `/auth/admin/users/new` | Directly create a local user. Requires `Administrator` or `authnet.users.manage`. |
| `/auth/admin/users/{id}` | View user state, run account-state actions, and manage role assignment. Requires `Administrator` or `authnet.users.manage`. |
| `/auth/admin/roles` | List roles. Requires `Administrator` or `authnet.roles.view`. |
| `/auth/admin/roles/new` | Create a role. Requires `Administrator` or `authnet.roles.manage`. |
| `/auth/admin/roles/{id}` | View role permissions and assign/remove built-in permissions. View requires `authnet.roles.view`; changes require `authnet.roles.manage`. |
| `/auth/admin/audit` | Review admin audit events. Requires `Administrator` or `authnet.audit.view`. |
| `/auth/admin/invitations` | List account invitations. Requires `Administrator` or `authnet.invitations.manage`. |
| `/auth/admin/invitations/new` | Create and send an account invitation. Requires `Administrator` or `authnet.invitations.manage`. |

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

The admin user-management UI is server-rendered and role/permission-based.

The `Administrator` role remains the built-in superuser role and satisfies every AuthNet admin permission. AuthNet does not seed a default administrator account, username, or password; host applications must bootstrap the first administrator through their own startup, deployment, or operations process.

The repository sample host creates a demo admin user in code at startup and also demonstrates an optional explicit bootstrap behind `AuthNet:AdminBootstrap`. The optional bootstrap can create a configured admin user when a password is supplied, or promote an existing user by email without a password. This sample-host behavior is not package behavior and should not be treated as production default credentials.

The admin UI supports:

- List and search users by email or display name.
- Directly create a local user with username, email, optional display name, password, email confirmation state, and optional fixed administrator access.
- View email, username, display name, phone number, email confirmation state, lockout state, access failed count, external login count, and roles.
- Create roles through ASP.NET Core Identity.
- Assign or remove roles on user detail.
- Grant or remove the `Administrator` role.
- Prevent removing the last administrator.
- Assign or remove built-in AuthNet permissions on roles.
- Confirm or unconfirm email.
- Lock or unlock users.
- Reset access failed count.
- Review admin audit events for successful admin mutations.

AuthNet permissions are stored as ASP.NET Core Identity role claims with claim type `authnet.permission`. The built-in permission values are:

- `authnet.users.view`
- `authnet.users.manage`
- `authnet.roles.view`
- `authnet.roles.manage`
- `authnet.invitations.manage`
- `authnet.audit.view`

Permission changes apply through the normal ASP.NET Core Identity claims principal. A signed-in user may need to sign out and sign in again before newly assigned role permissions are reflected in their current session.

The manage permissions for users and roles also satisfy the corresponding view policies.

## Admin Audit Events

AuthNet records successful administrator mutations in a persisted audit table.

The audit flow uses:

- `/auth/admin/audit` to review recent audit events.

Current audit coverage includes direct user creation, invitation creation, role creation, role assignment/removal, role permission assignment/removal, administrator grant/remove, email confirm/unconfirm, lock/unlock, and access failure reset.

Audit events include timestamp, action, outcome, actor, target, and compact metadata. AuthNet does not store passwords, raw invitation tokens, or invitation acceptance URLs in audit metadata.

## Account Invitations

Administrators can invite users without enabling public registration.

Use invitations when the user should set their own password without the administrator knowing it. Use direct admin creation when an administrator intentionally creates a local account with an initial password.

The invitation flow uses:

- `/auth/admin/invitations` to view invitation status.
- `/auth/admin/invitations/new` to create an invitation by email.
- `/auth/invitations/accept` for the invited user to set username, display name, and password.

Invitation links are sent through `IAuthNetEmailSender`. AuthNet stores only a hash of the raw invitation token.

When an invitation is accepted, AuthNet creates a local Identity user, marks the invited email confirmed, marks the invitation accepted, and signs in the new user. Invitations are single-use and expire after the configured invitation lifetime.

Deferred admin features include role deletion, host-defined permission catalogs, tenant-scoped permissions, impersonation, audit export, audit retention policy, tamper-proof audit signing, invitation resend/cancel, bulk invitations, and API endpoints.

## External Login

When generic OpenID Connect is enabled, the login page displays an external sign-in button.

External login behavior:

- AuthNet signs in the linked account if one exists.
- Authenticated users can link an external login from the profile page.
- New external accounts are provisioned only when the provider returns a verified email address.
- AuthNet does not implicitly link an external login to an existing local account by matching email alone.
