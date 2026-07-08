# Developer Onboarding

This guide is for engineers joining AuthNet development. Product/spec artifacts live in `docs/`; developer operating docs live in `docs/developer/`.

## What AuthNet Is

AuthNet is a reusable .NET 10 ASP.NET Identity and Access Management component.

MVP slice 1 is server-rendered and cookie-based:

- ASP.NET Core Identity foundation.
- EF Core persistence.
- PostgreSQL through Npgsql.
- Razor Pages account UI.
- Cookie authentication.
- Email verification and password reset.
- Role infrastructure.
- Admin user-management UI protected by the `Administrator` role.
- Account invitation flow.
- Authenticator-app MFA with recovery codes.
- Generic OpenID Connect external login.

Deferred work:

- API/JWT and refresh tokens.
- SPA authentication.
- Invitation resend/cancel, deletion, impersonation, and audit events.
- Fine-grained permissions.
- Custom stores.
- SMS/email OTP, passkeys, advanced MFA policy, and multi-tenancy.

## Repository Map

- `src/AuthNet.Core`: options, shared contracts, email abstractions.
- `src/AuthNet.AspNetCore`: service registration, middleware integration, validation, development email sender.
- `src/AuthNet.Persistence.Postgres`: Identity user, EF Core DbContext, PostgreSQL migrations.
- `src/AuthNet.UI.Razor`: reusable Razor Pages account and admin UI.
- `src/AuthNet.ExternalProviders`: generic OpenID Connect registration.
- `samples/AuthNet.SampleHost`: runnable host app for manual verification.
- `tests/AuthNet.Tests`: unit tests for configuration and development seams.
- `docs/architecture-context.md`: compact architecture context; keep it synced.
- `docs/tasks.md`: local MVP task checklist.

## Architecture Rules

- Build on ASP.NET Core Identity; do not reimplement password hashing, lockout, claims, roles, or external login primitives.
- Keep public registration disabled by default.
- Production must use a real `IAuthNetEmailSender`; the development sender is not allowed in production.
- Use standard ASP.NET Core authentication/authorization integration points.
- Keep PostgreSQL/EF Core as the only persistence path for MVP slice 1.
- Update `docs/architecture-context.md` when package boundaries, auth flow, persistence, UI strategy, commands, or MVP scope change.

## Development Workflow

1. Read `AGENTS.md` and `docs/architecture-context.md`.
2. Pick the next unchecked item from `docs/tasks.md` or a user-provided issue.
3. Make the smallest vertical change that satisfies the acceptance criteria.
4. Run build and tests before committing.
5. Update developer docs or architecture context if behavior or commands changed.

## Verification Expectations

Minimum before commit:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
```

When touching startup, routing, authentication, or Razor UI, also start the sample host:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --no-build --no-launch-profile --urls http://127.0.0.1:5127
```

The command keeps running until stopped. Successful startup includes:

```text
Now listening on: http://127.0.0.1:5127
Application started.
```

## Common Pitfalls

- Do not use the system `dotnet`; this repo relies on the project-local `.dotnet\dotnet.exe`.
- Do not commit `.dotnet/`, `.tools/`, `bin/`, `obj/`, or sample `App_Data/`.
- Do not add JWT/API behavior to MVP slice 1 unless the scope changes explicitly.
- Do not introduce provider-specific Google/Microsoft helpers before the generic OIDC path is hardened.
- Do not treat the development email sender as production-safe.
