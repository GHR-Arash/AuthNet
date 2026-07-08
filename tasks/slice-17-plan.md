# Slice 17 Plan: SPA MFA JSON Workflows

## Overview

Slice 17 adds same-origin SPA JSON endpoints for authenticator-app MFA while preserving the existing Razor Pages MFA behavior. The endpoints will use ASP.NET Core Identity primitives, the existing application cookie, and the existing two-factor challenge cookie created by password login. JWT access tokens, refresh tokens, admin MFA reset, required-MFA policy, SMS/email OTP, and passkeys remain deferred.

## Scope

In scope:

- Add request/response DTOs and service methods for:
  - MFA status.
  - Authenticator setup start.
  - Authenticator setup verification.
  - MFA disable.
  - Recovery-code status.
  - Recovery-code regeneration.
  - MFA challenge sign-in.
  - Recovery-code challenge sign-in.
- Add same-origin JSON endpoints under the existing `{AccountRoutePrefix}/api` root:
  - `GET /auth/api/mfa`
  - `POST /auth/api/mfa/setup/start`
  - `POST /auth/api/mfa/setup/verify`
  - `POST /auth/api/mfa/disable`
  - `GET /auth/api/mfa/recovery-codes`
  - `POST /auth/api/mfa/recovery-codes/regenerate`
  - `POST /auth/api/login/mfa`
  - `POST /auth/api/login/recovery-code`
- Preserve existing Razor Pages MFA route behavior.
- Reuse existing ASP.NET Core Identity authenticator key, TOTP, recovery-code, lockout, and two-factor sign-in behavior.
- Return consistent `AuthNetApiResult` success/error responses where commands do not naturally return data.
- Update `/auth/api/openapi.json` for all new routes and schemas.
- Add focused integration tests that mirror existing Razor MFA coverage for the JSON API.
- Update sample `/Spa` controls for MFA smoke testing.
- Update user/developer docs, architecture context, next-iteration context, and `context.md`.

Out of scope:

- JWT access tokens and refresh tokens.
- Cross-origin SPA auth/CORS policy management.
- SMS/email OTP, passkeys, remember-this-browser, required-MFA policy, and admin-managed MFA reset.
- Admin JSON APIs.
- External-login JSON orchestration.
- Invitation acceptance JSON endpoint.
- Swagger/Scalar UI.
- Changing Razor Pages route behavior.

## Architecture Decisions

- Keep implementation in `AuthNet.Api`, using the existing SPA service boundary style. Add MFA methods to `IAuthNetSpaAccountService` unless the implementation becomes large enough to justify a package-internal MFA service.
- Use the same Identity APIs as the Razor Pages:
  - `GetAuthenticatorKeyAsync`
  - `ResetAuthenticatorKeyAsync`
  - `VerifyTwoFactorTokenAsync`
  - `SetTwoFactorEnabledAsync`
  - `GenerateNewTwoFactorRecoveryCodesAsync`
  - `CountRecoveryCodesAsync`
  - `TwoFactorAuthenticatorSignInAsync`
  - `TwoFactorRecoveryCodeSignInAsync`
- Require an authenticated application cookie for MFA management endpoints under `/mfa`.
- Keep `/login/mfa` and `/login/recovery-code` anonymous in route policy but require the Identity two-factor challenge user through `GetTwoFactorAuthenticationUserAsync`.
- Use the existing login endpoint behavior where password sign-in returns a `RequiresTwoFactor` conflict when MFA is required. SPA clients then call the MFA completion endpoints using the same cookie jar.
- Return recovery codes only immediately after enabling MFA or regenerating codes. Status endpoints should return counts/state, not stored code values.
- Build authenticator URI in the API with the same issuer/email/secret format as the Razor setup page.
- Update the package-owned OpenAPI document manually and keep tests enforcing path/schema coverage.

## Candidate Routes

With default `AccountRoutePrefix=/auth`:

- `GET /auth/api/mfa`
- `POST /auth/api/mfa/setup/start`
- `POST /auth/api/mfa/setup/verify`
- `POST /auth/api/mfa/disable`
- `GET /auth/api/mfa/recovery-codes`
- `POST /auth/api/mfa/recovery-codes/regenerate`
- `POST /auth/api/login/mfa`
- `POST /auth/api/login/recovery-code`

Existing SPA routes remain unchanged:

- `GET /auth/api/session`
- `GET /auth/api/profile`
- `PUT /auth/api/profile`
- `POST /auth/api/login`
- `POST /auth/api/logout`
- `POST /auth/api/register`
- `POST /auth/api/forgot-password`
- `POST /auth/api/reset-password`
- `POST /auth/api/resend-confirmation`
- `POST /auth/api/confirm-email`
- `POST /auth/api/change-password`
- `GET /auth/api/openapi.json`

