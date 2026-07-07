# Slice 03 Package Consumption Smoke Plan

This document tracks how to verify AuthNet packages from a local package source before any NuGet publication.

## Intent

Prove that a clean ASP.NET Core consumer can install locally packed AuthNet packages, compile the documented setup, and keep using standard ASP.NET Core authentication and authorization.

## Verified Flow

1. Pack AuthNet into a local artifact folder:

```powershell
.\.dotnet\dotnet.exe pack AuthNet.slnx --no-build --output .\artifacts\packages
```

2. Create or use a temporary consumer app outside committed source. The current local smoke app is ignored at:

```text
artifacts/package-smoke
```

3. Add `.\artifacts\packages` as a package source for the smoke app.

4. Install the primary integration package:

```powershell
.\.dotnet\dotnet.exe add artifacts\package-smoke\AuthNet.PackageSmoke.csproj package AuthNet.AspNetCore --version 0.1.0 --source C:\Projects\AuthNet\artifacts\packages
```

`AuthNet.AspNetCore` brings the MVP AuthNet package dependencies transitively.

5. Compile a minimal setup using:

```csharp
builder.Services.AddAuthNet(options =>
{
    options.EnablePublicRegistration = false;
});

app.UseAuthentication();
app.UseAuthorization();
app.MapAuthNet();
```

## Acceptance Notes

- The smoke app should compile from packages, not project references.
- The smoke test does not need a live PostgreSQL database unless runtime startup is selected as part of implementation.
- The smoke flow should not require public NuGet publication.

## Latest Result

Passed:

```powershell
.\.dotnet\dotnet.exe build artifacts\package-smoke\AuthNet.PackageSmoke.csproj --no-restore
```
