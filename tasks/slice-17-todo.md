# Slice 17 Todo: SPA MFA JSON Workflows

## Task 1: Add MFA request and response contracts

**Description:** Add public API DTOs for MFA state, setup start, setup verification, recovery-code status/regeneration, MFA challenge login, and recovery-code login.

**Acceptance criteria:**
- [x] `AuthNetMfaStatusResponse` exists with enabled/authenticator/recovery-code state.
- [x] `AuthNetMfaSetupStartResponse` exists with shared key and authenticator URI.
- [x] `AuthNetMfaSetupVerifyRequest` exists with required authenticator code.
- [x] `AuthNetMfaSetupVerifyResponse` exists with enabled state and one-time recovery codes.
- [x] `AuthNetRecoveryCodesResponse` and `AuthNetRecoveryCodesRegenerateResponse` exist.
- [x] `AuthNetMfaChallengeRequest` and `AuthNetRecoveryCodeLoginRequest` exist.
- [x] Public DTOs have XML summary comments and data annotations.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaRequests.cs`
- `src/AuthNet.Api/AuthNetApiResult.cs`

**Estimated scope:** S

## Task 2: Extend SPA account service surface with MFA methods

**Description:** Add MFA service methods to `IAuthNetSpaAccountService` and the implementation scaffold before endpoint mapping.

**Acceptance criteria:**
- [x] Interface includes MFA status.
- [x] Interface includes setup start and setup verify.
- [x] Interface includes MFA disable.
- [x] Interface includes recovery-code status and regeneration.
- [x] Interface includes MFA challenge and recovery-code challenge login.
- [x] Service implementation compiles and uses existing Identity managers.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Api/IAuthNetSpaAccountService.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`

**Estimated scope:** M

## Task 3: Implement MFA status and setup-start behavior

**Description:** Add service behavior for reading the signed-in user's MFA state and starting authenticator setup by ensuring an authenticator key exists and returning the formatted shared key plus otpauth URI.

**Acceptance criteria:**
- [x] Anonymous users receive unauthorized behavior.
- [x] Authenticated users receive enabled/authenticator/recovery-code state.
- [x] Setup-start returns shared key and authenticator URI for authenticated users.
- [x] Setup-start does not reset an already enabled authenticator by default.
- [x] Authenticator URI issuer/email/secret format matches Razor MFA setup behavior.

**Verification:**
- [x] Focused tests cover anonymous rejection, status response, and setup-start response.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaMfaApiTests.cs`

**Estimated scope:** M

## Task 4: Implement setup verification and initial recovery-code generation

**Description:** Verify a submitted authenticator code using ASP.NET Core Identity, enable MFA, and return one-time recovery codes.

**Acceptance criteria:**
- [x] Endpoint requires an authenticated cookie session.
- [x] Valid authenticator code enables MFA.
- [x] Invalid authenticator code returns JSON failure.
- [x] Successful verification returns generated recovery codes exactly once in the response.
- [x] Password policy, lockout, and Identity security-stamp behavior remain owned by Identity.

**Verification:**
- [x] Focused tests cover valid setup verification, invalid code, enabled state, and recovery-code count.

**Dependencies:** Tasks 1 through 3

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaMfaApiTests.cs`

**Estimated scope:** M

## Task 5: Implement MFA disable and recovery-code status/regeneration

**Description:** Add service behavior for disabling user-owned MFA, reading recovery-code count, and regenerating recovery codes for MFA-enabled users.

**Acceptance criteria:**
- [x] Disable requires an authenticated cookie session.
- [x] Disable turns off MFA and resets the authenticator key like the Razor page.
- [x] Recovery-code status returns count only, not stored code values.
- [x] Regeneration requires MFA to be enabled.
- [x] Regeneration returns newly generated recovery codes in the response.

**Verification:**
- [x] Focused tests cover disable, status count, regeneration, and regeneration rejection when MFA is disabled.

**Dependencies:** Tasks 1, 2, and 4

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaMfaApiTests.cs`

**Estimated scope:** M

## Task 6: Map authenticated MFA management endpoints

**Description:** Add JSON endpoints for status, setup start, setup verify, disable, recovery-code status, and recovery-code regeneration.

**Acceptance criteria:**
- [x] `GET /auth/api/mfa` is mapped.
- [x] `POST /auth/api/mfa/setup/start` is mapped.
- [x] `POST /auth/api/mfa/setup/verify` is mapped.
- [x] `POST /auth/api/mfa/disable` is mapped.
- [x] `GET /auth/api/mfa/recovery-codes` is mapped.
- [x] `POST /auth/api/mfa/recovery-codes/regenerate` is mapped.
- [x] Authenticated endpoints return `401` for anonymous callers.

**Verification:**
- [x] Focused management endpoint tests pass.

**Dependencies:** Tasks 3 through 5

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaMfaApiTests.cs`

**Estimated scope:** M

## Task 7: Implement authenticator-code login challenge endpoint

**Description:** Add service behavior and route for completing an MFA-required password sign-in with a TOTP authenticator code.

**Acceptance criteria:**
- [x] `POST /auth/api/login/mfa` is mapped.
- [x] Endpoint uses `SignInManager.GetTwoFactorAuthenticationUserAsync`.
- [x] Valid authenticator code completes sign-in.
- [x] Missing challenge user returns unauthorized JSON.
- [x] Invalid code returns unauthorized JSON.
- [x] Locked-out challenge returns conflict JSON.

