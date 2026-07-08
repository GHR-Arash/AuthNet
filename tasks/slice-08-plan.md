# Slice 08 Plan: MFA Foundation

## Overview

Slice 08 implements the next Should Have roadmap item: MFA. The goal is a minimal server-rendered multi-factor authentication foundation using ASP.NET Core Identity primitives, starting with authenticator-app TOTP. This slice should let users enable MFA from their profile, complete MFA during local password sign-in, use recovery codes, and disable MFA. It should not introduce SMS delivery, WebAuthn/passkeys, API/JWT flows, SPA flows, or custom token storage.

## Scope

In scope:

- Add built-in Razor Pages for authenticator-app MFA setup, verification, recovery-code display, MFA login challenge, recovery-code login, and disabling MFA.
- Add account routes under the existing AuthNet route prefix.
- Use ASP.NET Core Identity's authenticator key, authenticator token provider, two-factor sign-in, and recovery-code APIs.
- Update local password login to redirect to an MFA challenge when `PasswordSignInAsync` returns `RequiresTwoFactor`.
- Let authenticated users enable MFA only after proving a valid authenticator code.
- Generate and show recovery codes after setup.
- Let users sign in with a recovery code when MFA is required.
- Let users disable MFA from the authenticated account UI.
- Add integration tests for setup, login challenge, recovery-code login, disable, and route protection.
- Document the feature and remaining MFA limitations.

Out of scope:

- SMS, email OTP, push, WebAuthn/passkeys, and hardware keys.
- Remember-this-browser behavior.
- Admin-managed MFA reset.
- Organization-wide required-MFA policy.
- Backup/recovery-code regeneration after setup unless needed for a complete minimal flow.
- API/JWT/SPA MFA endpoints.
- QR-code image generation; display the manual setup key and otpauth URI text/link first.

## Proposed Routes

With `AccountRoutePrefix` set to `/auth`, routes should be:

- `/auth/mfa`
- `/auth/mfa/setup`
- `/auth/mfa/recovery-codes`
- `/auth/mfa/disable`
- `/auth/login/mfa`
- `/auth/login/recovery-code`

Authenticated management routes:

- `/auth/mfa`
- `/auth/mfa/setup`
- `/auth/mfa/recovery-codes`
- `/auth/mfa/disable`

Anonymous login-challenge routes:

- `/auth/login/mfa`
- `/auth/login/recovery-code`

## Architecture Decisions

- Use ASP.NET Core Identity MFA primitives instead of custom token generation or custom persistence.
- Start with authenticator-app TOTP because Identity already supports it without external delivery infrastructure.
- Keep MFA as account UI in `AuthNet.UI.Razor`; service registration remains in `AuthNet.AspNetCore`.
- Keep routes under the existing account route prefix rather than adding a separate MFA prefix.
- Do not require MFA globally in this slice; users opt in per account.
- Recovery codes should be shown once after setup and consumed through Identity APIs during login.
- External login MFA behavior remains unchanged unless Identity returns a two-factor requirement for a local sign-in path.

## Task List

### Phase 1: Routing and Login Challenge

- [x] Task 1: Add MFA route conventions.
- [x] Task 2: Update password login to redirect on `RequiresTwoFactor`.
- [x] Task 3: Add MFA login challenge page.

### Checkpoint: Login Challenge

- [x] Focused MFA login tests pass.
- [x] Existing login tests still pass.
- [x] Non-MFA users still sign in normally.

### Phase 2: MFA Setup and Recovery

- [x] Task 4: Add authenticated MFA management page.
- [x] Task 5: Add authenticator setup and verification page.
- [x] Task 6: Add recovery-code display and recovery-code login.

### Checkpoint: Setup Flow

- [x] User can enable TOTP MFA from account UI.
- [x] User receives recovery codes after setup.
- [x] MFA-enabled user can sign in with authenticator code or recovery code.

### Phase 3: Disable, Tests, and Docs

- [x] Task 7: Add MFA disable flow.
- [x] Task 8: Expand integration test helpers and focused MFA tests.
- [x] Task 9: Update user, developer, architecture, roadmap, and context docs.
- [x] Task 10: Final verification and local review.
- [x] Task 11: Commit Slice 08.

### Checkpoint: Complete

- [x] `.\scripts\verify.ps1` passes.
- [x] MFA is implemented through ASP.NET Core Identity primitives.
- [x] No SMS/email OTP, passkey, API/JWT, SPA, or global required-MFA policy is introduced.
- [x] Slice 08 artifacts are complete and named with `slice-08`.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| TOTP tests are brittle because codes are time-based | High | Use Identity's registered token provider in tests to generate valid current tokens instead of hardcoded codes. |
| Users enable MFA without recovery path | High | Generate and display recovery codes immediately after successful setup. |
| Login challenge breaks non-MFA sign-in | High | Add regression tests for normal local login and MFA login. |
| Scope grows into multiple MFA providers | Medium | Limit Slice 08 to authenticator-app TOTP and recovery codes. |
| QR code dependency adds package/UI complexity | Medium | Start with manual setup key and otpauth URI text/link; defer generated QR images. |
| External login two-factor behavior becomes unclear | Medium | Keep external login unchanged in this slice and document local password MFA scope. |

## Open Questions

- Should the first setup UI include an otpauth URI link, manual key only, or generated QR image?
- Should recovery-code regeneration be included in Slice 08 or deferred to a follow-up?
- Should users be required to confirm email before enabling MFA?

## Recommended Default

Implement authenticator-app TOTP with manual key plus otpauth URI text/link, include recovery-code display after setup and recovery-code login, and defer QR image generation and recovery-code regeneration unless implementation proves trivial.