## Response Shape

Proposed response DTOs:

- `AuthNetMfaStatusResponse`
  - `isMfaEnabled`
  - `hasAuthenticator`
  - `recoveryCodesLeft`
- `AuthNetMfaSetupStartResponse`
  - `sharedKey`
  - `authenticatorUri`
- `AuthNetMfaSetupVerifyResponse`
  - `isMfaEnabled`
  - `recoveryCodes`
- `AuthNetRecoveryCodesResponse`
  - `recoveryCodesLeft`
- `AuthNetRecoveryCodesRegenerateResponse`
  - `recoveryCodes`
- `AuthNetMfaChallengeRequest`
  - `code`
  - `rememberMe`
- `AuthNetRecoveryCodeLoginRequest`
  - `recoveryCode`

Use `AuthNetApiResult` for disable and failed command responses. Use existing login-style response handling for MFA challenge results:

- `200` on successful MFA sign-in.
- `400` for validation errors.
- `401` when no two-factor challenge user exists or a code is invalid.
- `409` for locked-out accounts.

## Task List

### Phase 1: Contracts and Service Surface

- [x] Task 1: Add MFA request and response contracts.
- [x] Task 2: Extend the SPA account service surface with MFA methods.

### Checkpoint: Foundation

- [x] Solution builds.
- [x] Existing SPA API, account workflow, and Razor MFA tests still pass.
- [x] No existing endpoint behavior changed.

### Phase 2: Authenticated MFA Management

- [x] Task 3: Implement MFA status and setup-start service behavior.
- [x] Task 4: Implement setup verification and recovery-code generation.
- [x] Task 5: Implement MFA disable and recovery-code status/regeneration.
- [x] Task 6: Add focused tests for authenticated MFA management endpoints.

### Checkpoint: MFA Management

- [x] Authenticated users can inspect MFA state.
- [x] Authenticated users can start setup and receive shared key/URI.
- [x] Authenticated users can verify TOTP and receive one-time recovery codes.
- [x] Authenticated users can disable MFA.
- [x] Authenticated users can view recovery-code count and regenerate codes.

### Phase 3: MFA Login Challenge Completion

- [x] Task 7: Implement authenticator-code login challenge endpoint.
- [x] Task 8: Implement recovery-code login challenge endpoint.
- [x] Task 9: Add focused tests for MFA sign-in and recovery-code sign-in.

### Checkpoint: Login Challenge Flows

- [x] Password login for an MFA-enabled user returns `RequiresTwoFactor`.
- [x] Valid authenticator code completes sign-in.
- [x] Invalid authenticator code fails safely.
- [x] Valid recovery code completes sign-in and consumes one code.
- [x] Locked-out MFA challenge state maps to conflict consistently.

### Phase 4: OpenAPI, Sample, Docs, and Verification

- [x] Task 10: Map endpoints and update OpenAPI document/tests.
- [x] Task 11: Update sample `/Spa` page for MFA smoke workflows.
- [x] Task 12: Update docs, context, and planning artifacts.
- [x] Task 13: Run focused and full verification.

### Checkpoint: Complete

- [x] Focused Slice 17 SPA MFA API tests pass.
- [x] Existing Razor MFA tests pass.
- [x] Existing SPA API, account workflow, and OpenAPI tests pass.
- [x] Full verification passes.
- [x] Slice 17 artifacts are complete and named with `slice-17`.
- [x] JWT, refresh tokens, admin MFA reset, required-MFA policy, SMS/email OTP, and passkeys remain explicitly deferred.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Two-factor challenge cookie behavior differs from normal authenticated endpoints | High | Test password login followed by JSON MFA completion using the same `AuthNetTestHost` cookie jar. |
| Recovery codes leak after initial display | High | Return recovery codes only from setup verify and regenerate endpoints; status returns count only. |
| Authenticator URI diverges from Razor page behavior | Medium | Reuse the same issuer/email/secret formatting and test shared key availability. |
| Endpoint status codes drift from login endpoint conventions | Medium | Centralize result-to-response mapping for MFA challenge outcomes. |
| OpenAPI document drifts from endpoint behavior | Medium | Extend `AuthNetOpenApiTests` for every new route, schema, and cookie security requirement. |
| Scope expands into token auth or broader MFA policy | High | Keep token auth, required-MFA policy, admin reset, SMS/email OTP, and passkeys out of this slice. |

## Open Questions

- Resolved: setup-start does not reset an existing authenticator key; it ensures one exists and returns the current setup data.
- Resolved: recovery-code regeneration requires MFA to already be enabled.
- Resolved: MFA challenge success returns `AuthNetApiResult`; clients can call `GET /session` for current session state.
