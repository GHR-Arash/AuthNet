# Slice 18 Todo: SPA External Login JSON Orchestration

## Task 1: Add external-login request and response contracts

**Description:** Add public API DTOs for external provider discovery, challenge initiation, external login callback outcomes, and external link callback outcomes.

**Acceptance criteria:**
- [x] `AuthNetExternalProviderResponse` exists with provider name and display name.
- [x] `AuthNetExternalProvidersResponse` exists with provider collection.
- [x] `AuthNetExternalChallengeRequest` exists with provider and optional return URL.
- [x] External login callback response DTO exists with stable status, message, return URL, provider, email, and user id where applicable.
- [x] External link callback response DTO exists with stable status, message, return URL, and provider where applicable.
- [x] Public DTOs have XML summary comments and data annotations.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaRequests.cs`
- `src/AuthNet.Api/AuthNetApiResult.cs`

**Estimated scope:** S

## Task 2: Preserve external-login callback decision logic

**Description:** Implement API callback behavior that preserves the existing Razor page safety decisions without changing the Razor user experience.

**Acceptance criteria:**
- [x] Shared logic handles remote provider errors.
- [x] Shared logic handles missing external login info.
- [x] Shared logic signs in already linked external accounts.
- [x] Shared logic auto-provisions only verified-email external accounts.
- [x] Shared logic rejects email-only linking to existing local accounts.
- [x] Shared logic links external identities only for an authenticated current user.
- [x] Existing Razor page behavior remains unchanged from a user perspective.
- [x] API behavior is covered by tests that mirror the existing Razor external-login safety cases.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetExternalLoginTests`

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `src/AuthNet.Api/AuthNetApiResult.cs`

**Estimated scope:** M

## Task 3: Extend SPA account service surface with external-login methods

**Description:** Add provider discovery, challenge, callback, link challenge, and link callback methods to the SPA API service boundary or a package-internal external-login service.

**Acceptance criteria:**
- [x] Service exposes external provider discovery.
- [x] Service exposes external login challenge preparation.
- [x] Service exposes external login callback completion.
- [x] Service exposes authenticated link challenge preparation.
- [x] Service exposes authenticated link callback completion.
- [x] Service methods support cancellation where they perform async work.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/IAuthNetSpaAccountService.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `src/AuthNet.Api/AuthNetExternalLoginService.cs` or equivalent

**Estimated scope:** M

## Task 4: Implement external provider discovery endpoint

**Description:** Add `GET /auth/api/external-providers` so same-origin SPA clients can discover configured external login providers.

**Acceptance criteria:**
- [x] Endpoint returns a list of configured external providers.
- [x] Provider response includes stable provider name and display name.
- [x] Endpoint returns an empty list when no external provider is configured.
- [x] Endpoint does not require authentication.
- [x] Endpoint does not expose secrets, authority URLs, client IDs, or callback internals.

**Verification:**
- [x] Focused tests cover provider list with and without configured providers where practical.

**Dependencies:** Tasks 1 and 3

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaExternalLoginApiTests.cs`

**Estimated scope:** M

## Task 5: Implement external login challenge initiation

**Description:** Add `POST /auth/api/external-login/challenge` to initiate browser external login using Identity's external authentication properties.

**Acceptance criteria:**
- [x] Endpoint accepts provider and optional return URL.
- [x] Unknown provider is rejected with JSON validation/failure.
- [x] Local return URLs are preserved.
- [x] Non-local return URLs are rejected or safely replaced.
- [x] Endpoint initiates a framework external authentication challenge for valid providers.
- [x] Endpoint does not require authentication.

**Verification:**
- [x] Focused tests cover unknown provider and local/non-local return URL handling.
- [x] Challenge behavior is covered as far as ASP.NET TestHost permits.

**Dependencies:** Tasks 1, 3, and 4

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaExternalLoginApiTests.cs`

**Estimated scope:** M

## Task 6: Implement authenticated external-link challenge initiation

**Description:** Add `POST /auth/api/external-login/link/challenge` so a signed-in user can deliberately start provider linking.

**Acceptance criteria:**
- [x] Endpoint requires an authenticated application cookie.
- [x] Endpoint accepts provider and optional return URL.
- [x] Unknown provider is rejected.
- [x] Local return URLs are preserved.
- [x] Non-local return URLs are rejected or safely replaced.
- [x] Endpoint initiates a framework external authentication challenge for valid providers.

**Verification:**
- [x] Focused tests cover anonymous rejection, unknown provider, and challenge preparation.

**Dependencies:** Tasks 1, 3, and 4

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaExternalLoginApiTests.cs`

**Estimated scope:** M

## Task 7: Implement external login callback completion

**Description:** Add `GET /auth/api/external-login/callback` to complete external sign-in and provisioning with JSON-safe outcomes.

**Acceptance criteria:**
- [x] Existing linked external account signs in.
- [x] New verified external account is provisioned, email-confirmed, linked, and signed in.
- [x] Existing local account with same email is not linked by email alone.
- [x] Missing email is rejected safely.
- [x] Unverified email is rejected safely.
- [x] Missing external login info is rejected safely.
- [x] Remote provider error is returned as a JSON-safe outcome.
- [x] Local return URL is included in the response where appropriate.

**Verification:**
- [x] Focused tests mirror existing Razor external-login safety tests for API callback behavior.

**Dependencies:** Tasks 1 through 5

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaExternalLoginApiTests.cs`

