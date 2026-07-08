# Slice 10 Todo: Admin Direct User Creation

## Task 1: Add admin create-user route and page shell

**Description:** Add a new admin-only Razor Page in the existing admin users area for direct local user creation.

**Acceptance criteria:**
- [x] `/auth/admin/users/new` maps to a Razor Page.
- [x] The page requires the fixed `Administrator` role.
- [x] Anonymous users are challenged.
- [x] Authenticated non-admin users are sent to access denied.
- [x] The page uses the existing AuthNet UI layout and admin route prefix behavior.

**Verification:**
- [x] Route/access tests cover anonymous, non-admin, and admin GET behavior.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** S

## Task 2: Implement create-user form model and validation

**Description:** Add input fields and validation for username, email, optional display name, password, confirm password, email confirmed state, and optional administrator role assignment.

**Acceptance criteria:**
- [x] Username, email, password, and confirm password are required.
- [x] Email format is validated.
- [x] Password confirmation must match.
- [x] Duplicate username and duplicate email produce user-facing validation messages.
- [x] Identity password policy errors are shown without creating a user.

**Verification:**
- [x] Integration tests cover duplicate email, duplicate username, and invalid password submission.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 3: Create users through Identity and redirect to detail

**Description:** Use `UserManager<AuthNetUser>.CreateAsync(user, password)` to create the user and redirect to the created user's detail page.

**Acceptance criteria:**
- [x] Valid submission creates an `AuthNetUser`.
- [x] Username, email, display name, and email confirmed state are persisted.
- [x] Successful creation redirects to `/auth/admin/users/{id}`.
- [x] Public registration disabled state does not block admin direct creation.

**Verification:**
- [x] Integration test submits a valid form, follows the redirect, and verifies the user detail page and persisted user state.

**Dependencies:** Task 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 4: Add optional fixed administrator assignment

**Description:** Let the creating administrator optionally grant the fixed `Administrator` role to the new user during direct creation.

**Acceptance criteria:**
- [x] Create form includes an administrator access checkbox.
- [x] When checked, the `Administrator` role is created if missing and assigned to the new user.
- [x] When unchecked, the new user is not assigned administrator access.
- [x] If role assignment fails after user creation, the page reports the failure clearly.

**Verification:**
- [x] Integration tests cover create-with-admin-role and create-without-admin-role.

**Dependencies:** Task 3

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`

**Estimated scope:** M

## Task 5: Link create-user flow from built-in admin and sample host UI

**Description:** Make direct user creation discoverable from the existing built-in admin user list and from the sample project.

**Acceptance criteria:**
- [x] `/auth/admin/users` includes a clear create-user link.
- [x] `samples/AuthNet.SampleHost/Pages/Index.cshtml` links to `/auth/admin/users/new`.
- [x] `samples/AuthNet.SampleHost/Pages/Admin.cshtml` links to `/auth/admin/users/new`.
- [x] `samples/AuthNet.SampleHost/Pages/Shared/_Layout.cshtml` exposes the direct create-user route if the navigation remains readable.

**Verification:**
- [x] Build succeeds and manual HTML review confirms links point to the expected route.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Index.cshtml`
- `samples/AuthNet.SampleHost/Pages/Index.cshtml`
- `samples/AuthNet.SampleHost/Pages/Admin.cshtml`
- `samples/AuthNet.SampleHost/Pages/Shared/_Layout.cshtml`

**Estimated scope:** S

## Task 6: Add focused integration tests

**Description:** Extend admin user integration coverage for direct user creation and route protection.

**Acceptance criteria:**
- [x] Tests cover route protection for `/auth/admin/users/new`.
- [x] Tests cover successful direct user creation.
- [x] Tests cover duplicate email and duplicate username validation.
- [x] Tests cover password policy failure.
- [x] Tests cover email confirmed true/false behavior.
- [x] Tests cover optional administrator role assignment.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests`

**Dependencies:** Tasks 1 through 5

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetAdminUserTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHtml.cs`

**Estimated scope:** M

## Task 7: Update user, developer, architecture, sample, and context docs

**Description:** Document direct admin user creation as implemented scope, including route, behavior, limitations, and sample project links.

**Acceptance criteria:**
- [x] User docs list `/auth/admin/users/new` and explain when to use direct creation versus invitations.
- [x] Developer docs include the focused test command and sample host route.
- [x] Architecture context records direct admin creation as part of admin user-management UI when complete.
- [x] Functional requirements include direct admin user creation as a Should Have admin onboarding path.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 10 status when complete.

**Verification:**
- [x] Manual read-through for stale statements that admins can only create users by invitation.

**Dependencies:** Tasks 1 through 6

**Files likely touched:**
- `docs/users/account-pages.md`
- `docs/users/getting-started.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/functional-requirements.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 8: Final verification and local review

**Description:** Run full verification and review for scope control, security posture, sample coverage, and documentation drift.

**Acceptance criteria:**
- [x] Focused admin user tests pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Sample project links are updated in the same slice.
- [x] No out-of-scope provisioning features are introduced.
- [x] Working tree contains only intended Slice 10 changes plus any pre-existing unrelated user changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 7

**Files likely touched:** None unless review finds issues

**Estimated scope:** S
