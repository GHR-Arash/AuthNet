# Slice 13 Todo: Role Management and Permission Enhancement

## Task 1: Add permission constants and policy registration

**Description:** Add a small AuthNet permission catalog and authorization policies that accept either the `Administrator` role or a matching role-claim permission.

**Acceptance criteria:**
- [x] Permission constants exist for users, roles, invitations, and audit operations.
- [x] Policies are registered during `AddAuthNet`.
- [x] The `Administrator` role satisfies every AuthNet permission policy.
- [x] A user with a role claim for a permission satisfies that specific policy.

**Verification:**
- [x] Focused authorization tests cover administrator fallback and role-claim permission success/failure.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Core`
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `tests/AuthNet.Tests/AuthNetOptionsTests.cs` or new authorization tests

**Estimated scope:** M

## Task 2: Add tests for permission policy behavior

**Description:** Add integration or service-level tests proving permission policies work through the normal AuthNet registration path.

**Acceptance criteria:**
- [x] Non-admin users without permission are denied from a permission-protected route.
- [x] Users with a role containing the matching permission claim are allowed.
- [x] Administrators remain allowed without explicit permission claims.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetPermissionTests`

**Dependencies:** Task 1

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetPermissionTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** M

## Task 3: Add admin role list and create pages

**Description:** Add built-in admin pages for listing existing roles and creating a new role through `RoleManager<IdentityRole>`.

**Acceptance criteria:**
- [x] `/auth/admin/roles` lists existing roles.
- [x] `/auth/admin/roles/new` creates a role with validated input.
- [x] Duplicate role names are rejected.
- [x] Pages are protected by `Administrator` or `authnet.roles.manage` as appropriate.

**Verification:**
- [x] Focused role tests cover route protection, role list rendering, valid creation, and duplicate rejection.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Roles/Index.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Roles/Index.cshtml.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Roles/Create.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Roles/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetRoleTests.cs`

**Estimated scope:** M

## Task 4: Add admin role detail page with permission assignment

**Description:** Add a role detail page where administrators can view role metadata and assign/remove built-in AuthNet permissions stored as Identity role claims.

**Acceptance criteria:**
- [x] `/auth/admin/roles/{id}` shows role name and assigned permissions.
- [x] Administrators can add a built-in permission to a role.
- [x] Administrators can remove a built-in permission from a role.
- [x] Duplicate permission claims are not created.
- [x] Unknown permission values are rejected.

**Verification:**
- [x] Focused role tests cover permission add/remove, duplicate handling, and invalid permission rejection.

**Dependencies:** Tasks 1 through 3

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Roles/Detail.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Roles/Detail.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetRoleTests.cs`

**Estimated scope:** M

## Task 5: Add admin user arbitrary role assignment

**Description:** Extend the admin user detail page from fixed administrator assignment to general role assignment while preserving last-administrator protection.

**Acceptance criteria:**
- [x] User detail shows all assigned roles.
- [x] User detail lets administrators add an existing role to the user.
- [x] User detail lets administrators remove an assigned non-last-admin role.
- [x] Removing the last remaining `Administrator` role is still blocked.
- [x] Existing fixed administrator grant/remove behavior remains compatible or is cleanly mapped to the general role UI.

**Verification:**
- [x] Focused admin user tests cover assigning and removing arbitrary roles plus last-admin protection.

**Dependencies:** Tasks 1 through 4

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 6: Add audit coverage for role and permission mutations

**Description:** Record audit events for successful role creation, user role assignment/removal, and role permission assignment/removal.

**Acceptance criteria:**
- [x] Role creation writes an audit event.
- [x] User role assignment/removal writes audit events.
- [x] Role permission assignment/removal writes audit events.
- [x] Metadata excludes secrets and remains compact.
- [x] Audit list can filter these new actions through existing filtering behavior.

**Verification:**
- [x] Focused audit tests cover the new action names.

**Dependencies:** Tasks 3 through 5

**Files likely touched:**
- Role/admin page models touched by Tasks 3 through 5
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`

**Estimated scope:** M

## Task 7: Update sample navigation and documentation

**Description:** Make role management discoverable in the sample host and document the new role and permission model for consumers.

**Acceptance criteria:**
- [x] Sample home page links to `/auth/admin/roles`.
- [x] Sample shared navigation links to role management after sign-in.
- [x] User docs explain roles, permissions, and `Administrator` superuser behavior.
- [x] Developer quick start lists the new role routes and focused test command.
- [x] Docs state permission changes may require sign-in/session refresh if that remains true.

**Verification:**
- [x] Manual doc read-through confirms no stale guidance says only fixed administrator assignment exists.

**Dependencies:** Tasks 3 through 6

**Files likely touched:**
- `samples/AuthNet.SampleHost/Pages/Index.cshtml`
- `samples/AuthNet.SampleHost/Pages/Shared/_Layout.cshtml`
- `samples/AuthNet.SampleHost/Pages/Admin.cshtml`
- `docs/users/getting-started.md`
- `docs/users/account-pages.md`
- `docs/developer/quick-start.md`

**Estimated scope:** S

## Task 8: Update project memory and run final verification

**Description:** Update compact context docs, run focused and full verification, and prepare the slice for commit.

**Acceptance criteria:**
- [x] `docs/next-iteration-context.md` records Slice 13 when complete.
- [x] `docs/architecture-context.md` records active role/permission behavior.
- [x] `context.md` records current role and permission state.
- [x] Full local verification passes.
- [x] Working tree contains only intended Slice 13 changes plus pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetRoleTests`
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetPermissionTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 7

**Files likely touched:**
- `context.md`
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `tasks/slice-13-plan.md`
- `tasks/slice-13-todo.md`

**Estimated scope:** S
