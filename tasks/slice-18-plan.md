# Slice 18 Plan: SPA External Login JSON Orchestration

## Overview

Slice 18 adds same-origin SPA JSON orchestration for external login and external account linking while preserving the existing Razor Pages external-login behavior and security rules. The slice should let a SPA discover configured external providers, initiate an external challenge, and complete the callback into JSON-safe account outcomes. It must keep the current safety posture: do not link local accounts by email alone, require verified provider email for auto-provisioning, and only allow external-login linking for an already authenticated user.

## Scope

In scope:

- Add request/response DTOs and service methods for:
  - External provider discovery.
  - External challenge start.
  - External callback completion.
  - Authenticated external-login linking.
- Add same-origin JSON endpoints under the existing `{AccountRoutePrefix}/api` root:
  - `GET /auth/api/external-providers`
  - `POST /auth/api/external-login/challenge`
  - `GET /auth/api/external-login/callback`
  - `POST /auth/api/external-login/link/challenge`
  - `GET /auth/api/external-login/link/callback`
- Preserve existing Razor Pages external-login route behavior.
- Reuse ASP.NET Core Identity external-login primitives and the existing external authentication cookie.
- Return JSON-safe outcomes for:
  - Successful sign-in of an already linked external account.
  - Successful auto-provisioning for a new verified external account.
  - Successful explicit linking for an already signed-in user.
  - Missing external login info.
  - Remote provider error.
  - Missing provider email.
  - Unverified provider email.
  - Existing local account safety rejection.
  - Duplicate external identity or failed `AddLoginAsync`.
- Update `/auth/api/openapi.json` for all new routes and schemas.
- Add focused integration tests around existing external-login safety rules.
- Update sample `/Spa` controls/links for external-login smoke workflows.
- Update user/developer docs, architecture context, next-iteration context, and `context.md`.

Out of scope:

- JWT access tokens and refresh tokens.
- Provider-specific Google/Microsoft helper packages.
- Cross-origin SPA auth/CORS policy management.
- Admin JSON APIs.
- Invitation acceptance JSON endpoint.
- Changing Razor Pages external-login route behavior.
- Adding multiple configured provider abstractions beyond currently registered authentication schemes/options.
- Full frontend redirect orchestration beyond returning a challenge response or challenge URL shape that works for same-origin clients.

## Architecture Decisions

- Keep implementation in `AuthNet.Api`, following the current SPA service boundary style.
- Add external-login methods to `IAuthNetSpaAccountService` unless the implementation becomes large enough to justify a package-internal external-login service.
- Use `SignInManager.GetExternalAuthenticationSchemesAsync()` or the configured OIDC option to expose provider metadata. Prefer framework-provided schemes where practical so future providers can appear without a new AuthNet options shape.
- Use `SignInManager.ConfigureExternalAuthenticationProperties` to initiate challenges, matching the Razor page.
- Keep challenge initiation as an HTTP result, not a fake JSON-only redirect. External authentication is browser-redirect based; the API endpoint may return a `ChallengeResult`/`IResult` that the browser follows, while OpenAPI documents the intent.
- Use a callback endpoint that returns JSON for callback completion when reached by the browser/test client. This endpoint should share the same safety logic as the Razor callback.
- Preserve `returnUrl` as optional input, but only allow local URLs. Non-local return URLs should fall back to `/` or be rejected with JSON failure.
- Do not auto-link external identities to existing local users by email. Existing local users must sign in first and use the explicit link flow.
- Auto-provision only when the provider returns an email claim and `email_verified` is true or `1`.
- Keep provider-specific claim mapping out of scope; the generic OIDC integration already requests email/profile scopes.

## Candidate Routes

With default `AccountRoutePrefix=/auth`:

- `GET /auth/api/external-providers`
- `POST /auth/api/external-login/challenge`
- `GET /auth/api/external-login/callback`
- `POST /auth/api/external-login/link/challenge`
- `GET /auth/api/external-login/link/callback`

Existing Razor routes remain unchanged:

- `/auth/external-login`
- `/signin-authnet-oidc` callback path for the configured OIDC handler.

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
- `GET /auth/api/mfa`
- `POST /auth/api/mfa/setup/start`
- `POST /auth/api/mfa/setup/verify`
- `POST /auth/api/mfa/disable`
- `GET /auth/api/mfa/recovery-codes`
- `POST /auth/api/mfa/recovery-codes/regenerate`
- `POST /auth/api/login/mfa`
- `POST /auth/api/login/recovery-code`
- `GET /auth/api/openapi.json`

## Response Shape

Proposed DTOs:

- `AuthNetExternalProviderResponse`
  - `name`
  - `displayName`
- `AuthNetExternalProvidersResponse`
  - `providers`
