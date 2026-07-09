# Slice 03 Package Consumption Smoke Plan

This document tracks how to verify AuthNet packages from a local package source before any NuGet publication.

## Intent

Prove that a clean ASP.NET Core consumer can install locally packed AuthNet packages, compile the documented setup, and keep using standard ASP.NET Core authentication and authorization.

## Verified Flow

1. Pack AuthNet into a local artifact folder:

```powershell
.\.dotnet\dotnet.exe pack AuthNet.slnx --no-build --output .\artifacts\packages
```

2. Use the committed package-consumer sample:

```text
samples/AuthNet.PackageConsumer
```

3. Restore and build the sample from the local package source:

```powershell
.\scripts\verify-package-consumer.ps1
```

The sample references the primary integration package:

```powershell
AuthNet.AspNetCore 0.1.0
```

`AuthNet.AspNetCore` brings the MVP AuthNet package dependencies transitively.

4. Compile a minimal setup using:

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

- The committed sample should compile from packages, not project references.
- The smoke test does not need a live PostgreSQL database unless runtime startup is selected as part of implementation.
- The smoke flow should not require public NuGet publication.
- Generated `.nupkg`, `bin`, `obj`, and restore artifacts stay ignored.

## Latest Result

Passed:

```powershell
.\scripts\verify-package-consumer.ps1
```
