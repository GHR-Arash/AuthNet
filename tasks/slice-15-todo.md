# Slice 15 Todo: OpenAPI Document Endpoint

## Task 1: Add OpenAPI document contracts and builder

**Description:** Add a small internal document builder in `AuthNet.Api` that creates a deterministic OpenAPI 3.1 JSON document for the AuthNet SPA API surface.

**Acceptance criteria:**
- [x] Builder returns an object that serializes to valid OpenAPI JSON.
- [x] Document includes `openapi`, `info`, `paths`, and `components`.
- [x] Document uses `AuthNetOptions.NormalizedAccountRoutePrefix` when constructing path keys.
- [x] Builder is scoped to AuthNet SPA API routes only.

**Verification:**
- [x] Unit or integration test parses the document with `System.Text.Json`.
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Api/AuthNetOpenApiDocumentBuilder.cs`
- `src/AuthNet.Api/AuthNet.Api.csproj`

**Estimated scope:** M

## Task 2: Add OpenAPI endpoint mapping

**Description:** Map the OpenAPI document endpoint from the existing AuthNet API route group so it is available through `app.MapAuthNet()`.

**Acceptance criteria:**
- [x] `GET /auth/api/openapi.json` returns the OpenAPI document by default.
- [x] Custom route prefixes map the endpoint under `{AccountRoutePrefix}/api/openapi.json`.
- [x] Endpoint has a stable route name and summary metadata.
- [x] Existing SPA API routes continue to work unchanged.

**Verification:**
- [x] Focused integration test verifies default route.
- [x] Focused integration test verifies custom account route prefix.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Api/AuthNetApiEndpointRouteBuilderExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetOpenApiTests.cs`

**Estimated scope:** S

## Task 3: Document all SPA API operations

**Description:** Add OpenAPI path and operation entries for all Slice 14 SPA JSON endpoints.

**Acceptance criteria:**
- [x] Document includes `GET /auth/api/session`.
- [x] Document includes `GET /auth/api/profile`.
- [x] Document includes `POST /auth/api/login`.
- [x] Document includes `POST /auth/api/logout`.
- [x] Document includes `POST /auth/api/register`.
- [x] Document includes `POST /auth/api/forgot-password`.
- [x] Document includes `POST /auth/api/resend-confirmation`.
- [x] Document does not include Razor Page, admin UI, or sample-host-only routes.

**Verification:**
- [x] Focused OpenAPI tests assert the expected path set.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api/AuthNetOpenApiDocumentBuilder.cs`
- `tests/AuthNet.Tests/Integration/AuthNetOpenApiTests.cs`

**Estimated scope:** M

## Task 4: Document schemas and cookie security semantics

**Description:** Add schema components for request/response contracts and document cookie-based browser session authentication.

**Acceptance criteria:**
- [x] Components include all public Slice 14 request and response records.
- [x] Request body schemas are referenced by POST operations.
- [x] Response schemas are referenced by success and error responses.
- [x] Authenticated operations document cookie-session security.
- [x] The document does not advertise JWT bearer auth.

**Verification:**
- [x] Focused OpenAPI tests assert core schema names.
- [x] Focused OpenAPI tests assert cookie security scheme exists and bearer scheme does not.

**Dependencies:** Tasks 1 through 3

**Files likely touched:**
- `src/AuthNet.Api/AuthNetOpenApiDocumentBuilder.cs`
- `tests/AuthNet.Tests/Integration/AuthNetOpenApiTests.cs`

**Estimated scope:** M

## Task 5: Add focused OpenAPI integration tests

**Description:** Add a focused test class covering route availability, JSON shape, custom prefixes, documented operations, and schema/security metadata.

**Acceptance criteria:**
- [x] Tests parse the OpenAPI document as JSON.
- [x] Tests verify default and custom prefix behavior.
- [x] Tests verify paths and schemas.
- [x] Tests verify the document excludes non-API routes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`

**Dependencies:** Tasks 1 through 4

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetOpenApiTests.cs`

**Estimated scope:** S

## Task 6: Add sample-host discoverability

**Description:** Add links or small sample-host copy so developers can discover the OpenAPI document while manually testing the SPA API.

**Acceptance criteria:**
- [x] Sample home page or SPA page links to `/auth/api/openapi.json`.
- [x] Existing sample account/admin/SPAs links remain intact.
- [x] No bundled Swagger UI or frontend documentation framework is introduced.

**Verification:**
- [x] Build succeeds.
- [ ] Optional integration test verifies the sample page contains the OpenAPI link.

**Dependencies:** Tasks 1 through 5

**Files likely touched:**
- `samples/AuthNet.SampleHost/Pages/Index.cshtml`
- `samples/AuthNet.SampleHost/Pages/Spa.cshtml`

**Estimated scope:** S

## Task 7: Update docs and context

**Description:** Document the OpenAPI endpoint route, scope, security assumptions, and deferred UI/token-auth work.

**Acceptance criteria:**
- [x] User guide documents `/auth/api/openapi.json`.
- [x] Configuration docs mention route-prefix behavior.
- [x] Developer quick start lists focused OpenAPI tests.
- [x] `docs/architecture-context.md` records the OpenAPI endpoint.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 15 when complete.
- [x] Docs clearly state Swagger UI and JWT bearer auth are deferred.

**Verification:**
- [x] Manual doc read-through confirms no stale guidance says OpenAPI is unavailable.

**Dependencies:** Tasks 1 through 6

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 8: Run focused and full verification

**Description:** Verify the OpenAPI slice and prepare it for commit.

**Acceptance criteria:**
- [x] Focused OpenAPI tests pass.
- [x] Existing SPA API tests still pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 15 changes plus pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaApiTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 7

**Files likely touched:**
- `tasks/slice-15-plan.md`
- `tasks/slice-15-todo.md`

**Estimated scope:** S
