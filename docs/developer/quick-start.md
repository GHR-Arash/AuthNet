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
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
```

Expected result:

- Build: `0 Warning(s)`, `0 Error(s)`.
- Tests: all tests pass.

## Pack Local NuGet Artifacts

Build Release first, then pack the intended package projects into ignored local artifacts:

```powershell
.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore
.\.dotnet\dotnet.exe pack src\AuthNet.Core\AuthNet.Core.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj --configuration Release --no-build --output .\artifacts\packages
```

Expected packages:

- `AuthNet.Core`
- `AuthNet.ExternalProviders`
- `AuthNet.Persistence.Postgres`
- `AuthNet.UI.Razor`
- `AuthNet.AspNetCore`

`AuthNet.SampleHost` and `AuthNet.Tests` are not package artifacts.

## Configure PostgreSQL

The sample host default connection string is in `samples/AuthNet.SampleHost/appsettings.json`:

```json
"ConnectionStrings": {
  "AuthNet": "Host=localhost;Port=5432;Database=authnet_sample;Username=postgres;Password=postgres"
}
```

Change this locally if your PostgreSQL credentials differ.

## Apply Database Schema

Install EF tooling into the ignored `.tools/` folder:

```powershell
.\.dotnet\dotnet.exe tool install dotnet-ef --version 10.0.9 --tool-path .tools
```

Apply migrations:

```powershell
.\.tools\dotnet-ef.exe database update --project src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --startup-project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --context AuthNetDbContext
```

Alternative: set `AuthNet:ApplyMigrations` to `true` in the sample host config before running it.

## Run Sample Host

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Open:

```text
http://127.0.0.1:5127
```

Useful routes:

- `/auth/login`
- `/auth/register`
- `/auth/forgot-password`
- `/auth/resend-confirmation`
- `/auth/profile`
- `/Admin`

The integration test suite uses EF Core InMemory through the AuthNet test host. Local manual runs still use PostgreSQL.

## Sample AuthNet Configuration

```json
"AuthNet": {
  "ApplicationName": "AuthNet Sample",
  "AccountRoutePrefix": "/auth",
  "EnablePublicRegistration": true,
  "UseDevelopmentEmailSender": true,
  "RequireConfirmedEmail": true,
  "ApplyMigrations": false,
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

For production-like testing, set `UseDevelopmentEmailSender` to `false` and register a real `IAuthNetEmailSender`.
