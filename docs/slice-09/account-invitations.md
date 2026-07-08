# Slice 09: Account Invitations

Slice 09 adds an administrator-driven account invitation flow.

## Implemented Capabilities

- Admin-only invitation list at `/auth/admin/invitations`.
- Admin-only invitation create page at `/auth/admin/invitations/new`.
- Anonymous invitation acceptance page at `/auth/invitations/accept`.
- Persisted invitation records in `AuthNet.Persistence.Postgres`.
- High-entropy invitation tokens with only SHA-256 token hashes stored.
- Invitation emails sent through `IAuthNetEmailSender`.
- Duplicate pending invitations rejected by normalized email.
- Existing users cannot be invited again.
- Accepted and expired invitations cannot be used.
- Successful acceptance creates an `AuthNetUser`, marks the invited email confirmed, marks the invitation accepted, and signs in the new user.

## Persistence

Invitation records are stored in `AuthNetInvitations` with:

- Email and normalized email.
- Token hash.
- Created and expiration timestamps.
- Accepted timestamp.
- Creator and accepted user identifiers.

The default invitation expiration is 7 days through `AuthNetOptions.Invitations.Expiration`.

## Non-Goals

- Bulk invitations.
- Role assignment during invite.
- Invitation resend/cancel workflows.
- Organization or team membership.
- API/JWT/SPA invitation endpoints.
- External-provider-only invitation acceptance.

## Verification

Focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetInvitationTests
```

Route tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetRouteTests
```
