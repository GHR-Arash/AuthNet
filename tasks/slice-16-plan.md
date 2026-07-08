# Slice 16 Plan: SPA Account Workflow Completion

## Overview

Slice 16 completes the remaining user-owned account workflows for same-origin SPA clients. Slice 14 added the initial cookie-backed SPA API, and Slice 15 added the OpenAPI document. This slice adds JSON endpoints for completing email/password workflows and editing authenticated account state, while preserving the existing Razor Pages behavior and keeping token auth, MFA JSON, admin JSON, and external-login JSON out of scope.

## Scope

In scope:

- Add request DTOs and service methods for:
  - Reset password completion.
  - Email confirmation completion.
  - Profile update.
  - Change password.
- Add same-origin JSON endpoints under the existing `{AccountRoutePrefix}/api` root:
  - `POST /auth/api/reset-password`
  - `POST /auth/api/confirm-email`
  - `PUT /auth/api/profile`
  - `POST /auth/api/change-password`
- Preserve existing Razor Pages account workflow behavior.
- Use existing ASP.NET Core Identity token semantics and password policy.
- Return consistent `AuthNetApiResult` success/error responses.
- Keep email/user enumeration behavior safe.
- Update `/auth/api/openapi.json` for all new routes and schemas.
- Add focused integration tests for new endpoint behavior and OpenAPI coverage.
- Add sample `/Spa` controls or links for discoverability.
- Update user/developer docs, architecture context, next-iteration context, and `context.md`.

Out of scope:

- JWT access tokens and refresh tokens.
- MFA JSON endpoints.
- Admin JSON APIs.
- External login JSON orchestration.
- Invitation acceptance JSON endpoint.
- Cross-origin CORS policy management.
- Swagger/Scalar UI.
- Changing Razor Pages route behavior.

## Architecture Decisions

- Keep implementation in `AuthNet.Api`, using the existing `IAuthNetSpaAccountService` service boundary.
- Keep endpoint mapping inside `MapAuthNetApi()` so consumers receive the routes through `app.MapAuthNet()`.
- Use existing Identity token generation/validation semantics. The JSON endpoints should accept the same URL-safe token format emitted by existing email links where practical.
- Use `PUT /profile` for profile update because it updates user-owned profile fields, and `POST /change-password` because it performs a credential action.
- Use authenticated cookie session for profile update and change password.
- Keep email confirmation and reset password anonymous because users arrive from email links.
- Update the package-owned OpenAPI document manually and keep tests enforcing path/schema coverage.

## Candidate Routes

With default `AccountRoutePrefix=/auth`:

- `POST /auth/api/reset-password`
- `POST /auth/api/confirm-email`
- `PUT /auth/api/profile`
- `POST /auth/api/change-password`

Existing routes remain unchanged:

- `GET /auth/api/session`
- `GET /auth/api/profile`
- `POST /auth/api/login`
- `POST /auth/api/logout`
- `POST /auth/api/register`
- `POST /auth/api/forgot-password`
- `POST /auth/api/resend-confirmation`
- `GET /auth/api/openapi.json`

## Response Shape

Reuse `AuthNetApiResult` for command endpoints:

```json
{
  "succeeded": true,
  "message": "Password reset.",
  "errors": []
}
```

Use `AuthNetProfileResponse` for successful profile update if the updated profile should be returned. Otherwise return `AuthNetApiResult` and let clients refetch `GET /auth/api/profile`. Default plan: return `AuthNetProfileResponse` from `PUT /profile` to avoid an extra round-trip.

## Task List

### Phase 1: Contracts and Service Surface

- [x] Task 1: Add request/response contracts for reset, confirm email, profile update, and change password.
- [x] Task 2: Extend `IAuthNetSpaAccountService` with ordered service methods.

### Checkpoint: Foundation

- [x] Solution builds.
- [x] Existing SPA API tests still pass.
- [x] No existing endpoint behavior changes.

### Phase 2: Anonymous Email-Link Completion Flows

- [x] Task 3: Implement reset-password JSON endpoint.
- [x] Task 4: Implement confirm-email JSON endpoint.
- [x] Task 5: Add focused tests for anonymous completion flows.

### Checkpoint: Email-Link Flows

- [x] Reset password succeeds with valid token and rejects invalid token.
- [x] Confirm email succeeds with valid token and rejects invalid token.
- [x] Forgot-password and registration/resend-confirmation flows still emit compatible links/tokens.

### Phase 3: Authenticated Account Update Flows

- [x] Task 6: Implement profile update JSON endpoint.
- [x] Task 7: Implement change-password JSON endpoint.
- [x] Task 8: Add focused tests for authenticated update flows.

### Checkpoint: Authenticated Flows

- [x] Profile update requires authentication and persists allowed fields.
- [x] Change password requires authentication, validates current password, and enforces password policy.
- [x] Existing `GET /auth/api/profile` reflects updated state.

### Phase 4: OpenAPI, Sample, Docs, and Verification

- [x] Task 9: Update OpenAPI document and OpenAPI tests.
- [x] Task 10: Update sample `/Spa` page and sample links.
- [x] Task 11: Update docs, context, and planning artifacts.
- [x] Task 12: Run focused and full verification.

### Checkpoint: Complete

- [x] Focused Slice 16 tests pass.
- [x] Existing SPA API and OpenAPI tests pass.
- [x] Full verification passes.
- [x] Slice 16 artifacts are complete and named with `slice-16`.
- [x] JWT, MFA JSON, admin JSON, and external-login JSON remain explicitly deferred.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Token format mismatch between Razor links and JSON endpoints | High | Reuse existing Base64Url token handling and test generated links/tokens end to end. |
| Profile update accidentally permits restricted fields | High | Restrict DTO to display name and phone number only unless requirements expand. |
| Change password leaves stale sessions/security stamp behavior inconsistent | Medium | Use ASP.NET Core Identity `ChangePasswordAsync` and refresh sign-in if existing Razor behavior does so. |
| OpenAPI document drifts from endpoint behavior | Medium | Extend `AuthNetOpenApiTests` for every new path/schema/status. |
| Scope expands into MFA or token auth | High | Keep MFA JSON and JWT/refresh tokens as future slices. |

## Open Questions

- Resolved: `PUT /auth/api/profile` returns `AuthNetProfileResponse` on success.
- Resolved: profile update includes display name and phone number only.
- Resolved: reset-password completion accepts `email + code + password`, matching the existing Razor reset page and reset email link.
