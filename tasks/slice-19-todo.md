# Slice 19 Todo: SPA Invitation Acceptance JSON

## Task 1: Add invitation acceptance request and response contracts

**Description:** Add public DTOs for SPA invitation status and acceptance under `AuthNet.Api`, following existing XML comments, data annotations, and response naming conventions.

**Acceptance criteria:**

- [x] Status response includes stable invitation status and safe display fields.
- [x] Acceptance request includes token, username, password, and confirm password.
- [x] Acceptance response includes stable status, message, email, user id where applicable, and session outcome where appropriate.
- [x] DTOs do not expose token hashes, raw acceptance URLs, or persistence internals.

**Verification:**

- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**

- `src/AuthNet.Api`

**Estimated scope:** Small

## Task 2: Reuse invitation inspection and acceptance decisions

**Description:** Implement the JSON flow using the same invitation token lookup, state classification, and acceptance rules as the Razor flow, while keeping Razor behavior unchanged and covered by regression tests.

**Acceptance criteria:**

- [x] JSON logic can classify valid, invalid, expired, already accepted, and existing-user invitation states.
- [x] JSON acceptance path creates the user, confirms email, marks the invitation accepted, and signs in when requested by the flow.
- [x] Existing Razor invitation acceptance behavior remains unchanged from a user perspective.
- [x] Raw tokens remain request-only and are not persisted.

**Verification:**

- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests`

**Dependencies:** Task 1

**Files likely touched:**

- `src/AuthNet.Core`
- `src/AuthNet.UI.Razor`
- `src/AuthNet.Persistence.Postgres`
- `tests/AuthNet.Tests`

**Estimated scope:** Medium

## Task 3: Extend the SPA account service surface

**Description:** Add service methods for invitation status inspection and invitation acceptance so endpoint mapping remains thin and follows the existing SPA API architecture.

**Acceptance criteria:**

- [x] Service exposes an async invitation status method.
- [x] Service exposes an async invitation acceptance method.
- [x] Service methods support cancellation where they perform async work.
- [x] Service maps Identity validation failures into existing JSON error conventions.

**Verification:**

- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Tasks 1, 2

**Files likely touched:**

- `src/AuthNet.Api`
- `src/AuthNet.AspNetCore`

**Estimated scope:** Medium

## Task 4: Implement invitation acceptance status endpoint

**Description:** Map an anonymous GET endpoint under the SPA API root to inspect invitation state for a supplied raw token.

**Acceptance criteria:**

- [x] Missing token returns a validation failure.
- [x] Valid pending invitation returns a success/status response.
- [x] Invalid, expired, and already accepted tokens return stable JSON status outcomes.
- [x] Endpoint does not require authentication and does not expose sensitive internals.

**Verification:**

- [x] Focused status endpoint tests pass once Task 6 exists.

**Dependencies:** Task 3

**Files likely touched:**

- `src/AuthNet.Api`
- `tests/AuthNet.Tests`

**Estimated scope:** Small

## Task 5: Implement invitation acceptance completion endpoint

**Description:** Map an anonymous POST endpoint under the SPA API root that accepts a valid invitation and creates a local account using the existing Identity application cookie.

**Acceptance criteria:**

- [x] Valid pending invitation plus valid credentials creates the user.
- [x] Created user's invited email is confirmed.
- [x] Invitation is marked accepted and cannot be reused.
- [x] User is signed in with the application cookie after successful acceptance.
- [x] Invalid credentials return field-addressable validation errors.
- [x] Existing-user, invalid-token, expired-token, and already-accepted states return stable JSON outcomes without creating a user.

**Verification:**

- [x] Focused acceptance endpoint tests pass once Task 6 exists.

**Dependencies:** Task 4

**Files likely touched:**

- `src/AuthNet.Api`
- `tests/AuthNet.Tests`

**Estimated scope:** Medium

## Task 6: Add focused SPA invitation API tests

**Description:** Add integration tests covering the status and completion endpoints, including success, token-state failures, validation failures, and session state after acceptance.

**Acceptance criteria:**

- [x] Tests cover valid invitation status.
- [x] Tests cover invalid, expired, already accepted, reused, and existing-user states.
- [x] Tests cover successful acceptance, email confirmation, invitation accepted state, and authenticated session.
- [x] Tests cover invalid password/confirmation validation.
- [x] Existing Razor invitation tests still pass.

**Verification:**

- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter "AuthNetSpaInvitationApiTests|AuthNetInvitationTests"`

**Dependencies:** Tasks 4, 5

**Files likely touched:**

- `tests/AuthNet.Tests`

**Estimated scope:** Medium

## Task 7: Update OpenAPI document and tests

**Description:** Add the new invitation endpoints and schemas to the AuthNet-owned SPA OpenAPI JSON document.

**Acceptance criteria:**

- [x] OpenAPI document includes both Slice 19 routes.
- [x] OpenAPI document includes request and response schemas.
- [x] Invitation endpoints are documented as anonymous same-origin cookie workflow endpoints.
- [x] OpenAPI tests cover paths, schemas, and response metadata.

**Verification:**

- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`

**Dependencies:** Tasks 4, 5

**Files likely touched:**

- `src/AuthNet.Api`
- `tests/AuthNet.Tests`

**Estimated scope:** Medium

## Task 8: Update sample SPA discoverability

**Description:** Extend the sample `/Spa` page with a minimal manual smoke path for invitation token status and acceptance.

**Acceptance criteria:**

- [x] Sample page can inspect an invitation token.
- [x] Sample page can submit invitation acceptance credentials.
- [x] Existing SPA controls remain intact.
- [x] OpenAPI link remains visible.

**Verification:**

- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] Manual sample route check documented in this task when completed: sample `/Spa` page builds with invitation token inspection and acceptance controls.

**Dependencies:** Tasks 4, 5

**Files likely touched:**

- `samples/AuthNet.SampleHost`

**Estimated scope:** Small

## Task 9: Update docs and context

**Description:** Update user/developer docs and compact project memory to record Slice 19 scope and keep deferred work explicit.

**Acceptance criteria:**

- [x] User guide lists the SPA invitation acceptance JSON routes.
- [x] Configuration docs mention unchanged route-prefix behavior.
- [x] Developer quick start lists focused Slice 19 tests.
- [x] `docs/architecture-context.md` records completed SPA invitation acceptance when implementation is complete.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 19 when complete.
- [x] Docs clearly state admin JSON APIs, invitation resend/cancel, JWT, refresh tokens, and cross-origin SPA auth remain deferred.

**Verification:**

- [x] Manual doc read-through confirms no stale guidance says invitation acceptance JSON is unavailable after implementation.

**Dependencies:** Tasks 6, 7, 8

**Files likely touched:**

- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** Medium

## Task 10: Run focused and full verification

**Description:** Run the focused Slice 19 verification set, related regression tests, full local verification, and final diff review.

**Acceptance criteria:**

- [x] Focused Slice 19 SPA invitation API tests pass.
- [x] Existing Razor invitation tests pass.
- [x] Existing SPA API, account workflow, MFA, external-login, and OpenAPI tests pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 19 changes plus pre-existing unrelated user changes.

**Verification:**

- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaInvitationApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaMfaApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaExternalLoginApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1-9

**Files likely touched:**

- All Slice 19 files

**Estimated scope:** Small