- `AuthNetExternalChallengeRequest`
  - `provider`
  - `returnUrl`
- `AuthNetExternalLoginCallbackResponse`
  - `status`
  - `message`
  - `returnUrl`
  - `email`
  - `provider`
  - `userId`
- `AuthNetExternalLinkCallbackResponse`
  - `status`
  - `message`
  - `returnUrl`
  - `provider`

Use stable status strings, for example:

- `signedIn`
- `provisioned`
- `linked`
- `alreadyLinked`
- `remoteError`
- `missingExternalInfo`
- `missingEmail`
- `unverifiedEmail`
- `existingLocalAccount`
- `externalLoginFailed`
- `linkFailed`

Use `AuthNetApiResult` or typed callback responses for failures. Default plan: callbacks return `200` with a typed failure status for user-actionable external-login outcomes, matching Razor behavior that renders a page with an error instead of returning raw HTTP errors. Validation and invalid provider inputs should return `400`.

## Task List

### Phase 1: Contracts and Shared External Login Logic

- [ ] Task 1: Add external-login request/response contracts.
- [ ] Task 2: Extract or centralize external-login callback logic so Razor and API can share safety decisions.
- [ ] Task 3: Extend the SPA account service surface with external-login methods.

### Checkpoint: Foundation

- [ ] Solution builds.
- [ ] Existing Razor external-login tests still pass.
- [ ] Existing SPA API, MFA, and OpenAPI tests still pass.

### Phase 2: Provider Discovery and Challenge Start

- [ ] Task 4: Implement external provider discovery.
- [ ] Task 5: Implement external login challenge initiation.
- [ ] Task 6: Implement authenticated external-link challenge initiation.

### Checkpoint: Challenge Flow

- [ ] Provider list reflects configured external providers.
- [ ] Unknown provider challenge is rejected.
- [ ] Login challenge uses the external authentication properties expected by Identity.
- [ ] Link challenge requires an authenticated application cookie.
- [ ] Return URL handling is local-only.

### Phase 3: Callback Completion and Safety Tests

- [ ] Task 7: Implement external login callback completion endpoint.
- [ ] Task 8: Implement authenticated external-link callback completion endpoint.
- [ ] Task 9: Add focused SPA external-login tests for safety and success outcomes.

### Checkpoint: Callback Flow

- [ ] Verified external account is provisioned and linked.
- [ ] Already linked external account signs in.
- [ ] Existing local account is not linked by email alone.
- [ ] Unverified or missing provider email is rejected.
- [ ] Authenticated user can link an external provider explicitly.
- [ ] Duplicate/existing external link is handled safely.

### Phase 4: OpenAPI, Sample, Docs, and Verification

- [ ] Task 10: Update endpoint mapping and OpenAPI document/tests.
- [ ] Task 11: Update sample `/Spa` page for external-login discovery/challenge smoke workflows.
- [ ] Task 12: Update docs, context, and planning artifacts.
- [ ] Task 13: Run focused and full verification.

### Checkpoint: Complete

- [ ] Focused Slice 18 SPA external-login tests pass.
- [ ] Existing Razor external-login tests pass.
- [ ] Existing SPA API, account workflow, MFA, and OpenAPI tests pass.
- [ ] Full verification passes.
- [ ] Slice 18 artifacts are complete and named with `slice-18`.
- [ ] JWT, refresh tokens, provider-specific packages, admin JSON APIs, and invitation acceptance JSON remain explicitly deferred.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| External auth challenge cannot be represented as plain JSON | High | Treat challenge initiation as browser-redirect orchestration and test the returned challenge/redirect behavior using ASP.NET TestHost where possible. |
| Razor and API callback behavior diverge | High | Extract shared callback decision logic or keep duplicated code minimal and covered by parallel Razor/API tests. |
| Email-only account linking regression | High | Add focused API tests mirroring `Existing_local_account_is_not_linked_by_email_alone`. |
| Return URL open redirect | High | Validate local return URLs before challenge/callback responses. |
| Provider discovery misses future providers | Medium | Prefer `GetExternalAuthenticationSchemesAsync()` over hardcoding only `AuthNetOptions.OpenIdConnect`. |
| OpenAPI document drifts from endpoints | Medium | Extend `AuthNetOpenApiTests` for every new route/schema/status. |

## Open Questions

- Should challenge endpoints return a framework challenge directly, or return a JSON payload containing a challenge URL that the SPA navigates to? Default plan: use framework challenge behavior because external providers require browser redirects.
- Should callback endpoints return JSON only, or redirect to a local SPA return URL with a status query string? Default plan: return JSON for `/auth/api/.../callback`; keep redirect behavior on the Razor callback.
- Should provider discovery expose only name/display name, or also a challenge URL? Default plan: expose name/display name only and keep challenge URL construction server-owned.
