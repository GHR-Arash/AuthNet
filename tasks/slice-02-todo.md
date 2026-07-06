# Slice 02 Todo: Integration Hardening

## Task 1: Add integration test dependencies

**Description:** Add the packages and project references needed to run ASP.NET Core host-level tests from `AuthNet.Tests`.

**Acceptance criteria:**
- [x] `AuthNet.Tests` can reference ASP.NET Core test hosting APIs.
- [x] Test project can reference the sample host or a purpose-built test host.
- [x] Existing unit tests still compile and run.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [ ] `.\.dotnet\dotnet.exe test AuthNet.slnx --no-build`

**Dependencies:** None

**Files likely touched:**
- `tests/AuthNet.Tests/AuthNet.Tests.csproj`

**Estimated scope:** S

## Task 2: Build minimal AuthNet test host factory

**Description:** Create a reusable test host fixture that registers AuthNet with deterministic test configuration and replaceable infrastructure services.

**Acceptance criteria:**
- [x] Tests can create an HTTP client against a host with AuthNet registered.
- [x] The host uses development-safe configuration.
- [x] Test setup avoids requiring a live PostgreSQL instance for basic route tests.

**Verification:**
- [x] A smoke test can request `/auth/login` and receive success.
- [x] Full test project passes.

**Dependencies:** Task 1

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`
- `tests/AuthNet.Tests/Integration/AuthNetRouteTests.cs`

**Estimated scope:** M

## Task 3: Add test email sender support

**Description:** Add a test email sender/store that captures outbound messages for assertions in registration, confirmation, resend, and reset flows.

**Acceptance criteria:**
- [x] Tests can inspect sent email recipient, subject, HTML body, text body, and links.
- [x] Captured messages are isolated per test.
- [x] Production validation is not weakened.

**Verification:**
- [x] Unit or integration test proves a message is captured.
- [x] Full test project passes.

**Dependencies:** Task 2

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/TestEmailSender.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** S

## Task 4: Cover route availability and auth requirements

**Description:** Add integration tests for every documented account route and verify authenticated-only pages redirect unauthenticated users.

**Acceptance criteria:**
- [x] Public account routes return success or expected redirect/status.
- [x] `/auth/profile` and `/auth/change-password` require authentication.
- [x] Test route list matches `docs/users/account-pages.md`.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetRouteTests`

**Dependencies:** Task 2

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetRouteTests.cs`
- `docs/users/account-pages.md` if route docs drift

**Estimated scope:** M

## Task 5: Cover registration and email confirmation

**Description:** Test public-registration behavior, confirmation email generation, and confirm-email token handling at the host boundary.

**Acceptance criteria:**
- [x] Registration is unavailable or blocked when public registration is disabled.
- [x] Enabled registration sends a confirmation email.
- [x] Confirmation link can confirm the user or fails safely for invalid tokens.

**Verification:**
- [x] Integration tests pass.
- [x] No live email provider is required.

**Dependencies:** Tasks 2, 3

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetRegistrationTests.cs`

**Estimated scope:** M

## Task 6: Cover resend confirmation and forgot-password behavior

**Description:** Test account recovery email flows and confirm they avoid user enumeration.

**Acceptance criteria:**
- [x] Resend confirmation sends an email for an existing unconfirmed account.
- [x] Resend confirmation returns the same visible response for missing or already confirmed accounts.
- [x] Forgot-password returns the same visible response for missing accounts and only sends when appropriate.

**Verification:**
- [x] Integration tests pass.

**Dependencies:** Tasks 2, 3

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetEmailFlowTests.cs`

**Estimated scope:** M

## Task 7: Cover profile, email change, and password access rules

**Description:** Test authenticated profile behavior, verified email-change message generation, and change-password route protection.

**Acceptance criteria:**
- [x] Unauthenticated profile access redirects/challenges.
- [x] Authenticated profile update can change display name and phone number.
- [x] Changing email sends a confirmation link and does not immediately mutate stored email.
- [x] Confirming the email-change link updates email and username.

**Verification:**
- [x] Integration tests pass.

**Dependencies:** Tasks 2, 3

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetProfileTests.cs`

