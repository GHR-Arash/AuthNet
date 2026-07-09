# Slice 19 Plan: SPA Invitation Acceptance JSON

## Overview

Add same-origin SPA JSON support for accepting existing account invitations. The slice should let browser SPA clients inspect an invitation token, submit local credentials for a valid pending invitation, and receive consistent JSON outcomes while reusing the existing invitation persistence, token hashing, expiration, user creation, email-confirmation, and cookie sign-in behavior.

## Scope

In scope:

- Anonymous JSON endpoint to inspect invitation acceptance state from a raw token.
- Anonymous JSON endpoint to accept an invitation with username, password, and confirmation password.
- Reuse existing invitation token validation, expiration, single-use, duplicate-user, and accepted-state rules.
- Create a local Identity user, mark the invited email confirmed, mark the invitation accepted, and sign in with the existing Identity application cookie.
- Add OpenAPI coverage for the new invitation JSON endpoints.
- Add focused integration tests and sample SPA discoverability.

Out of scope:

- Admin invitation JSON APIs.
- Invitation resend, cancel, delete, or bulk invitation workflows.
- Role assignment during invite acceptance.
- Cross-origin SPA auth, JWT access tokens, and refresh tokens.
- Changing the existing Razor invitation acceptance route behavior.

## Architecture Decisions

- Keep the endpoints same-origin and cookie-based under the existing SPA API root, `/auth/api` by default.
- Reuse the current invitation persistence model and token-hash storage; raw invitation tokens remain request-only values and are never persisted.
- Keep invitation acceptance independent from public registration being enabled or disabled.
- Reuse the existing invitation persistence, token, and Identity acceptance rules for the JSON flow while preserving the Razor flow behavior with regression coverage.
- Return field-addressable validation errors where the failure is input-related, and stable JSON status values for token-state outcomes.

## Candidate Routes

Default routes under `AccountRoutePrefix=/auth`:

```text
GET  /auth/api/invitations/accept?token={token}
POST /auth/api/invitations/accept
```

The GET endpoint should return safe invitation state such as validity, email, expiration, and status, without exposing token hashes or internal persistence details.

The POST endpoint should accept the raw invitation token plus local credential fields and complete the same account-creation outcome as the Razor acceptance page.

## Response Shape

Use stable DTOs in `AuthNet.Api`:

- `AuthNetInvitationAcceptanceStatusResponse`
- `AuthNetAcceptInvitationRequest`
- `AuthNetAcceptInvitationResponse`

Suggested statuses:

- `Valid`
- `InvalidToken`
- `Expired`
- `AlreadyAccepted`
- `ExistingUser`
- `Accepted`
- `ValidationFailed`

The final naming can follow existing API DTO conventions if the codebase already has a better pattern.

## Task List

### Phase 1: Contracts and Shared Logic

- [x] Task 1: Add invitation acceptance request and response contracts.
- [x] Task 2: Reuse invitation token inspection and acceptance rules while preserving Razor behavior.
- [x] Task 3: Extend the SPA account service surface with invitation inspection and acceptance methods.

### Checkpoint: Foundation

- [x] Solution builds.
- [x] Existing Razor invitation tests still pass.
- [x] Existing SPA API, account workflow, MFA, external-login, and OpenAPI tests still pass.

### Phase 2: JSON Endpoints

- [x] Task 4: Implement invitation acceptance status endpoint.
- [x] Task 5: Implement invitation acceptance completion endpoint.
- [x] Task 6: Add focused SPA invitation API tests.

### Checkpoint: Invitation API

- [x] Valid pending invitation can be inspected.
- [x] Invalid, expired, and already accepted invitations return stable JSON outcomes.
- [x] Valid invitation acceptance creates the user, confirms email, marks invitation accepted, and signs in through the application cookie.
- [x] Invalid credentials return field-addressable JSON validation errors.

### Phase 3: OpenAPI, Sample, Docs, and Verification

- [x] Task 7: Update endpoint mapping and OpenAPI document/tests.
- [x] Task 8: Update sample `/Spa` page for invitation status and acceptance smoke workflows.
- [x] Task 9: Update user/developer docs and compact context files.
- [x] Task 10: Run focused and full verification.

### Checkpoint: Complete

- [x] Focused Slice 19 SPA invitation API tests pass.
- [x] Existing Razor invitation tests pass.
- [x] Existing SPA API, account workflow, MFA, external-login, and OpenAPI tests pass.
- [x] Full verification passes.
- [x] Slice 19 artifacts are complete and named with `slice-19`.
- [x] Admin JSON APIs, invitation resend/cancel, JWT, refresh tokens, and cross-origin SPA auth remain explicitly deferred.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Razor and JSON invitation flows drift | High | Share token inspection and acceptance logic where practical, and keep existing Razor tests in the focused verification set. |
| Raw tokens leak into logs, persistence, or audit metadata | High | Preserve existing hash-only persistence and assert responses do not include token hashes or raw acceptance URLs. |
| Acceptance endpoint weakens registration policy | Medium | Add tests proving invitation acceptance works independently from public registration and does not enable open registration. |
| Cookie sign-in behavior differs from Razor flow | Medium | Test accepted invitation response plus authenticated session state after POST. |
| Response statuses expose account enumeration details | Medium | Match existing invitation behavior and only reveal state tied to possession of a raw invitation token. |

## Open Questions

- Should the status endpoint return the invited email before acceptance, or only a masked email? Default plan: return the invited email because possession of the invitation token is already the authorization factor and the Razor page likely displays it.
- Should `DisplayName` be accepted during invitation completion? Default plan: only username/password fields unless existing Razor acceptance already supports more fields.
- Should the sample SPA provide a manual token textbox only, or parse `token` from the page URL? Default plan: support both if it stays small.
