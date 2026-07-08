# Slice 09 Todo: Account Invitation Flow

## Task 1: Add invitation entity and DbContext mapping

**Description:** Add a persisted invitation model to the existing AuthNet persistence project and expose it through `AuthNetDbContext`.

**Acceptance criteria:**
- [x] Invitation records include email, normalized email, token hash, created timestamp, expiration timestamp, accepted timestamp, accepted user id, and creator user id.
- [x] `AuthNetDbContext` exposes invitations through a `DbSet`.
- [x] EF mapping includes required fields, string lengths, and indexes for token hash and normalized email.

**Verification:**
- [x] Persistence-focused tests can create and query an invitation record with EF Core InMemory.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Persistence.Postgres/AuthNetInvitation.cs`
- `src/AuthNet.Persistence.Postgres/AuthNetDbContext.cs`
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 2: Add invitation token helper and lifecycle rules

**Description:** Add a small service or helper for generating invitation tokens, hashing tokens for storage, calculating expiration, and checking pending/accepted/expired state.

**Acceptance criteria:**
- [x] Generated invitation tokens are high entropy and URL-safe.
- [x] Stored invitation records keep only a hash of the token.
- [x] Expired and accepted invitations are distinguishable from pending invitations.
- [x] Duplicate pending invitations for the same normalized email are rejected by application logic.

**Verification:**
- [x] Unit or integration tests cover token hashing, valid lookup, invalid token rejection, expired state, and accepted state.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Core` or `src/AuthNet.Persistence.Postgres` invitation helper/service files
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 3: Add PostgreSQL migration and snapshot update

**Description:** Add a migration for the invitation table and update the EF Core model snapshot.

