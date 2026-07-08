# Slice 14 Todo: SPA Workflow Foundation

## Task 1: Add `AuthNet.Api` project and solution/package wiring

**Description:** Add a new package project for SPA-oriented JSON endpoints without changing existing Razor UI behavior.

**Acceptance criteria:**
- [x] `src/AuthNet.Api` exists and targets .NET 10.
- [x] Project references match the minimal required AuthNet packages.
- [x] `AuthNet.slnx` includes the project.
- [x] Package metadata is consistent with existing packable packages.
- [x] Verify script packs `AuthNet.Api` if it is intended to ship.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Api/AuthNet.Api.csproj`
- `AuthNet.slnx`
- `scripts/verify.ps1`
- package docs

**Estimated scope:** M

## Task 2: Add API response contracts and endpoint mapping extension

**Description:** Define small JSON response models and a mapping extension for account API endpoints under the configured AuthNet route prefix.

**Acceptance criteria:**
- [x] Response contracts represent success, message, and validation errors.
- [x] Endpoint mapping extension exists, for example `MapAuthNetApi()`.
- [x] API routes use `AuthNetOptions.NormalizedAccountRoutePrefix`.
- [x] Existing `MapAuthNet()` Razor route behavior remains unchanged.

**Verification:**
- [x] Integration tests verify default route behavior.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Api`
- `tests/AuthNet.Tests/Integration/AuthNetSpaApiTests.cs`

**Estimated scope:** M

## Task 3: Add session and logout endpoints

**Description:** Add JSON endpoints for reading current session state and signing out of the existing Identity cookie session.

**Acceptance criteria:**
- [x] `GET /auth/api/session` returns anonymous state when unauthenticated.
- [x] `GET /auth/api/session` returns user id, email, username, and roles when authenticated.
- [x] `POST /auth/api/logout` signs out authenticated users.
- [x] Logout is safe and idempotent for anonymous requests.

**Verification:**
- [x] Focused SPA API tests cover anonymous session, authenticated session, and logout.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Api`
- `tests/AuthNet.Tests/Integration/AuthNetSpaApiTests.cs`

**Estimated scope:** M

## Task 4: Add login endpoint

**Description:** Add JSON password sign-in using email or username and the existing Identity sign-in behavior.

**Acceptance criteria:**
- [x] `POST /auth/api/login` accepts email/username and password.
- [x] Valid credentials issue the existing application cookie.
- [x] Invalid credentials return a generic failure response.
- [x] Locked-out and email-confirmation-required states are handled safely.
- [x] Existing Razor login behavior remains unchanged.

**Verification:**
- [x] Focused SPA API tests cover successful login, invalid login, and email confirmation requirement.

**Dependencies:** Tasks 1 through 3

**Files likely touched:**
- `src/AuthNet.Api`
- `tests/AuthNet.Tests/Integration/AuthNetSpaApiTests.cs`

**Estimated scope:** M

## Task 5: Add registration endpoint

**Description:** Add JSON registration using existing AuthNet options, Identity user creation, and email confirmation delivery.

**Acceptance criteria:**
- [x] `POST /auth/api/register` respects `EnablePublicRegistration`.
- [x] Valid registration creates a local user.
- [x] Duplicate email/username and password policy errors return JSON validation errors.
- [x] Email confirmation message is sent when required.
- [x] Created account behavior matches existing Razor registration semantics.

**Verification:**
- [x] Focused SPA API tests cover disabled registration, successful registration, duplicates, and email delivery.

**Dependencies:** Tasks 1 through 4

**Files likely touched:**
- `src/AuthNet.Api`
- `tests/AuthNet.Tests/Integration/AuthNetSpaApiTests.cs`

**Estimated scope:** M

## Task 6: Add forgot-password and resend-confirmation endpoints

**Description:** Add JSON endpoints for email-driven account recovery flows using the existing email sender and safe non-enumerating responses.

**Acceptance criteria:**
- [x] `POST /auth/api/forgot-password` sends reset instructions for known users.
- [x] Unknown forgot-password emails receive the same generic response.
- [x] `POST /auth/api/resend-confirmation` sends confirmation email for known unconfirmed users.
- [x] Responses do not reveal account existence.
- [x] Email bodies use existing link-generation/message helpers where practical.

**Verification:**
- [x] Focused SPA API tests cover known/unknown forgot-password and resend-confirmation behavior.

**Dependencies:** Tasks 1 through 5

**Files likely touched:**
- `src/AuthNet.Api`
- `tests/AuthNet.Tests/Integration/AuthNetSpaApiTests.cs`

**Estimated scope:** M

## Task 7: Add profile read endpoint

**Description:** Add an authenticated JSON endpoint for reading the current user's profile data.

**Acceptance criteria:**
- [x] `GET /auth/api/profile` requires authentication.
- [x] Authenticated response includes email, username, display name, phone number, email confirmation state, roles, and MFA enabled state if readily available.
- [x] Anonymous requests receive a proper unauthorized response.

**Verification:**
- [x] Focused SPA API tests cover anonymous and authenticated profile reads.

**Dependencies:** Tasks 1 through 4

**Files likely touched:**
- `src/AuthNet.Api`
- `tests/AuthNet.Tests/Integration/AuthNetSpaApiTests.cs`

**Estimated scope:** S

## Task 8: Add sample SPA smoke page in sample host

**Description:** Add a minimal same-origin SPA-style sample surface to exercise the JSON workflow from the sample host.

**Acceptance criteria:**
- [x] Sample host links to a SPA smoke page.
- [x] Page can call session, login, profile, and logout endpoints.
- [x] Page is minimal plain HTML/JavaScript or Razor-hosted static assets.
- [x] Existing sample admin/account links remain intact.

**Verification:**
- [x] Manual sample host smoke instructions are documented.
- [ ] Optional integration test confirms the sample route renders.

**Dependencies:** Tasks 1 through 7

**Files likely touched:**
- `samples/AuthNet.SampleHost/Pages`
- `samples/AuthNet.SampleHost/wwwroot`
- `docs/developer/quick-start.md`

**Estimated scope:** S

## Task 9: Update docs, context, and package guidance

**Description:** Document the first SPA workflow, package shape, route surface, security assumptions, and deferred JWT/refresh-token work.

**Acceptance criteria:**
- [x] User docs explain same-origin SPA cookie workflow.
- [x] Configuration docs mention API route behavior.
- [x] Developer quick start lists focused SPA API tests.
- [x] `docs/architecture-context.md` records `AuthNet.Api`.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 14 when complete.
- [x] Docs clearly state JWT/refresh tokens are deferred.

**Verification:**
- [x] Manual doc read-through confirms no stale guidance says all SPA/API work is future scope.

**Dependencies:** Tasks 1 through 8

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 10: Run focused and full verification

**Description:** Verify the SPA workflow slice and prepare it for commit.

**Acceptance criteria:**
- [x] Focused SPA API tests pass.
- [x] Existing account, invitation, role, and admin tests still pass under full verification.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [ ] Working tree contains only intended Slice 14 changes plus pre-existing unrelated user changes.

**Verification:**
- [ ] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaApiTests`
- [ ] `.\scripts\verify.ps1`
- [ ] `git diff --check`
- [ ] `git status --short`

**Dependencies:** Tasks 1 through 9

**Files likely touched:**
- `tasks/slice-14-plan.md`
- `tasks/slice-14-todo.md`

**Estimated scope:** S
