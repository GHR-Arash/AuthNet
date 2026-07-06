# AuthNet MVP Tasks

Local implementation backlog for MVP slice 1. Keep this compact and update it with `docs/architecture-context.md` when architecture or scope changes.

## 1. Scaffold AuthNet Solution and Sample Host

Blocked by: None

Build a .NET 10 solution with the initial package projects and a sample ASP.NET host app.

Acceptance criteria:

- [ ] The solution restores and builds.
- [ ] The sample host app runs.
- [ ] Project names match `docs/architecture-context.md`.
- [ ] The sample host references the AuthNet packages through project references.

## 2. Wire ASP.NET Core Identity with PostgreSQL

Blocked by: Task 1

Add ASP.NET Core Identity, EF Core, and Npgsql setup through AuthNet registration APIs.

Acceptance criteria:

- [ ] The sample host can configure PostgreSQL-backed Identity stores.
- [ ] Identity users and roles are registered through AuthNet setup.
- [ ] The chosen EF schema/migration workflow is documented.
- [ ] The solution has a verification path for creating or applying the Identity schema.

## 3. Add AuthNet Configuration and Validation

Blocked by: Task 1

Implement options for registration, cookie settings, email policy, route prefix, and basic branding.

Acceptance criteria:

- [ ] Public registration defaults to disabled.
- [ ] Cookie, password, lockout, email verification, route prefix, and branding options are configurable.
- [ ] Production configuration without an email sender fails clearly.
- [ ] Development configuration can use the development email sender.

## 4. Implement Local Account Razor Pages

Blocked by: Tasks 2, 3

Add the self-service account lifecycle using Razor Pages and cookie authentication.

Acceptance criteria:

- [ ] Users can log in and log out.
- [ ] Public registration works only when explicitly enabled.
- [ ] Users can verify email.
- [ ] Users can request and complete password reset.
- [ ] Authenticated users can view/update profile information.
- [ ] Authenticated users can change password.

## 5. Add Email Sender Contract and Development Sender

Blocked by: Task 3

Define the production email sender contract and a development logging/fake implementation.

Acceptance criteria:

- [ ] Verification emails use the configured sender.
- [ ] Password reset emails use the configured sender.
- [ ] Development sender exposes or logs generated messages for local testing.
- [ ] Production mode requires a real sender implementation.

## 6. Integrate Role Infrastructure

Blocked by: Task 2

Enable ASP.NET Core Identity roles and standard role-based authorization.

Acceptance criteria:

- [ ] Roles are enabled in Identity setup.
- [ ] The sample host demonstrates a role-protected resource.
- [ ] Role checks use standard ASP.NET Core authorization.
- [ ] No custom fine-grained permission model is introduced.

## 7. Add Generic OpenID Connect Login

Blocked by: Tasks 3, 4

Configure generic OpenID Connect external login for the Razor Pages and cookie authentication flow.

Acceptance criteria:

- [ ] The sample host can configure a generic OIDC provider.
- [ ] Users can start an OIDC challenge.
- [ ] Callback handling signs in or links the external identity through ASP.NET Core Identity.
- [ ] Provider-specific Google/Microsoft helpers are not introduced in this slice.

## 8. Harden Developer Experience and Docs

Blocked by: Tasks 1-7

Add the documentation and verification path needed for another developer to use the MVP.

Acceptance criteria:

- [ ] Setup docs explain restore, build, run, and required configuration.
- [ ] Sample configuration covers PostgreSQL, cookies, email sender, and OIDC.
- [ ] Test commands are documented.
- [ ] `docs/architecture-context.md` is updated if implementation differs from the plan.

