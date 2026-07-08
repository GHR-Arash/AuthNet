# Slice 16 Todo: SPA Account Workflow Completion

## Task 1: Add request and response contracts

**Description:** Add public API DTOs for reset password completion, email confirmation completion, profile update, and change password.

**Acceptance criteria:**
- [x] `AuthNetResetPasswordRequest` exists with required email, code, and new password fields.
- [x] `AuthNetConfirmEmailRequest` exists with required user id and code fields.
- [x] `AuthNetUpdateProfileRequest` exists and exposes only allowed user-owned profile fields.
- [x] `AuthNetChangePasswordRequest` exists with current password and new password fields.
- [x] Public DTOs have XML summary comments and data annotations.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaRequests.cs`
- `src/AuthNet.Api/AuthNetApiResult.cs`

**Estimated scope:** S

## Task 2: Extend SPA account service surface

**Description:** Add service methods to `IAuthNetSpaAccountService` and the implementation for the four new workflows, initially wired enough to compile before endpoint mapping.

**Acceptance criteria:**
- [x] Interface includes reset password completion.
- [x] Interface includes email confirmation completion.
- [x] Interface includes profile update.
- [x] Interface includes change password.
- [x] Service implementation compiles and uses existing Identity managers.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Api/IAuthNetSpaAccountService.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`

**Estimated scope:** M

## Task 3: Implement reset-password endpoint

**Description:** Add `POST /auth/api/reset-password` to complete password reset using existing Identity reset-token semantics.

**Acceptance criteria:**
- [x] Endpoint accepts reset token and new password.
- [x] Valid reset token changes the user's password.
- [x] Invalid token returns a generic failure response.
- [x] Password policy errors return JSON errors.
- [x] Endpoint does not require authentication.

**Verification:**
- [x] Focused tests cover valid reset, invalid token, and weak password.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaAccountWorkflowTests.cs`

**Estimated scope:** M

## Task 4: Implement confirm-email endpoint

**Description:** Add `POST /auth/api/confirm-email` to complete email confirmation using existing Identity confirmation-token semantics.

**Acceptance criteria:**
- [x] Endpoint accepts user id and confirmation token.
- [x] Valid token confirms email.
- [x] Invalid token returns a generic failure response.
- [x] Unknown user returns a safe failure response.
- [x] Endpoint does not require authentication.

**Verification:**
- [x] Focused tests cover valid confirmation, invalid token, and unknown user.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaAccountWorkflowTests.cs`

**Estimated scope:** M

## Task 5: Add anonymous completion-flow tests

**Description:** Add focused integration tests that exercise reset-password and confirm-email through the same tokens emitted by existing account flows where practical.

**Acceptance criteria:**
- [x] Registration or resend-confirmation token can be used by JSON confirm-email endpoint.
- [x] Forgot-password token can be used by JSON reset-password endpoint.
- [x] Invalid tokens fail safely.
- [x] Successful reset allows login with the new password.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests`

**Dependencies:** Tasks 3 and 4

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetSpaAccountWorkflowTests.cs`

**Estimated scope:** M

## Task 6: Implement profile update endpoint

**Description:** Add `PUT /auth/api/profile` for authenticated users to update allowed profile fields and receive the updated profile response.

**Acceptance criteria:**
- [x] Endpoint requires an authenticated cookie session.
- [x] Endpoint allows updating display name.
- [x] Endpoint allows updating phone number if included in the DTO.
- [x] Endpoint does not allow updating email, username, roles, email-confirmed state, or MFA state.
- [x] Successful response returns updated `AuthNetProfileResponse`.

**Verification:**
- [x] Focused tests cover anonymous rejection and authenticated profile update.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaAccountWorkflowTests.cs`

**Estimated scope:** M

## Task 7: Implement change-password endpoint

**Description:** Add `POST /auth/api/change-password` for authenticated users to change local account password.

**Acceptance criteria:**
- [x] Endpoint requires an authenticated cookie session.
- [x] Valid current password and valid new password change the password.
- [x] Incorrect current password returns JSON failure.
- [x] Weak new password returns JSON password-policy errors.
- [x] Existing session remains coherent after successful password change.

**Verification:**
- [x] Focused tests cover anonymous rejection, successful change, wrong current password, and weak new password.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaAccountWorkflowTests.cs`

