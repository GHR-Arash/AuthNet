# Slice 14 Plan: SPA Workflow Foundation

## Overview

Slice 14 starts SPA support by adding a JSON endpoint surface for browser-based clients while reusing AuthNet's existing ASP.NET Core Identity, cookie authentication, email flows, and PostgreSQL persistence. This is the first SPA slice, so it should focus on same-origin SPA/BFF-style workflows instead of adding JWT and refresh-token persistence at the same time.

## Scope

In scope:

- Add an `AuthNet.Api` package/project for JSON endpoints.
- Add endpoint mapping for API account routes under the existing AuthNet route prefix.
- Add JSON endpoints for current user, registration, login, logout, forgot password, resend confirmation, and profile read.
- Use the existing application cookie for authenticated browser sessions.
- Return consistent JSON success/error responses and validation errors.
- Add antiforgery/CSRF protection guidance for unsafe browser requests.
- Add a sample SPA page or minimal sample assets in `AuthNet.SampleHost` to exercise the workflow.
- Add integration tests for JSON account flows.
- Update docs, context, and package guidance.

Out of scope:

- JWT access tokens.
- Refresh tokens and refresh-token rotation.
- API authorization for non-browser machine clients.
- External login JSON callback redesign.
- MFA JSON endpoints.
- Admin JSON APIs.
- CORS policy management for cross-origin SPAs.
- A full frontend framework app.

## Architecture Decisions

- Create `src/AuthNet.Api` as a new packable package that depends on `AuthNet.Core`, `AuthNet.AspNetCore`, and `AuthNet.Persistence.Postgres` as needed.
- Keep `AuthNet.AspNetCore` as the primary integration package and have it expose or depend on the API mapping package if that fits current package conventions.
- Use minimal APIs for JSON endpoints to avoid duplicating Razor Page UI concerns.
- Use the existing Identity cookie for session continuity in same-origin browser SPAs.
- Keep route prefix compatible with `AuthNetOptions.AccountRoutePrefix`; API routes should live under a clear subpath such as `/auth/api`.
- Do not change existing Razor Pages routes or behavior.
- Do not add new database tables in this slice unless endpoint behavior proves it is required.
- Treat JWT/refresh token support as a follow-up token-auth slice, not part of this first SPA workflow.

## Candidate API Routes

With default `AccountRoutePrefix=/auth`:

- `GET /auth/api/session` returns authenticated user/session state.
- `POST /auth/api/register` creates a local account when public registration is enabled.
- `POST /auth/api/login` signs in with email or username and password.
- `POST /auth/api/logout` signs out.
- `POST /auth/api/forgot-password` sends reset instructions.
- `POST /auth/api/resend-confirmation` sends a confirmation email.
- `GET /auth/api/profile` returns profile data for the current user.

Deferred routes:

- Password reset completion JSON endpoint.
- Email confirmation JSON endpoint.
- Profile update and change password JSON endpoints.
- MFA JSON endpoints.
- Invitation acceptance JSON endpoint.
- External login JSON orchestration.

## Response Shape

Use a small consistent response contract:

```json
{
  "succeeded": true,
  "message": "Signed in.",
  "errors": []
}
```

Validation errors should be field-addressable where possible:

```json
{
  "succeeded": false,
  "message": "Validation failed.",
  "errors": [
    { "code": "PasswordTooShort", "field": "password", "description": "..." }
  ]
}
```

## Task List

### Phase 1: API Package and Contracts

- [x] Task 1: Add `AuthNet.Api` project and solution/package wiring.
- [x] Task 2: Add API response contracts and endpoint mapping extension.
- [x] Task 3: Add session and logout endpoints.

### Checkpoint: API Foundation

- [x] Solution builds.
- [x] `/auth/api/session` works for anonymous and signed-in users.
- [x] `/auth/api/logout` clears cookie sessions.

### Phase 2: Account JSON Flows

- [x] Task 4: Add login endpoint.
- [x] Task 5: Add registration endpoint.
- [x] Task 6: Add forgot-password and resend-confirmation endpoints.
- [x] Task 7: Add profile read endpoint.

### Checkpoint: Account Flows

- [x] SPA can register, sign in, read session/profile, request email flows, and sign out using JSON endpoints.
- [x] Existing Razor UI tests still pass.
- [x] Email sender behavior remains shared with existing flows.

### Phase 3: Sample, Docs, and Verification

- [x] Task 8: Add sample SPA smoke page in sample host.
- [x] Task 9: Update docs, context, and package guidance.
- [x] Task 10: Run focused and full verification.

### Checkpoint: Complete

- [x] Focused SPA API tests pass.
- [x] Full verification passes.
- [x] Slice 14 artifacts are complete and named with `slice-14`.
- [x] JWT/refresh-token work remains explicitly deferred.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Scope expands into full token auth | High | Keep this slice cookie/BFF-style and defer JWT/refresh tokens. |
| Duplicate account logic diverges from Razor Pages | High | Reuse Identity managers, existing options, and email message helpers. |
| CSRF behavior is underspecified | High | Document same-origin assumptions and add antiforgery guidance before claiming production readiness. |
| Package dependency direction gets muddy | Medium | Keep `AuthNet.Api` narrowly focused and update package docs. |
| SPA sample becomes a frontend project | Medium | Add a minimal sample page/assets only, not a full framework app. |

## Open Questions

- Should `AddAuthNet` map API endpoints by default, or should hosts call a separate `MapAuthNetApi()`?
- Should the first SPA sample be a plain HTML/JavaScript page in the sample host or a separate sample project?
- Should password reset and email confirmation completion endpoints be included in this slice or the next SPA slice?
