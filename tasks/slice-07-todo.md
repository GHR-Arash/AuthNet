# Slice 07 Todo: Admin Role Assignment UI

## Task 1: Add detail-page role assignment state

**Description:** Extend the admin user detail model and page with explicit state for the fixed `Administrator` role.

**Acceptance criteria:**
- [x] Detail page indicates whether the viewed user is an administrator.
- [x] Existing role list remains visible.
- [x] State is derived from ASP.NET Core Identity role membership.

**Verification:**
- [x] Focused detail test sees administrator role state for admin and non-admin users.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** S

## Task 2: Add administrator grant action

**Description:** Add a POST handler and UI action that grants the fixed `Administrator` role to the viewed user.

**Acceptance criteria:**
- [x] Admin can grant `Administrator` to a non-admin user.
- [x] The `Administrator` role is created if missing.
- [x] Granting a user who already has the role is idempotent.
- [x] Action uses an antiforgery-protected POST.

**Verification:**
- [x] Focused integration test posts the grant action and verifies role membership.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 3: Add administrator removal action

**Description:** Add a POST handler and UI action that removes the fixed `Administrator` role from the viewed user when safe.

**Acceptance criteria:**
- [x] Admin can remove `Administrator` from a user when at least one other admin remains.
- [x] Removing from a non-admin user is idempotent.
- [x] Action uses an antiforgery-protected POST.

**Verification:**
- [x] Focused integration test posts the remove action and verifies role membership is removed.

**Dependencies:** Task 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 4: Add last-admin protection

**Description:** Prevent removal of the `Administrator` role when the target user is the last remaining administrator.

**Acceptance criteria:**
- [x] Attempting to remove the last administrator leaves role membership unchanged.
- [x] Page shows a validation or status message explaining why removal was blocked.
- [x] The check runs immediately before removal.

**Verification:**
- [x] Focused integration test verifies the last administrator remains in role after a remove attempt.

**Dependencies:** Task 3

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 5: Expand admin integration tests

**Description:** Consolidate focused coverage for role assignment authorization and state changes.

**Acceptance criteria:**
- [x] Admin grant test passes.
- [x] Admin remove test passes.
- [x] Last-admin protection test passes.
- [x] Non-admin POST to grant/remove is denied and does not mutate roles.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests`

**Dependencies:** Tasks 1 through 4

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** M

## Task 6: Update user and developer documentation

**Description:** Document administrator role assignment behavior for package consumers and local developers.

**Acceptance criteria:**
- [x] User docs explain how the built-in admin UI grants and removes `Administrator`.
- [x] Developer quick start mentions role assignment UI as the normal post-bootstrap path.
- [x] Account page docs list role assignment as part of the admin detail page.

**Verification:**
- [x] Manual doc review.

**Dependencies:** Tasks 1 through 5

**Files likely touched:**
- `docs/users/getting-started.md`
- `docs/users/account-pages.md`
- `docs/developer/quick-start.md`

**Estimated scope:** S

## Task 7: Update architecture, slice, and context docs

**Description:** Record Slice 07 scope and current product state in compact project memory.

**Acceptance criteria:**
- [x] `docs/slice-06/admin-user-management.md` or a new `docs/slice-07/admin-role-assignment.md` records the completed role assignment scope.
- [x] `docs/architecture-context.md` reflects that fixed-role assignment UI is active scope.
- [x] `docs/next-iteration-context.md` and `context.md` reflect the completed slice and next likely work.

**Verification:**
- [x] Manual read-through for stale references to role assignment as deferred.

**Dependencies:** Task 6

**Files likely touched:**
- `docs/slice-07/admin-role-assignment.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 8: Final verification and local review

**Description:** Run full verification and review the diff for security, scope, and documentation drift.

**Acceptance criteria:**
- [x] `.\scripts\verify.ps1` passes.
- [x] `git diff --check` passes.
- [x] Admin role assignment does not expose unauthenticated or non-admin mutation paths.
- [x] No arbitrary role management, API/JWT surface, invitation flow, or custom permission model is introduced.

**Verification:**
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 7

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 9: Commit Slice 07

**Description:** Commit the completed admin role assignment UI slice.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 07 changes.
- [x] Commit message describes admin role assignment UI.
- [x] Working tree is clean except unrelated user-owned changes.

**Verification:**
- [x] `git status --short`
- [x] `git log --oneline -1`

**Dependencies:** Task 8

**Files likely touched:** Git metadata only

**Estimated scope:** XS