**Estimated scope:** M

## Task 8: Add authenticated update-flow tests

**Description:** Complete focused integration coverage for profile update and change password behavior.

**Acceptance criteria:**
- [x] Tests prove profile update persists and `GET /auth/api/profile` reflects changes.
- [x] Tests prove profile update cannot be used anonymously.
- [x] Tests prove password change affects subsequent login.
- [x] Tests prove password change cannot be used anonymously.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests`

**Dependencies:** Tasks 6 and 7

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetSpaAccountWorkflowTests.cs`

**Estimated scope:** S

## Task 9: Update OpenAPI document and tests

**Description:** Update `/auth/api/openapi.json` to describe all new Slice 16 endpoints, request DTOs, response schemas, status codes, and cookie security requirements.

**Acceptance criteria:**
- [x] OpenAPI document includes `POST /auth/api/reset-password`.
- [x] OpenAPI document includes `POST /auth/api/confirm-email`.
- [x] OpenAPI document includes `PUT /auth/api/profile`.
- [x] OpenAPI document includes `POST /auth/api/change-password`.
- [x] OpenAPI document includes new request schemas.
- [x] OpenAPI tests cover all new paths and schemas.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`

**Dependencies:** Tasks 1, 3, 4, 6, and 7

**Files likely touched:**
- `src/AuthNet.Api/AuthNetOpenApiDocumentBuilder.cs`
- `tests/AuthNet.Tests/Integration/AuthNetOpenApiTests.cs`

**Estimated scope:** M

## Task 10: Update sample SPA discoverability

**Description:** Add minimal sample `/Spa` controls or links for reset password, confirm email, profile update, and change password workflows without turning the sample into a full frontend app.

**Acceptance criteria:**
- [x] Sample page exposes profile update and change password calls for signed-in users.
- [x] Sample page exposes text inputs for reset/confirm token workflows or links to documented API routes.
- [x] Existing session/login/logout/profile controls remain intact.
- [x] OpenAPI link remains visible.

**Verification:**
- [x] Build succeeds.
- [x] Manual sample route check documented.

**Dependencies:** Tasks 3, 4, 6, 7, and 9

**Files likely touched:**
- `samples/AuthNet.SampleHost/Pages/Spa.cshtml`
- `docs/developer/quick-start.md`

**Estimated scope:** M

## Task 11: Update docs and context

**Description:** Update user docs, developer docs, architecture context, next-iteration context, and working memory to reflect the completed SPA account workflows.

**Acceptance criteria:**
- [x] User guide lists all new SPA API routes.
- [x] Configuration docs mention unchanged route-prefix behavior.
- [x] Developer quick start lists focused Slice 16 tests.
- [x] `docs/architecture-context.md` records completed SPA account workflows.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 16 when complete.
- [x] Docs clearly state JWT, MFA JSON, admin JSON, and external-login JSON remain deferred.

**Verification:**
- [x] Manual doc read-through confirms no stale guidance says these SPA workflows are unavailable.

**Dependencies:** Tasks 1 through 10

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`
- `tasks/slice-16-plan.md`
- `tasks/slice-16-todo.md`

**Estimated scope:** S

## Task 12: Run focused and full verification

**Description:** Verify the Slice 16 SPA account workflow completion and prepare it for commit.

**Acceptance criteria:**
- [x] Focused Slice 16 workflow tests pass.
- [x] Existing SPA API tests pass.
- [x] Existing OpenAPI tests pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 16 changes plus pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 11

**Files likely touched:**
- `tasks/slice-16-plan.md`
- `tasks/slice-16-todo.md`

**Estimated scope:** S