**Acceptance criteria:**
- [x] Migration creates the invitation table with expected columns and indexes.
- [x] Model snapshot matches the new entity mapping.
- [x] Existing Identity schema remains unchanged except for the new invitation table.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Persistence.Postgres/Migrations/*_AddAuthNetInvitations.cs`
- `src/AuthNet.Persistence.Postgres/Migrations/*_AddAuthNetInvitations.Designer.cs`
- `src/AuthNet.Persistence.Postgres/Migrations/AuthNetDbContextModelSnapshot.cs`

**Estimated scope:** M

## Task 4: Add admin invitation routes and list page

**Description:** Register admin invitation routes and add a page where administrators can review pending, accepted, and expired invitations.

**Acceptance criteria:**
- [x] `/auth/admin/invitations` maps to an admin-only invitation list page.
- [x] Page lists invitation email, created time, expiration time, accepted time, and status.
- [x] Anonymous users are challenged.
- [x] Authenticated non-admin users receive access denied.

**Verification:**
- [x] Route tests cover admin invitation list access behavior.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations/Index.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations/Index.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetRouteTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 5: Add admin invitation creation page

**Description:** Add an admin-only form for creating an invitation by email.

**Acceptance criteria:**
- [x] `/auth/admin/invitations/new` maps to an admin-only create page.
- [x] Email is required and normalized before lookup/storage.
- [x] Existing users cannot be invited again.
- [x] Existing pending invitations for the same email are rejected with a validation message.
- [x] Successful creation redirects to the invitation list or shows a success message.

**Verification:**
- [x] Integration tests cover valid creation, duplicate pending invite, and existing-user rejection.

**Dependencies:** Tasks 1, 2, and 4

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations/Create.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 6: Send invitation email with acceptance link

**Description:** Use the existing AuthNet email sender to deliver the invitation acceptance URL after an admin creates an invitation.

**Acceptance criteria:**
- [x] Successful invitation creation calls `IAuthNetEmailSender`.
- [x] Email contains an absolute acceptance URL with the raw invitation token.
- [x] Raw invitation token is not persisted.
- [x] Development email store tests can inspect the generated link.

**Verification:**
- [x] Integration test creates an invitation and extracts the acceptance link from the development email sender.

**Dependencies:** Tasks 2 and 5

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations/Create.cshtml.cs`
- Existing email sender abstractions if a new method is needed
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 7: Add invitation acceptance page

**Description:** Add an anonymous page where a user opens a valid invitation token and enters account credentials.

**Acceptance criteria:**
- [x] `/auth/invitations/accept` maps to the acceptance page.
- [x] GET with a valid token displays the invited email and account creation form.
- [x] GET with an invalid, expired, or accepted token shows a non-enumerating invalid invitation state.
- [x] POST validates username, password, confirm password, and optional display name.

**Verification:**
- [x] Integration tests cover valid GET and invalid-token GET behavior.

**Dependencies:** Tasks 1, 2, and 6

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/AcceptInvitation.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/AcceptInvitation.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 8: Create user and mark invitation accepted

**Description:** Complete invitation acceptance by creating an Identity user with the invited email and marking the invitation as accepted.

**Acceptance criteria:**
- [x] Valid acceptance creates an `AuthNetUser` with the invited email.
- [x] Password policy is enforced by Identity.
- [x] Invited email is marked confirmed after successful acceptance.
- [x] Invitation is marked accepted with timestamp and accepted user id.
- [x] New user is signed in after successful acceptance unless existing project patterns argue against it.

**Verification:**
- [x] Integration test accepts an invitation and verifies user creation, confirmed email, accepted invitation state, and authenticated session.

**Dependencies:** Task 7

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/AcceptInvitation.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 9: Add expired, invalid, and reused invitation handling

**Description:** Harden invitation acceptance edge cases so failed or repeated attempts cannot create accounts or leak invitation state.

**Acceptance criteria:**
- [x] Expired invitations cannot be accepted.
- [x] Accepted invitations cannot be reused.
- [x] Invalid token values do not throw and do not create users.
- [x] Error copy does not reveal whether a specific email is invited.

**Verification:**
- [x] Integration tests cover expired invitation, reused invitation, invalid token, and duplicate user race fallback.

**Dependencies:** Task 8

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/AcceptInvitation.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account/AcceptInvitation.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`

**Estimated scope:** M

## Task 10: Add focused invitation integration tests

**Description:** Consolidate invitation test coverage and add helpers for admin sign-in, invitation creation, email link extraction, acceptance, and invitation state assertions.

**Acceptance criteria:**
- [x] Tests cover route protection, create invitation, email delivery, valid accept, expired accept, reused accept, and invalid token.
- [x] Tests verify public registration remains independent from invitations.
- [x] Existing admin, MFA, login, registration, and email-flow tests still pass.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests`

**Dependencies:** Tasks 1 through 9

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetInvitationTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetRouteTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHtml.cs`

**Estimated scope:** M

## Task 11: Update user, developer, architecture, roadmap, and context docs

**Description:** Document invitation routes, behavior, limits, and architecture decisions.

**Acceptance criteria:**
- [x] User docs list admin invitation and acceptance routes.
- [x] Developer docs include focused invitation test command.
- [x] Architecture context records invitation flow as implemented scope when complete.
- [x] Roadmap marks account invitation flow as implemented when complete.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 09 status.

**Verification:**
- [x] Manual read-through for stale references to invitation as deferred.

**Dependencies:** Tasks 1 through 10

**Files likely touched:**
- `docs/users/account-pages.md`
- `docs/users/getting-started.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/mvp-roadmap.md`
- `docs/product-decisions.md`
- `docs/slice-09/account-invitations.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 12: Final verification and local review

**Description:** Run full verification and review for security, scope, migrations, and documentation drift.

**Acceptance criteria:**
- [x] `.\scripts\verify.ps1` passes.
- [x] `git diff --check` passes.
- [x] Invitation tokens are high entropy and stored hashed.
- [x] Public registration behavior is unchanged.
- [x] No role assignment during invite, API/JWT, SPA, bulk invite, or organization feature is introduced.

**Verification:**
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 11

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 13: Commit Slice 09

**Description:** Commit the completed account invitation flow slice.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 09 changes.
- [x] Commit message describes account invitations.
- [x] Working tree is clean except unrelated user-owned changes.

**Verification:**
- [ ] `git status --short`
- [ ] `git log --oneline -1`

**Dependencies:** Task 12

**Files likely touched:** Git metadata only

**Estimated scope:** XS
