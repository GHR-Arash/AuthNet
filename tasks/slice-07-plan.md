# Slice 07 Plan: Admin Role Assignment UI

## Overview

Slice 07 extends the built-in admin user management UI with narrowly scoped role assignment for the fixed `Administrator` role. The goal is to let an existing administrator grant or remove administrator access from user detail pages without introducing a general role-management system, custom permissions, invitations, audit storage, or API endpoints.

## Scope

In scope:

- Show whether the viewed user is currently in the `Administrator` role.
- Add POST-only admin actions to grant and remove the `Administrator` role.
- Create the `Administrator` role if it is missing before granting it.
- Prevent removing the last remaining administrator.
- Keep role assignment inside the existing `/auth/admin/users/{id}` detail page.
- Add integration tests for grant, revoke, last-admin protection, and non-admin denial.
- Update user, developer, slice, architecture, and context docs.

Out of scope:

- Managing arbitrary roles.
- Configuring the admin role name.
- Fine-grained permissions.
- User invitation or creation flow.
- User deletion.
- Impersonation.
- Audit event storage.
- API endpoints for admin features.

## Proposed UX

On `/auth/admin/users/{id}`:

- Continue showing the existing role list.
- Add an administrator-access section with the current state.
- If the target is not an administrator, show a `Grant administrator` POST action.
- If the target is an administrator, show a `Remove administrator` POST action.
- If removing would leave zero administrators, keep the user in role and show a validation message.

## Architecture Decisions

- Continue using the fixed ASP.NET Core Identity role name `Administrator`.
- Use `UserManager<AuthNetUser>` and `RoleManager<IdentityRole>` directly; do not add a service abstraction until the behavior is reused.
- Keep actions on `DetailModel` because role assignment is a user-detail operation in this slice.
- Count current administrators through Identity/EF Core role membership data before removal.
- Keep mutations POST-only and rely on Razor Pages antiforgery behavior.
- Use integration tests against the existing in-memory test host to cover authorization and Identity state changes.

## Task List

### Phase 1: Role State Foundation

- [x] Task 1: Add detail-page role assignment state.
- [x] Task 2: Add administrator grant action.

### Checkpoint: Grant Flow

- [x] Focused admin tests pass for detail page and grant behavior.
- [x] Existing admin detail actions still work.

### Phase 2: Safe Removal

- [x] Task 3: Add administrator removal action.
- [x] Task 4: Add last-admin protection.

### Checkpoint: Revoke Flow

- [x] Admin can remove administrator access when another admin remains.
- [x] The last administrator cannot be removed.
- [x] Non-admin users cannot post role assignment actions.

### Phase 3: Docs and Closeout

- [x] Task 5: Expand admin integration tests.
- [x] Task 6: Update user and developer documentation.
- [x] Task 7: Update architecture, slice, and context docs.
- [x] Task 8: Final verification and local review.
- [x] Task 9: Commit Slice 07.

### Checkpoint: Complete

- [x] `.\scripts\verify.ps1` passes.
- [x] Admin role assignment remains limited to the fixed `Administrator` role.
- [x] No custom permission system, API surface, or invitation flow is introduced.
- [x] Slice 07 artifacts are complete and named with `slice-07`.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Removing all admins locks the built-in admin UI | High | Block removal when the target is the last current administrator. |
| Scope grows into general role management | High | Only expose the fixed `Administrator` role and document arbitrary role management as out of scope. |
| Admin role does not exist before assignment | Medium | Create `Administrator` role before grant, matching bootstrap behavior. |
| Race condition during last-admin removal | Medium | Re-check administrator count immediately before removal and treat failure as validation error. |
| UI lets non-admins mutate role membership | High | Keep `[Authorize(Roles = "Administrator")]` and add non-admin POST tests. |

## Open Questions

- Should an admin be allowed to remove their own administrator role when another administrator remains?
- Should the UI show a stronger warning when removing administrator access from the current signed-in user?

## Recommended Default

Allow self-removal only when another administrator remains, because the invariant that matters for this slice is preserving at least one administrator. Keep the UI message plain and avoid adding confirmation dialogs until there is a broader admin UX pattern.
