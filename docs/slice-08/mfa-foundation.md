# Slice 08: MFA Foundation

## Intent

Implement the first MFA slice from the Should Have roadmap using ASP.NET Core Identity primitives.

## Implemented Slice

AuthNet now supports user-owned authenticator-app MFA for local password sign-in.

Routes under the configured AuthNet prefix:

- `/auth/mfa`
- `/auth/mfa/setup`
- `/auth/mfa/recovery-codes`
- `/auth/mfa/disable`
- `/auth/login/mfa`
- `/auth/login/recovery-code`

## Capabilities

Authenticated users can:

- View MFA status.
- Set up an authenticator app with a manual key and otpauth URI.
- Verify a TOTP authenticator code to enable MFA.
- Receive recovery codes after setup.
- View recovery-code remaining count.
- Disable MFA.

MFA-enabled users can:

- Complete local password sign-in with an authenticator code.
- Complete local password sign-in with a recovery code.

## Non-Goals

- No SMS OTP.
- No email OTP.
- No push MFA.
- No WebAuthn/passkeys.
- No remember-this-browser flow.
- No admin-managed MFA reset.
- No global required-MFA policy.
- No API/JWT or SPA MFA endpoints.
- No generated QR-code image in this slice.

## Security Model

Use ASP.NET Core Identity:

- `PasswordSignInAsync`
- `RequiresTwoFactor`
- `TwoFactorAuthenticatorSignInAsync`
- `TwoFactorRecoveryCodeSignInAsync`
- `GetAuthenticatorKeyAsync`
- `ResetAuthenticatorKeyAsync`
- `VerifyTwoFactorTokenAsync`
- `SetTwoFactorEnabledAsync`
- `GenerateNewTwoFactorRecoveryCodesAsync`

AuthNet does not implement custom MFA token generation or custom MFA persistence.

## Verification Target

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetMfaTests
.\scripts\verify.ps1
```

## Follow-On Candidates

- Recovery-code regeneration.
- Remember-this-browser support.
- Admin-managed MFA reset.
- Generated QR-code image.
- Required-MFA policy.
