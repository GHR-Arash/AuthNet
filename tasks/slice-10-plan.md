# Slice 10 Plan: Admin Direct User Creation

## Overview

Slice 10 fills the remaining onboarding gap in the admin surface: administrators can invite users, but they cannot directly create a local user from the admin UI. This slice adds a minimal server-rendered admin-only create-user flow that uses ASP.NET Core Identity primitives, appears in the existing admin user-management area, and is discoverable from the sample host.

## Scope

In scope:

- Add an admin-only direct user creation page under the existing AuthNet admin users route group.
- Add a route at `/auth/admin/users/new` with the default `/auth` route prefix.
- Let administrators enter username, email, optional display name, password, confirm password, email confirmed state, and optional fixed `Administrator` role assignment.
- Validate duplicate email and duplicate username before creation where practical, while still relying on Identity validation as the source of truth.
- Use `UserManager<AuthNetUser>.CreateAsync(user, password)` so password policy, normalization, and Identity store behavior remain standard.
- Create or ensure the fixed `Administrator` role only when the admin checks the administrator role option.
- Redirect successful creation to the created user's detail page.
- Link the create-user page from the built-in admin user list and sample host admin entry points.
- Add focused integration tests for access control, successful creation, duplicate validation, password policy failure, email confirmation, and optional administrator assignment.
- Update user, developer, architecture, sample, and context docs.

Out of scope:

- Bulk user import.
- CSV upload.
- Random temporary password generation.
- Emailing credentials.
- Force password reset on first login.
- Arbitrary role selection or role management.
- Admin-managed MFA reset.
- User deletion.
- Audit events.
- API/JWT/SPA endpoints.

## Proposed Route

With `AccountRoutePrefix` set to `/auth`:

- `/auth/admin/users/new`

The route requires a signed-in user in the fixed `Administrator` role.

## Architecture Decisions

- Keep the page in `AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users` beside the existing user list/detail pages.
- Use Identity's `UserManager<AuthNetUser>` and `RoleManager<IdentityRole>` instead of custom persistence logic.
- Do not introduce a new domain service unless the page model becomes meaningfully complex.
- Keep the fixed `Administrator` role model from Slice 07; arbitrary role management remains deferred.
- Do not automatically send an email from this flow. Invitations remain the email-driven onboarding path.
- Do not require a database migration; direct creation uses existing Identity tables.
- Keep public registration settings independent from admin direct creation.

## Task List

### Phase 1: Route, Page, and Validation

- [x] Task 1: Add admin create-user route and empty page shell.
- [x] Task 2: Implement create-user form model and validation.
- [x] Task 3: Create users through Identity and redirect to detail.

### Checkpoint: Core Create Flow

- [x] Admin can open `/auth/admin/users/new`.
- [x] Valid form creates a user and redirects to `/auth/admin/users/{id}`.
- [x] Anonymous and non-admin access is rejected.

### Phase 2: Admin Role Option and Navigation

- [x] Task 4: Add optional fixed administrator assignment.
- [x] Task 5: Link create-user flow from built-in admin and sample host UI.

### Checkpoint: Admin UX

- [x] Admin user list has a clear create-user action.
- [x] Sample home page and `/Admin` page expose direct user creation.
- [x] Optional administrator assignment respects existing fixed-role behavior.

### Phase 3: Tests and Documentation

- [x] Task 6: Add focused integration tests.
- [x] Task 7: Update user, developer, architecture, sample, and context docs.
- [x] Task 8: Final verification and local review.

### Checkpoint: Complete

- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests` passes.
- [x] `.\scripts\verify.ps1` passes.
- [x] Slice 10 artifacts are complete and named with `slice-10`.
- [x] No bulk import, arbitrary role management, password-reset enforcement, email credential delivery, API/JWT, or SPA feature is introduced.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Admin-created password becomes an unsafe operational pattern | Medium | Document invitations as preferred email onboarding and keep direct creation explicit/manual. |
| Duplicate checks diverge from Identity behavior | Medium | Use pre-checks for friendly messages but still surface Identity errors from `CreateAsync`. |
| Optional administrator assignment creates role inconsistencies | Medium | Use `RoleManager`/`UserManager.AddToRoleAsync` and the existing fixed role name. |
| Scope expands into full provisioning | Medium | Defer bulk import, random password generation, force reset, arbitrary roles, deletion, and audit events. |
| Sample host falls behind built-in UI | Medium | Include sample home, `/Admin`, and navbar links in the same slice. |

## Open Questions

- Should the direct-create form default `Email confirmed` to checked or unchecked?
- Should admin-created users be required to change their password on first login in a future slice?

## Recommended Default

Default `Email confirmed` to checked because this is an explicit administrator-created local account. Keep force-password-reset as a future slice, and recommend invitations when the user should set their own password without the administrator knowing it.