**Estimated scope:** M

## Task 8: Implement external link callback completion

**Description:** Add `GET /auth/api/external-login/link/callback` to complete explicit external provider linking for an already authenticated user.

**Acceptance criteria:**
- [x] Endpoint requires or validates an authenticated current user before linking.
- [x] Authenticated user can link a new external provider identity.
- [x] Already linked provider identity is handled idempotently where possible.
- [x] Duplicate external identity or failed `AddLoginAsync` returns JSON-safe failure.
- [x] Current sign-in is refreshed after successful linking.
- [x] Local return URL is included in the response where appropriate.

**Verification:**
- [x] Focused tests cover explicit linking, duplicate/existing link behavior, and anonymous rejection.

**Dependencies:** Tasks 1 through 6

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `src/AuthNet.Api/AuthNetSpaAccountService.cs`
- `tests/AuthNet.Tests/Integration/AuthNetSpaExternalLoginApiTests.cs`

**Estimated scope:** M

## Task 9: Add focused Slice 18 external-login API tests

**Description:** Add `AuthNetSpaExternalLoginApiTests` covering provider discovery, callback safety outcomes, provisioning, sign-in, and explicit linking.

**Acceptance criteria:**
- [x] Tests cover verified external provisioning and linking.
- [x] Tests cover already linked external sign-in.
- [x] Tests cover unverified email rejection.
- [x] Tests cover missing email rejection if test host can simulate it.
- [x] Tests cover existing local account safety rejection.
- [x] Tests cover authenticated explicit linking.
- [x] Tests cover provider discovery and challenge validation.
- [x] Existing `AuthNetExternalLoginTests` still pass.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter "AuthNetSpaExternalLoginApiTests|AuthNetExternalLoginTests"`

**Dependencies:** Tasks 4 through 8

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetSpaExternalLoginApiTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** M

## Task 10: Update OpenAPI document and tests

**Description:** Update `/auth/api/openapi.json` to describe Slice 18 external-login endpoints, request DTOs, response schemas, status codes, and cookie security on link endpoints.

**Acceptance criteria:**
- [x] OpenAPI document includes all five Slice 18 routes.
- [x] OpenAPI document includes all new request/response schemas.
- [x] Link challenge/callback endpoints document cookie authentication where applicable.
- [x] Challenge endpoints document expected redirect/challenge behavior or documented response limitations.
- [x] OpenAPI tests cover paths, schemas, and security metadata.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`

**Dependencies:** Tasks 1 and 4 through 8

**Files likely touched:**
- `src/AuthNet.Api/AuthNetOpenApiDocumentBuilder.cs`
- `tests/AuthNet.Tests/Integration/AuthNetOpenApiTests.cs`

**Estimated scope:** M

## Task 11: Update sample SPA discoverability

**Description:** Add sample `/Spa` controls for external provider discovery and challenge start without turning the sample into a full frontend application.

**Acceptance criteria:**
- [x] Sample page can load external providers.
- [x] Sample page can submit external login challenge requests.
- [x] Sample page can submit external link challenge requests while signed in.
- [x] Existing SPA controls remain intact.
- [x] OpenAPI link remains visible.

**Verification:**
- [x] Build succeeds.
- [x] Manual sample route check documented.

**Dependencies:** Tasks 4 through 10

**Files likely touched:**
- `samples/AuthNet.SampleHost/Pages/Spa.cshtml`
- `docs/developer/quick-start.md`

**Estimated scope:** S

## Task 12: Update docs and context

**Description:** Update user docs, developer docs, architecture context, next-iteration context, and working memory to reflect completed SPA external-login orchestration.

**Acceptance criteria:**
- [x] User guide lists the new SPA external-login JSON routes.
- [x] Configuration docs mention unchanged route-prefix behavior for external-login API routes.
- [x] Developer quick start lists focused Slice 18 tests.
- [x] `docs/architecture-context.md` records completed SPA external-login orchestration.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 18 when complete.
- [x] Docs clearly state JWT, refresh tokens, provider-specific packages, admin JSON APIs, and invitation acceptance JSON remain deferred.

**Verification:**
- [x] Manual doc read-through confirms no stale guidance says SPA external-login workflows are unavailable after implementation.

**Dependencies:** Tasks 1 through 11

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`
- `tasks/slice-18-plan.md`
- `tasks/slice-18-todo.md`

**Estimated scope:** S

## Task 13: Run focused and full verification

**Description:** Verify the Slice 18 SPA external-login orchestration and prepare it for commit.

**Acceptance criteria:**
- [x] Focused Slice 18 SPA external-login API tests pass.
- [x] Existing Razor external-login tests pass.
- [x] Existing SPA API, account workflow, MFA, and OpenAPI tests pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 18 changes plus pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaExternalLoginApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetExternalLoginTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaMfaApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 12

**Files likely touched:**
- `tasks/slice-18-plan.md`
- `tasks/slice-18-todo.md`

**Estimated scope:** S
