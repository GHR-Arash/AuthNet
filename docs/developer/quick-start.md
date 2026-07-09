# Developer Quick Start

Use this when you only need to restore, build, run, and verify AuthNet locally.

## Prerequisites

- Windows PowerShell.
- PostgreSQL if you want to exercise real account flows.
- The repo-local .NET 10 SDK at `.dotnet\dotnet.exe`.

If `.dotnet\dotnet.exe` is missing, install the local SDK:

```powershell
$installScript = Join-Path $env:TEMP "dotnet-install-$([guid]::NewGuid()).ps1"
try {
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installScript
    & $installScript -Channel '10.0' -Quality 'ga' -InstallDir '.dotnet'
}
finally {
    if (Test-Path -LiteralPath $installScript) {
        Remove-Item -LiteralPath $installScript -Force
    }
}
```

## Restore, Build, Test

From the repo root:

```powershell
.\scripts\verify.ps1
```

This is the canonical local verification path. It restores, builds, tests, builds Release, packs the intended packages, and verifies the package output.
It also verifies package metadata and restores/builds the committed package-consumer sample from the freshly packed local packages.

For troubleshooting, run the individual commands:

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
```

Expected result:

- Build: `0 Warning(s)`, `0 Error(s)`.
- Tests: all tests pass.

Focused MFA tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetMfaTests
```

Focused invitation tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests
```

Focused admin user tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAdminUserTests
```

Focused role tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetRoleTests
```

Focused permission tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetPermissionTests
```

Focused audit tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests
```

Focused SPA API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter AuthNetSpaApiTests
```

Focused SPA account workflow tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaAccountWorkflowTests
```

Focused SPA MFA API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaMfaApiTests
```

Focused SPA external-login API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaExternalLoginApiTests
```

Focused SPA invitation API tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetSpaInvitationApiTests
```

Focused OpenAPI tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetOpenApiTests
```

Focused sample email sender tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostEmailSenderTests
```

Focused fluent startup tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests
```

## Pack Local NuGet Artifacts

Build Release first, then pack the intended package projects into ignored local artifacts:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore
.\.dotnet\dotnet.exe pack src\AuthNet.Core\AuthNet.Core.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Api\AuthNet.Api.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj --configuration Release --no-build --output .\artifacts\packages
```

Expected packages:

- `AuthNet.Core`
- `AuthNet.ExternalProviders`
- `AuthNet.Persistence.Postgres`
- `AuthNet.UI.Razor`
- `AuthNet.Api`
- `AuthNet.AspNetCore`

`AuthNet.SampleHost` and `AuthNet.Tests` are not package artifacts.

Verify the committed package-consumer sample directly after packages exist:

```powershell
.\scripts\verify-package-consumer.ps1
```

The sample lives at `samples\AuthNet.PackageConsumer`, references `AuthNet.AspNetCore` `0.1.0` as a package, and is intentionally not part of `AuthNet.slnx` so root solution restore does not depend on generated local packages.

Verify generated package metadata directly after packages exist:

```powershell
.\scripts\verify-package-metadata.ps1
```

Publish generated packages manually:

```powershell
.\scripts\publish-nuget.ps1 -ApiKey $env:NUGET_API_KEY -SkipDuplicate
```

Strict public-publication metadata verification should pass before packages are published:

```powershell
.\scripts\verify-package-metadata.ps1 -RequirePublicPublicationMetadata
```

## CI

GitHub Actions runs the verify-only workflow in `.github/workflows/ci.yml` for pushes and pull requests to `master`.

GitHub Actions also runs `.github/workflows/nuget-release.yml` on pushes and merges to `master`. That workflow runs verification, packs packages, and publishes them to NuGet with the `NUGET_API_KEY` repository secret.

## Configure PostgreSQL

The sample host default connection string is in `samples/AuthNet.SampleHost/appsettings.json`:

```json
"ConnectionStrings": {
  "AuthNet": "Host=localhost;Port=5432;Database=authnet_sample;Username=postgres;Password=postgres"
}
```