**Verification:**
- [x] Focused tests cover password login requiring MFA, valid MFA completion, missing challenge, invalid code, and locked-out state if practical.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaMfaApiTests.cs`

**Estimated scope:** M

## Task 8: Implement recovery-code login challenge endpoint

**Description:** Add service behavior and route for completing an MFA-required password sign-in with a recovery code.

**Acceptance criteria:**
- [x] `POST /auth/api/login/recovery-code` is mapped.
- [x] Endpoint uses `SignInManager.GetTwoFactorAuthenticationUserAsync`.
- [x] Valid recovery code completes sign-in.
- [x] Successful recovery-code login consumes one recovery code.
- [x] Missing challenge user returns unauthorized JSON.
- [x] Invalid recovery code returns unauthorized JSON.

**Verification:**
- [x] Focused tests cover valid recovery-code login, code consumption, missing challenge, and invalid code.

**Dependencies:** Tasks 1, 2, and 7

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaMfaApiTests.cs`

**Estimated scope:** M

## Task 9: Add focused Slice 17 test coverage

**Description:** Complete a dedicated `AuthNetSpaMfaApiTests` class covering management and challenge flows with the same Identity behavior already proven by Razor MFA tests.

**Acceptance criteria:**
- [x] Tests cover authenticated MFA status/setup/verify/disable.
- [x] Tests cover recovery-code count/regeneration.
- [x] Tests cover password login followed by JSON MFA completion.
- [x] Tests cover password login followed by JSON recovery-code completion.
- [x] Tests cover anonymous/missing challenge rejection.
- [x] Existing `AuthNetMfaTests` still pass.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter "AuthNetSpaMfaApiTests|AuthNetMfaTests"`

**Dependencies:** Tasks 3 through 8

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetSpaMfaApiTests.cs`

**Estimated scope:** M

## Task 10: Update OpenAPI document and tests

**Description:** Update `/auth/api/openapi.json` to describe Slice 17 MFA endpoints, request DTOs, response schemas, status codes, and cookie/two-factor challenge security expectations.

**Acceptance criteria:**
- [x] OpenAPI document includes all eight Slice 17 routes.
- [x] OpenAPI document includes all new request/response schemas.
- [x] Cookie-required management endpoints include the cookie security scheme.
- [x] Login challenge endpoints document `200`, `400`, `401`, and `409` where applicable.
- [x] OpenAPI tests cover paths, schemas, and security metadata.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`

**Dependencies:** Tasks 1, 6, 7, and 8

**Files likely touched:**
- `src/AuthNet.Api/AuthNetOpenApiDocumentBuilder.cs`
- `tests/AuthNet.Tests/Integration/AuthNetOpenApiTests.cs`

**Estimated scope:** M

## Task 11: Update sample SPA discoverability

**Description:** Add sample `/Spa` controls for the MFA management and challenge endpoints without turning the sample into a full frontend application.

**Acceptance criteria:**
- [x] Sample page can call MFA status.
- [x] Sample page can start setup and verify an authenticator code.
- [x] Sample page can disable MFA.
- [x] Sample page can view recovery-code count and regenerate recovery codes.
- [x] Sample page can submit MFA and recovery-code login challenges after password login reports `RequiresTwoFactor`.
- [x] Existing login/logout/session/profile controls remain intact.

**Verification:**
- [x] Build succeeds.
- [x] Manual sample route check documented.

**Dependencies:** Tasks 6 through 10

**Files likely touched:**
- `samples/AuthNet.SampleHost/Pages/Spa.cshtml`
- `docs/developer/quick-start.md`

**Estimated scope:** M

## Task 12: Update docs and context

**Description:** Update user docs, developer docs, architecture context, next-iteration context, and working memory to reflect completed SPA MFA JSON workflows.

**Acceptance criteria:**
- [x] User guide lists the new SPA MFA JSON routes.
- [x] Configuration docs mention unchanged route-prefix behavior for MFA API routes.
- [x] Developer quick start lists focused Slice 17 tests.
- [x] `docs/architecture-context.md` records completed SPA MFA workflows.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 17 when complete.
- [x] Docs clearly state JWT, refresh tokens, admin MFA reset, required-MFA policy, SMS/email OTP, and passkeys remain deferred.

**Verification:**
- [x] Manual doc read-through confirms no stale guidance says SPA MFA workflows are unavailable after implementation.

**Dependencies:** Tasks 1 through 11

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`
- `tasks/slice-17-plan.md`
- `tasks/slice-17-todo.md`

**Estimated scope:** S

## Task 13: Run focused and full verification

**Description:** Verify the Slice 17 SPA MFA workflow completion and prepare it for commit.

**Acceptance criteria:**
- [x] Focused Slice 17 SPA MFA API tests pass.
- [x] Existing Razor MFA tests pass.
- [x] Existing SPA API tests pass.
- [x] Existing SPA account workflow tests pass.
- [x] Existing OpenAPI tests pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 17 changes plus pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaMfaApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetMfaTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 12

**Files likely touched:**
- `tasks/slice-17-plan.md`
- `tasks/slice-17-todo.md`

**Estimated scope:** S
