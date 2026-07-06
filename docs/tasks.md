# AuthNet MVP Tasks

Local implementation backlog for MVP slice 1. Keep this compact and update it with `docs/architecture-context.md` when architecture or scope changes.

## 1. Scaffold AuthNet Solution and Sample Host

Blocked by: None

Build a .NET 10 solution with the initial package projects and a sample ASP.NET host app.

Acceptance criteria:

- [x] The solution restores and builds.
- [x] The sample host app runs.
- [x] Project names match `docs/architecture-context.md`.
- [x] The sample host references the AuthNet packages through project references.

## 2. Wire ASP.NET Core Identity with PostgreSQL

Blocked by: Task 1

Add ASP.NET Core Identity, EF Core, and Npgsql setup through AuthNet registration APIs.

Acceptance criteria:

- [x] The sample host can configure PostgreSQL-backed Identity stores.
- [x] Identity users and roles are registered through AuthNet setup.
- [x] The chosen EF schema/migration workflow is documented.
- [x] The solution has a verification path for creating or applying the Identity schema.

## 3. Add AuthNet Configuration and Validation

Blocked by: Task 1

Implement options for registration, cookie settings, email policy, route prefix, and basic branding.

Acceptance criteria:

- [x] Public registration defaults to disabled.
- [x] Cookie, password, lockout, email verification, route prefix, and branding options are configurable.
- [x] Production configuration without an email sender fails clearly.
- [x] Development configuration can use the development email sender.

## 4. Implement Local Account Razor Pages

Blocked by: Tasks 2, 3

Add the self-service account lifecycle using Razor Pages and cookie authentication.

Acceptance criteria:

- [x] Users can log in and log out.
- [x] Public registration works only when explicitly enabled.
- [x] Users can verify email.
- [x] Users can request and complete password reset.
- [x] Authenticated users can view/update profile information.
- [x] Authenticated users can change password.

## 5. Add Email Sender Contract and Development Sender

Blocked by: Task 3

Define the production email sender contract and a development logging/fake implementation.

Acceptance criteria:

- [x] Verification emails use the configured sender.
- [x] Password reset emails use the configured sender.
- [x] Development sender exposes or logs generated messages for local testing.
- [x] Production mode requires a real sender implementation.

## 6. Integrate Role Infrastructure

Blocked by: Task 2

Enable ASP.NET Core Identity roles and standard role-based authorization.

Acceptance criteria:

- [x] Roles are enabled in Identity setup.
- [x] The sample host demonstrates a role-protected resource.
- [x] Role checks use standard ASP.NET Core authorization.
- [x] No custom fine-grained permission model is introduced.

## 7. Add Generic OpenID Connect Login

Blocked by: Tasks 3, 4

Configure generic OpenID Connect external login for the Razor Pages and cookie authentication flow.

Acceptance criteria:

- [x] The sample host can configure a generic OIDC provider.
- [x] Users can start an OIDC challenge.
- [x] Callback handling signs in or links the external identity through ASP.NET Core Identity.
- [x] Provider-specific Google/Microsoft helpers are not introduced in this slice.

## 8. Harden Developer Experience and Docs

Blocked by: Tasks 1-7

Add the documentation and verification path needed for another developer to use the MVP.

Acceptance criteria:

- [x] Setup docs explain restore, build, run, and required configuration.
- [x] Sample configuration covers PostgreSQL, cookies, email sender, and OIDC.
- [x] Test commands are documented.
- [x] `docs/architecture-context.md` is updated if implementation differs from the plan.