Change this locally if your PostgreSQL credentials differ.

## Development InMemory Sample Mode

In Development, the sample host uses EF Core InMemory by default through `samples/AuthNet.SampleHost/appsettings.Development.json`:

```json
"AuthNet": {
  "UseInMemoryDatabase": true
}
```

This lets you run the sample account UI without a local PostgreSQL server:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Use PostgreSQL for production-like local testing by setting `AuthNet:UseInMemoryDatabase` to `false` or running outside Development with a real connection string.

## Sample SMTP Email Sender

The sample host keeps the development email sender enabled by default for local smoke testing. For production-like email testing, disable it and enable the sample SMTP sender:

```powershell
$env:AuthNet__UseDevelopmentEmailSender='false'
$env:AuthNet__Email__Smtp__Enabled='true'
$env:AuthNet__Email__Smtp__Host='smtp.example.com'
$env:AuthNet__Email__Smtp__Port='587'
$env:AuthNet__Email__Smtp__UserName='smtp-user'
$env:AuthNet__Email__Smtp__Password='smtp-password'
$env:AuthNet__Email__Smtp__FromEmail='no-reply@example.com'
$env:AuthNet__Email__Smtp__FromName='AuthNet Sample'
$env:AuthNet__Email__Smtp__EnableSsl='true'
```

The same configuration shape is shown in `samples/AuthNet.SampleHost/appsettings.SmtpSample.json`. Keep the password empty in committed JSON and provide it through environment variables or another secret provider.

## Initial Administrator Bootstrap

The sample host creates a demo administrator user through the package-owned fluent startup API in `samples\AuthNet.SampleHost\Program.cs`.

Demo admin credentials:

```text
Username: admin
Email: admin@admin.com
Password: Password1!
```

Programmatic demo bootstrap behavior:

- Creates the `Administrator` role if needed.
- Creates `admin@admin.com` with username `admin` if missing.
- Confirms the sample admin email.
- Assigns the sample admin to `Administrator`.

The sample host also keeps optional config-driven initial administrator setup for local override and promotion scenarios:

- `AuthNet:InitialAdministrator:Enabled=false` does nothing.
- `AuthNet:InitialAdministrator:Enabled=true` creates the `Administrator` role if needed.
- `AuthNet:InitialAdministrator:Email` is required and identifies the user to create or promote.
- `AuthNet:InitialAdministrator:UserName` is optional and is used only for a newly created user.
- `AuthNet:InitialAdministrator:Password` is required only when the user does not already exist.
- Existing users can be promoted to `Administrator` without providing a password.

The demo admin is available without setting environment variables:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Then sign in at `/auth/login` with:

```text
admin
Password1!
```

To create or promote an additional configured admin user:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:AuthNet__InitialAdministrator__Enabled='true'
$env:AuthNet__InitialAdministrator__UserName='admin'
$env:AuthNet__InitialAdministrator__Email='admin@example.test'
$env:AuthNet__InitialAdministrator__Password='Password1!'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Open the admin UI:

```text
http://127.0.0.1:5127/auth/admin/users
```

Use the role pages to create roles and assign built-in AuthNet permissions. Use a user's detail page to assign or remove roles. The UI prevents removing the last remaining administrator.