**Estimated scope:** M

## Task 8: Cover external-login safety behavior

**Description:** Add tests around external login callback rules without requiring a real external provider.

**Acceptance criteria:**
- [x] Already linked external login signs in the linked account.
- [x] Existing local account is not linked by matching unauthenticated external email alone.
- [x] New external provisioning requires a verified email claim.
- [x] Authenticated user can link an external provider from profile flow.

**Verification:**
- [x] Integration tests pass.

**Dependencies:** Task 2

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetExternalLoginTests.cs`
- Possibly a test authentication handler

**Estimated scope:** M

## Task 9: Introduce explicit AuthNet endpoint mapping API

**Description:** Add a clearer public endpoint mapping API so host apps can map AuthNet pages without `UseAuthNet()` globally mapping all Razor Pages.

**Acceptance criteria:**
- [x] New API maps AuthNet account endpoints under `AccountRoutePrefix`.
- [x] Host apps keep control of their own endpoint mapping.
- [x] Existing configuration validation still runs.

**Verification:**
- [x] Integration tests prove AuthNet routes and host-owned routes coexist.
- [x] Build and focused tests pass.

**Dependencies:** Tasks 4 through 8

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetApplicationBuilderExtensions.cs`
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetEndpointMappingTests.cs`

**Estimated scope:** M

## Task 10: Update sample host and docs for explicit mapping

**Description:** Move the sample and documentation to the refined endpoint mapping API.

**Acceptance criteria:**
- [x] Sample host uses the new mapping API.
- [x] User quick start and integration requirements use the new API.
- [x] Old guidance is removed or clearly marked as compatibility.

**Verification:**
- [x] Build and focused tests pass.
- [x] Sample host starts.

**Dependencies:** Task 9

**Files likely touched:**
- `samples/AuthNet.SampleHost/Program.cs`
- `docs/users/getting-started.md`
- `docs/integration-requirements.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`

**Estimated scope:** M

## Task 11: Decide and test `UseAuthNet()` compatibility

**Description:** Either keep `UseAuthNet()` as a wrapper for compatibility or mark it obsolete before packaging.

**Acceptance criteria:**
- [x] Decision is documented.
- [x] Tests cover the chosen behavior.
- [x] Public API behavior is unambiguous for consumers.

**Verification:**
- [x] Build has no unexpected warnings.
- [x] Focused tests pass.

**Dependencies:** Task 9

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetApplicationBuilderExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetEndpointMappingTests.cs`
- `docs/users/getting-started.md`

**Estimated scope:** S

## Task 12: Update compact context docs

**Description:** Keep project memory synchronized with the slice outcome.

**Acceptance criteria:**
- [x] `docs/architecture-context.md` reflects endpoint mapping and integration test strategy.
- [x] `docs/next-iteration-context.md` records current state, verification commands, and likely next work.
- [x] Docs stay compact.

**Verification:**
- [x] Manual read-through for drift.

**Dependencies:** Tasks 9 through 11

**Files likely touched:**
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `tasks/slice-02-todo.md`

**Estimated scope:** S

## Task 13: Final verification and local review

**Description:** Run the full verification path and review the slice for security, behavior, and documentation drift.

**Acceptance criteria:**
- [x] Build passes.
- [x] Tests pass.
- [x] `git diff --check` passes.
- [x] Local review findings are addressed or documented.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [x] `.\.dotnet\dotnet.exe test AuthNet.slnx --no-build`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 12

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 14: Commit Slice 2

**Description:** Commit the completed slice with implementation, tests, and docs.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 2 changes.
- [x] Commit message is concise and describes the outcome.
- [x] Working tree is clean after commit.

**Verification:**
- [x] `git status --short`
- [x] `git log --oneline -1`

**Dependencies:** Task 13

**Files likely touched:** Git metadata only

**Estimated scope:** XS
