# Slice 06 Todo: Admin User Management UI

## Task 1: Add admin route conventions and route docs

**Description:** Extend AuthNet endpoint routing to map admin Razor Pages under the existing account route prefix.

**Acceptance criteria:**
- [x] `/auth/admin/users` maps to the admin user list page.
- [x] `/auth/admin/users/{id}` maps to the admin user detail page.
- [x] Route docs list admin routes separately from public account routes.

**Verification:**
- [x] Focused route tests cover the new admin URLs.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `docs/users/account-pages.md`
- `tests/AuthNet.Tests/Integration/AuthNetRouteTests.cs`

**Estimated scope:** S

## Task 2: Add admin user list page

**Description:** Add a Razor Page that lists users with search and pagination, protected by the `Administrator` role.

**Acceptance criteria:**
- [x] Page requires authenticated `Administrator` role.
- [x] Page lists email, display name, email confirmation state, lockout state, and created/known account fields available today.
- [x] Search filters by email or display name.
- [x] Pagination prevents unbounded result rendering.

**Verification:**
- [x] Admin integration test can load list page and see seeded users.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Index.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Index.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 3: Add admin route authorization tests

**Description:** Test that anonymous users and authenticated non-admin users cannot access admin pages, while admins can.

**Acceptance criteria:**
- [x] Anonymous access to admin list challenges or redirects to login.
- [x] Non-admin authenticated access is denied.
- [x] Admin authenticated access succeeds.

**Verification:**
- [x] Focused admin authorization tests pass.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** S

## Task 4: Add admin user detail page

**Description:** Add a detail page for a single user showing account state and safe metadata from ASP.NET Core Identity.

**Acceptance criteria:**
- [x] Admin can open `/auth/admin/users/{id}` for an existing user.
- [x] Missing user returns not found.
- [x] Page shows email, username, display name, phone number, email confirmed, lockout, access failed count, external login count, and roles.

**Verification:**
- [x] Focused detail tests pass.

**Dependencies:** Task 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 5: Add email confirmation and lockout actions

**Description:** Add POST handlers on the detail page for reversible user state changes: confirm/unconfirm email, lock/unlock user, and reset access failed count.

**Acceptance criteria:**
- [x] Admin can confirm and unconfirm email.
- [x] Admin can lock and unlock a user.
- [x] Admin can reset access failed count.
- [x] Actions use antiforgery-protected POST handlers.

**Verification:**
- [x] Focused action tests pass.

**Dependencies:** Task 4

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 6: Add integration tests for admin detail and actions

**Description:** Expand integration coverage for user detail, search, pagination, and state-changing admin actions.

**Acceptance criteria:**
- [x] Search returns matching users and omits non-matching users.
- [x] Detail page shows expected user state.
- [x] Each admin action mutates Identity state as expected.
- [x] Admin actions reject non-admin users.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests`

**Dependencies:** Tasks 2 through 5

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** M

## Task 7: Update user, developer, architecture, and route docs

**Description:** Document the admin UI routes, required role, intended scope, and deferred admin capabilities.

**Acceptance criteria:**
- [x] User docs explain how to access admin user management.
- [x] Developer docs explain how to create/assign the `Administrator` role for testing.
- [x] Architecture context records admin UI scope.
- [x] Deferred features remain documented as future work.

**Verification:**
- [x] Manual doc review.

**Dependencies:** Tasks 1 through 6

**Files likely touched:**
- `docs/users/account-pages.md`
- `docs/users/getting-started.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/slice-06/admin-user-management.md`

**Estimated scope:** S

## Task 8: Update compact context docs

**Description:** Update `docs/next-iteration-context.md` and `context.md` with Slice 06 status and next likely work.

**Acceptance criteria:**
- [x] Next iteration context records admin UI when complete.
- [x] `context.md` reflects the current product surface.
- [x] Context stays compact.

**Verification:**
- [x] Manual read-through.

**Dependencies:** Task 7

**Files likely touched:**
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 9: Final verification and local review

**Description:** Run the full verification path and review for security, route, and documentation drift.

**Acceptance criteria:**
- [x] `.\scripts\verify.ps1` passes.
- [x] `git diff --check` passes.
- [x] Admin UI does not expose unauthenticated or non-admin access.
- [x] No API/JWT or custom permission scope is introduced.

**Verification:**
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 8

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 10: Commit Slice 06

**Description:** Commit the completed admin user management UI slice.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 06 changes.
- [x] Commit message describes admin user management UI.
- [x] Working tree is clean except unrelated user-owned changes.

**Verification:**
- [x] `git status --short`
- [x] `git log --oneline -1`

**Dependencies:** Task 9

**Files likely touched:** Git metadata only

**Estimated scope:** XS