If the user already exists, the bootstrap can assign the `Administrator` role without a password:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:AuthNet__InitialAdministrator__Enabled='true'
$env:AuthNet__InitialAdministrator__Email='existing@example.test'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Focused bootstrap tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetStartupTests
```

Package consumers should use `await app.UseAuthNet(authNet => authNet.InitialAdministrator(...))` for first-admin bootstrap. The package intentionally does not create hardcoded credentials unless the host opts in.

## Apply Database Schema

Install EF tooling into the ignored `.tools/` folder:

```powershell
.\.dotnet\dotnet.exe tool install dotnet-ef --version 10.0.9 --tool-path .tools
```

Apply migrations:

```powershell
.\.tools\dotnet-ef.exe database update --project src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --startup-project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --context AuthNetDbContext
```

Alternative: set `AuthNet:ApplyMigrations` to `true` in the sample host config before running it. The sample host passes that setting into `UseAuthNet(...ApplyMigrations(...))`.

## Run Sample Host

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Open:

```text
http://127.0.0.1:5127
```

The sample home page, navbar, and protected `/Admin` page link to the built-in AuthNet admin screens after you sign in as an administrator.

Useful routes:

- `/auth/login`
- `/auth/register`
- `/auth/forgot-password`
- `/auth/resend-confirmation`
- `/auth/profile`
- `/auth/mfa`
- `/auth/admin/users` after signing in as a user in the `Administrator` role
- `/auth/admin/users/new` to directly create a local user
- `/auth/admin/users/{id}` to view user state and manage role assignment
- `/auth/admin/roles` to list roles
- `/auth/admin/roles/new` to create a role
- `/auth/admin/roles/{id}` to manage built-in AuthNet permissions on a role
- `/auth/admin/audit` to review admin audit events
- `/auth/admin/invitations` to list account invitations
- `/auth/admin/invitations/new` to invite a user
- `/auth/api/session` to inspect same-origin SPA session JSON
- `/auth/api/profile` to inspect authenticated SPA profile JSON
- `/auth/api/reset-password` to complete password reset from a reset email code
- `/auth/api/confirm-email` to complete email confirmation from a confirmation email code
- `/auth/api/change-password` to change the signed-in user's password
- `/auth/api/mfa` to inspect authenticated SPA MFA state
- `/auth/api/mfa/setup/start` to start authenticator-app setup
- `/auth/api/mfa/setup/verify` to enable MFA with an authenticator code
- `/auth/api/login/mfa` to complete an MFA challenge after password login
- `/auth/api/login/recovery-code` to complete sign-in with a recovery code
- `/auth/api/external-providers` to inspect configured external providers
- `/auth/api/external-login/challenge` to start SPA external sign-in
- `/auth/api/external-login/callback` to complete SPA external sign-in callback handling
- `/auth/api/external-login/link/challenge` to start signed-in external account linking
- `/auth/api/external-login/link/callback` to complete signed-in external account linking
- `/auth/api/invitations/accept` to inspect and accept invitation tokens from a same-origin SPA
- `/auth/api/openapi.json` to inspect the AuthNet SPA API OpenAPI document
- `/Spa` to exercise the sample same-origin SPA workflow
- `/Admin`

The integration test suite uses isolated EF Core InMemory databases through the AuthNet test host. The sample host also supports Development-only InMemory mode for local manual account-flow smoke testing.

The sample host creates the demo admin user in code at startup. This is sample-host-only behavior and is intentionally separate from AuthNet package behavior.

## Sample AuthNet Configuration

```json
"AuthNet": {
  "ApplicationName": "AuthNet Sample",
  "AccountRoutePrefix": "/auth",
  "EnablePublicRegistration": true,
  "UseDevelopmentEmailSender": true,
  "RequireConfirmedEmail": true,
  "ApplyMigrations": false,
  "Email": {
    "Smtp": {
      "Enabled": false,
      "Host": "",
      "Port": 587,
      "UserName": "",
      "Password": "",
      "FromEmail": "",
      "FromName": "",
      "EnableSsl": true
    }
  },
  "Invitations": {
    "Expiration": "7.00:00:00"
  },
  "InitialAdministrator": {
    "Enabled": false,
    "UserName": "",
    "Email": "",
    "Password": ""
  },
  "OpenIdConnect": {
    "Enabled": false,
    "DisplayName": "OpenID Connect",
    "Authority": "",
    "ClientId": "",
    "ClientSecret": "",
    "CallbackPath": "/signin-authnet-oidc"
  }
}
```

For production-like sample testing, set `UseDevelopmentEmailSender` to `false` and configure `AuthNet:Email:Smtp`. Package consumers should register their own real `IAuthNetEmailSender`.
