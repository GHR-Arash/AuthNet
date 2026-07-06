# AuthNet Agent Instructions

## Project Context

Before making architecture, package structure, authentication, persistence, or public API changes, read:

- `docs/next-iteration-context.md`
- `docs/architecture-context.md`
- `docs/product-decisions.md`
- The relevant section of `docs/functional-requirements.md`

## Architecture Context Maintenance

Keep `docs/architecture-context.md` compact and current as the project evolves.
Keep `docs/next-iteration-context.md` compact and current when finishing a meaningful implementation milestone.

Update it when:

- A major architecture decision changes.
- A package/project is added, removed, or renamed.
- Authentication, persistence, UI, or external provider strategy changes.
- A deferred feature becomes active scope.
- A new development command or test command becomes canonical.

Do not expand it into a full spec. Prefer short bullets and links to detailed docs.

## Planning Artifact Naming

Name planning and task artifacts by slice number so progress is traceable:

- `tasks/slice-02-plan.md`
- `tasks/slice-02-todo.md`

If a slice needs phase-specific artifacts, include both slice and phase:

- `tasks/slice-02-phase-01-plan.md`
- `tasks/slice-02-phase-01-todo.md`
