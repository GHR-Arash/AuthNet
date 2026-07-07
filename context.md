# AuthNet Context

## Current Iteration

Slice 03 package readiness is implemented locally.

## Current Package Shape

Packable:

- `AuthNet.Core`
- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Persistence.Postgres`
- `AuthNet.ExternalProviders`

Not packable:

- `AuthNet.SampleHost`
- `AuthNet.Tests`

Deferred:

- `AuthNet.Api`

## Current Verification

Latest known package verification uses Release build plus per-project pack commands into ignored `artifacts/packages`.

Latest package-consumer smoke app is ignored at `artifacts/package-smoke` and compiles against `AuthNet.AspNetCore` `0.1.0` from the local package source.

## Current Persistence Modes

PostgreSQL remains the default production/package persistence path.

Development-only InMemory is implemented for the sample host through `AuthNet:UseInMemoryDatabase=true` in `appsettings.Development.json`.

Slice 04 files:

- `tasks/slice-04-plan.md`
- `tasks/slice-04-todo.md`
- `docs/slice-04/development-inmemory.md`

## Next Attention

- Confirm real repository URL and license metadata before public package publication.
- Add CI for restore, build, test, and package verification.
