# Slice 12 Plan: Real Email Sender Sample

## Overview

Slice 12 adds a production-style email sender example to the sample host. AuthNet already has email-driven flows for confirmation, password recovery, email change verification, and invitations; the sample should show how a host application wires a real sender while preserving the development sender as the default local path.

## Scope

In scope:

- Add a sample-host SMTP email sender implementation.
- Add sample-host email configuration under `AuthNet:Email:Smtp`.
- Register SMTP sender when `UseDevelopmentEmailSender=false` and SMTP config is enabled.
- Keep the development email sender as the default for local development.
- Validate production-like configuration so the sample fails clearly when real email is required but incomplete.
- Document environment-variable based setup for SMTP credentials.
- Add tests for sample-host registration and validation behavior.
- Update sample docs and project memory.

Out of scope:

- SendGrid, Mailgun, SES, or provider-specific packages.
- Background queueing, retry policy, and dead-letter handling.
- Email template editor or branding/theme system.
- Attachments.
- Inbound email/webhook handling.
- Secret management beyond normal configuration and environment variables.
- Changing AuthNet package email abstractions.

## Architecture Decisions

- Keep SMTP sender code in `samples/AuthNet.SampleHost`; this is a sample integration, not package behavior.
- Use the existing `IAuthNetEmailSender` contract.
- Prefer BCL `SmtpClient` for the sample to avoid adding third-party package surface.
- Bind sender settings from configuration and validate them before registration.
- Do not log or expose SMTP passwords.
- Keep sample Development mode defaulting to `UseDevelopmentEmailSender=true`.

## Configuration Shape

Recommended sample configuration:

```json
"AuthNet": {
  "UseDevelopmentEmailSender": false,
  "Email": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.example.com",
      "Port": 587,
      "UserName": "",
      "Password": "",
      "FromEmail": "no-reply@example.com",
      "FromName": "AuthNet Sample",
      "EnableSsl": true
    }
  }
}
```

Recommended secret injection:

```powershell
$env:AuthNet__Email__Smtp__Password='...'
```

## Task List

### Phase 1: Sample Email Foundation

- [x] Task 1: Add sample SMTP options and validation helper.
- [x] Task 2: Add sample SMTP email sender.
- [x] Task 3: Register development or SMTP sender based on sample configuration.

### Checkpoint: Foundation

- [x] Sample app can still run with development email sender by default.
- [x] Sample app registers SMTP sender when configured.
- [x] Production-like missing SMTP settings fail clearly.

### Phase 2: Tests and Documentation

- [x] Task 4: Add sample email registration tests.
- [x] Task 5: Update user/developer docs and sample config docs.
- [x] Task 6: Update project memory and next-iteration direction.

### Checkpoint: Complete

- [x] Focused sample email tests pass.
- [x] Full verification passes.
- [x] Slice 12 artifacts are complete and named with `slice-12`.
- [x] No provider-specific package, queueing, retry, template editor, or package email abstraction change is introduced.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Sample accidentally becomes package behavior | Medium | Keep implementation and tests in `samples/AuthNet.SampleHost` and sample-focused tests. |
| Credentials leak into source | High | Document environment variables and leave password empty in JSON. |
| SMTP test tries to send real mail | Medium | Test registration/options, not external network delivery. |
| `SmtpClient` limitations confuse production guidance | Low | Present it as sample wiring, not a recommended universal provider. |
| Production config silently uses development sender | High | Validate production-like sample config explicitly. |

## Next Slice Direction

Per product direction, the next planned area after Slice 12 is role creation, role assignment, and permission-system enhancement. That should be planned as a separate Slice 13 because it changes authorization semantics and should not be mixed with email-sender sample work.
