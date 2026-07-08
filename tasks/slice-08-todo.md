# Slice 08 Todo: MFA Foundation

## Task 1: Add MFA route conventions

**Description:** Register built-in MFA Razor Pages under the existing AuthNet account route prefix.

**Acceptance criteria:**
- [x] `/auth/mfa` maps to the MFA management page.
- [x] `/auth/mfa/setup` maps to authenticator setup.
- [x] `/auth/mfa/recovery-codes` maps to recovery-code display.
- [x] `/auth/mfa/disable` maps to MFA disable.
- [x] `/auth/login/mfa` and `/auth/login/recovery-code` map to anonymous login challenge pages.

**Verification:**
- [x] Focused route tests cover the new MFA URLs.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetRouteTests.cs`

**Estimated scope:** S

## Task 2: Update password login for MFA challenge

**Description:** Change local password sign-in so users with enabled two-factor auth are redirected to an MFA challenge when Identity returns `RequiresTwoFactor`.

**Acceptance criteria:**
- [x] Password login redirects MFA-enabled users to `/auth/login/mfa`.
- [x] Return URL and remember-me intent are preserved for the challenge.
- [x] Non-MFA users still sign in normally.

**Verification:**
- [x] Focused login tests cover MFA and non-MFA branches.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/Login.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetMfaTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetEmailFlowTests.cs` or existing login coverage if needed

**Estimated scope:** M

## Task 3: Add MFA login challenge page

**Description:** Add a Razor Page where a pending two-factor user enters an authenticator-app code to complete sign-in.

**Acceptance criteria:**
- [x] Page requires a pending two-factor sign-in user.
- [x] Valid authenticator code signs the user in.
- [x] Invalid code shows a validation error.
- [x] Lockout behavior follows Identity defaults.

**Verification:**
- [x] Focused integration test signs in an MFA-enabled user with a generated valid token.

**Dependencies:** Task 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/LoginWithMfa.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/LoginWithMfa.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetMfaTests.cs`

**Estimated scope:** M

## Task 4: Add authenticated MFA management page

**Description:** Add a signed-in account page showing MFA status and links to setup, recovery codes, and disable actions.

**Acceptance criteria:**
- [x] Authenticated users can view MFA enabled/disabled state.
- [x] Anonymous users are challenged.
- [x] Profile page links to MFA management.
- [x] Page does not expose secrets or recovery codes.

**Verification:**
- [x] Focused integration test verifies route protection and status rendering.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/Mfa.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/Mfa.cshtml.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/Profile.cshtml`
- `tests/AuthNet.Tests/Integration/AuthNetMfaTests.cs`

**Estimated scope:** M

## Task 5: Add authenticator setup and verification page

**Description:** Add an authenticated page that shows the authenticator setup key, accepts a TOTP code, enables MFA, and generates recovery codes.

**Acceptance criteria:**
- [x] Page resets or creates an authenticator key when needed.
- [x] Page displays a manual setup key and otpauth URI text/link.
- [x] Valid TOTP code enables MFA.
- [x] Invalid TOTP code does not enable MFA and shows a validation error.

**Verification:**
- [x] Focused integration test enables MFA using a token generated from the authenticator key.

**Dependencies:** Task 4

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/MfaSetup.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/MfaSetup.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetMfaTests.cs`

**Estimated scope:** M

## Task 6: Add recovery-code display and recovery-code login

**Description:** Show recovery codes after setup and support recovery-code sign-in when an MFA-enabled user cannot use their authenticator app.

**Acceptance criteria:**
- [x] Recovery codes are generated after successful MFA setup.
- [x] Recovery-code display page is authenticated and does not regenerate codes on refresh.
- [x] Pending MFA users can sign in with a valid recovery code.
- [x] Used or invalid recovery codes are rejected by Identity.

**Verification:**
- [x] Focused integration tests cover recovery-code display and sign-in.

**Dependencies:** Tasks 3 and 5

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/MfaRecoveryCodes.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/MfaRecoveryCodes.cshtml.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/LoginWithRecoveryCode.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/LoginWithRecoveryCode.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetMfaTests.cs`

**Estimated scope:** M

## Task 7: Add MFA disable flow

**Description:** Add an authenticated page or POST action that disables MFA for the current user.

**Acceptance criteria:**
- [x] Authenticated user can disable their own MFA.
- [x] Disabling clears MFA enabled state through Identity APIs.
- [x] Anonymous users are challenged.
- [x] User can sign in with password only after disabling MFA.

**Verification:**
- [x] Focused integration test enables MFA, disables it, and verifies password-only login works.

**Dependencies:** Task 5

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/MfaDisable.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/MfaDisable.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetMfaTests.cs`

**Estimated scope:** M

## Task 8: Expand integration test helpers and focused MFA tests

**Description:** Add reusable test-host helpers for enabling MFA, generating current authenticator tokens, extracting recovery codes, and asserting sign-in state.

**Acceptance criteria:**
- [x] Tests can generate valid TOTP codes without hardcoding time-sensitive values.
- [x] Tests cover setup, challenge login, recovery-code login, disable, and route protection.
- [x] Existing login, profile, and external-login tests still pass.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetMfaTests`

**Dependencies:** Tasks 1 through 7

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetMfaTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHtml.cs`

**Estimated scope:** M

## Task 9: Update user, developer, architecture, roadmap, and context docs

**Description:** Document MFA setup, login challenge, recovery codes, limitations, and current architecture decisions.

**Acceptance criteria:**
- [x] User docs list MFA routes and explain authenticator-app setup.
- [x] Developer quick start includes focused MFA test command.
- [x] Architecture context records MFA as active implemented scope.
- [x] Roadmap marks MFA as implemented when complete.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 08 status.

**Verification:**
- [x] Manual read-through for stale references to MFA as deferred.

**Dependencies:** Tasks 1 through 8

**Files likely touched:**
- `docs/users/account-pages.md`
- `docs/users/getting-started.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/mvp-roadmap.md`
- `docs/slice-08/mfa-foundation.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 10: Final verification and local review

**Description:** Run full verification and review for security, scope, and documentation drift.

**Acceptance criteria:**
- [x] `.\scripts\verify.ps1` passes.
- [x] `git diff --check` passes.
- [x] MFA uses Identity primitives and does not add custom token storage.
- [x] No SMS/email OTP, passkey, API/JWT, SPA, or global required-MFA policy is introduced.

**Verification:**
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 9

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 11: Commit Slice 08

**Description:** Commit the completed MFA foundation slice.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 08 changes.
- [x] Commit message describes MFA foundation.
- [x] Working tree is clean except unrelated user-owned changes.

**Verification:**
- [ ] `git status --short`
- [ ] `git log --oneline -1`

**Dependencies:** Task 10

**Files likely touched:** Git metadata only

**Estimated scope:** XS
