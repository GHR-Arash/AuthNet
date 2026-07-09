# Slice 22 Plan: Built-In UI Polish

## Goal

Refine the built-in Razor Pages UI so package consumers get a clean, navigable, production-ready default experience without host CSS.

## Scope

- Add a built-in AuthNet home page under the configured route prefix.
- Give the fallback AuthNet layout clear account and administration navigation.
- Ship package-owned CSS for the fallback layout, forms, tables, buttons, detail lists, alerts, and responsive behavior.
- Keep existing account/admin functionality and route behavior intact.
- Update sample-host links and consumer documentation for the new home entry point.

## Out of Scope

- Full page-by-page redesign of every form workflow.
- Host layout replacement APIs beyond the existing `LayoutPath`.
- JavaScript-heavy navigation or client-side state.
- New authentication, authorization, or persistence behavior.

## Verification

- Build the solution.
- Run focused route, login, and admin UI integration tests.
- Start the sample host in Development and verify `/auth` and the package stylesheet render.
