# Next Iteration Context

Compact memory for future development sessions. Read this first, then `docs/architecture-context.md` only if deeper context is needed.

## Current State

- Repo is a Git repository on `master`.
- Latest known commits:
  - Current commit: Add development InMemory sample persistence
  - `e50dadf Add Slice 03 package readiness`
  - `7afcf99 Add Slice 02 integration hardening`
  - `364f22a Harden account verification flows`
- Slice 01/MVP account flows are implemented and `docs/tasks.md` is checked off.
- Slice 02 integration hardening is implemented and tracked in `tasks/slice-02-plan.md` and `tasks/slice-02-todo.md`.
- Slice 03 package readiness is implemented and tracked in `tasks/slice-03-plan.md`, `tasks/slice-03-todo.md`, and `docs/slice-03/`.
- Slice 04 development InMemory sample persistence is implemented and tracked in `tasks/slice-04-plan.md`, `tasks/slice-04-todo.md`, and `docs/slice-04/`.
- Slice 05 CI and publication metadata readiness is implemented and tracked in `tasks/slice-05-plan.md`, `tasks/slice-05-todo.md`, and `docs/slice-05/`.

## Implemented Product Surface

- .NET 10 solution: `AuthNet.slnx`.
- Project-local SDK: `.dotnet\dotnet.exe` (ignored).
- Projects:
  - `src/AuthNet.Core`
  - `src/AuthNet.AspNetCore`
  - `src/AuthNet.Persistence.Postgres`
  - `src/AuthNet.UI.Razor`
  - `src/AuthNet.ExternalProviders`
  - `samples/AuthNet.SampleHost`
  - `tests/AuthNet.Tests`
- Built-in Razor Pages account UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account`.
- Account UI includes login/logout, registration, confirm/resend email, forgot/reset password, profile, verified email change, change password, and external login/linking.
- ASP.NET Core Identity user/context in `AuthNet.Persistence.Postgres`.
- Initial PostgreSQL Identity migration exists in `src/AuthNet.Persistence.Postgres/Migrations`.
- Generic OpenID Connect extension exists in `AuthNet.ExternalProviders`.
- External login signs in already linked accounts, lets authenticated users link from profile, and no longer links existing local accounts by email alone.
- Sample host wires `AddAuthNet`, `UseAuthentication`, `UseAuthorization`, and `MapAuthNet`.
- `UseAuthNet()` remains as a compatibility wrapper.
- AuthNet UI ships fallback shared `_Layout.cshtml` and `_ValidationScriptsPartial.cshtml` so account pages render in a bare host.
- `AuthNet.Tests` has an in-memory integration test host covering routes, registration, confirm/resend email, forgot password, profile update, verified email change, external-login safety, and endpoint mapping compatibility.
- MVP packable packages are `AuthNet.Core`, `AuthNet.AspNetCore`, `AuthNet.UI.Razor`, `AuthNet.Persistence.Postgres`, and `AuthNet.ExternalProviders`.
- Package metadata is centralized in `Directory.Build.props`; local packages output to ignored `artifacts/packages`.
- Sample host supports Development-only EF Core InMemory via `AuthNet:UseInMemoryDatabase=true`; PostgreSQL remains the default production/package persistence path.
- Local verification is centralized in `scripts/verify.ps1`.
- GitHub Actions verify-only CI exists at `.github/workflows/ci.yml`; it does not publish or upload packages.

## Current Verification

Known passing commands:

```powershell
.\scripts\verify.ps1
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
```

Latest full test count: 43 passing tests.

Slice 04 focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter SampleHostAuthNetPersistenceTests
```

Known passing package commands:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore
.\.dotnet\dotnet.exe pack src\AuthNet.Core\AuthNet.Core.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj --configuration Release --no-build --output .\artifacts\packages
```

Known passing package-consumer smoke:

```powershell
.\.dotnet\dotnet.exe build artifacts\package-smoke\AuthNet.PackageSmoke.csproj --no-restore
```

Known passing sample startup:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --no-build --no-launch-profile --urls http://127.0.0.1:5127
```

Expected startup signal:

```text
Now listening on: http://127.0.0.1:5127
Application started.
```

## Important Constraints

- Keep MVP slice 1 server-rendered and cookie-based.
- Do not add API/JWT/refresh-token/SPA flows unless explicitly re-scoped.
- Do not replace ASP.NET Core Identity primitives.
- PostgreSQL/EF Core is the only persistence path for now.
- PostgreSQL/EF Core is the production/default persistence path.
- Integration tests and sample-host Development mode can use EF Core InMemory; this is not a production persistence provider.
- Production must use a real `IAuthNetEmailSender`; development sender is rejected in production.
- Public registration remains disabled by default.
- External auto-provisioning requires a verified provider email claim.
- Keep `docs/architecture-context.md` compact and synchronized when architecture changes.

## Documentation Map

For library consumers:

- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/users/account-pages.md`

For contributors:

- `docs/developer/onboarding.md`
- `docs/developer/quick-start.md`

For product/architecture:

- `docs/architecture-context.md`
- `docs/product-decisions.md`
- `tasks/slice-02-plan.md`
- `tasks/slice-02-todo.md`
- `tasks/slice-03-plan.md`
- `tasks/slice-03-todo.md`
- `tasks/slice-04-plan.md`
- `tasks/slice-04-todo.md`
- `tasks/slice-05-plan.md`
- `tasks/slice-05-todo.md`
- `docs/slice-03/package-readiness.md`
- `docs/slice-03/package-consumption-smoke.md`
- `docs/slice-04/development-inmemory.md`
- `docs/slice-05/ci-and-publication-readiness.md`
- `docs/tasks.md`
- `docs/prd.md`

## Likely Next Work

Recommended Slice 06:

- Decide repository URL and license metadata before public package publication.
- Decide whether XML documentation generation should be required before package publishing.
- Add a release/publish workflow after repository URL, license, and package ownership are confirmed.

Other candidates:

- Add a real email sender sample implementation.
- Add a committed package-consumer sample if local smoke coverage should be permanent.

Before starting any next feature, check whether it belongs to MVP slice 1 or deferred scope.
