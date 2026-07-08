# Slice 12 Todo: Real Email Sender Sample

## Task 1: Add sample SMTP options and validation helper

**Description:** Add a sample-host-only configuration model and validation helper for SMTP email settings.

**Acceptance criteria:**
- [x] SMTP settings bind from `AuthNet:Email:Smtp`.
- [x] Required settings include host, port, from email, and from name when SMTP is enabled.
- [x] Password is optional only when the SMTP server does not need credentials.
- [x] Invalid SMTP settings produce clear sample-host validation errors.

**Verification:**
- [x] Focused tests cover valid settings and missing required settings.

**Dependencies:** None

**Files likely touched:**
- `samples/AuthNet.SampleHost/SampleHostSmtpEmailOptions.cs`
- `tests/AuthNet.Tests/SampleHostEmailSenderTests.cs`

**Estimated scope:** S

## Task 2: Add sample SMTP email sender

**Description:** Implement `IAuthNetEmailSender` in the sample host using SMTP configuration.

**Acceptance criteria:**
- [x] Sender uses the existing `IAuthNetEmailSender` contract.
- [x] Sender maps AuthNet email messages to SMTP messages.
- [x] Sender sets from address/name and recipient.
- [x] Sender supports SSL/TLS flag and optional credentials.
- [x] Sender does not log or expose password values.

**Verification:**
- [x] Build succeeds.
- [x] Unit-level tests cover registration and validation behavior without sending email.

**Dependencies:** Task 1

**Files likely touched:**
- `samples/AuthNet.SampleHost/SampleHostSmtpEmailSender.cs`
- `samples/AuthNet.SampleHost/SampleHostSmtpEmailOptions.cs`
- `tests/AuthNet.Tests/SampleHostEmailSenderTests.cs`

**Estimated scope:** M

## Task 3: Register development or SMTP sender based on sample configuration

**Description:** Update sample-host service registration so local development keeps the development sender, while SMTP is registered when configured.

**Acceptance criteria:**
- [x] Development default remains `UseDevelopmentEmailSender=true`.
- [x] When `UseDevelopmentEmailSender=false` and SMTP is enabled, the sample registers the SMTP sender.
- [x] Production-like configuration without development sender and without valid SMTP fails clearly.
- [x] AuthNet package behavior remains unchanged.

**Verification:**
- [x] Focused tests cover development sender default, SMTP sender registration, and invalid production-like config.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `samples/AuthNet.SampleHost/Program.cs`
- `samples/AuthNet.SampleHost/SampleHostAuthNetPersistence.cs` or new sample registration helper
- `tests/AuthNet.Tests/SampleHostEmailSenderTests.cs`

**Estimated scope:** M

## Task 4: Add sample email registration tests

**Description:** Add tests that verify sample-host email sender configuration without sending real email.

**Acceptance criteria:**
- [x] Tests verify development email sender remains the default local path.
- [x] Tests verify SMTP sender registration when configured.
- [x] Tests verify missing SMTP host/from settings fail.
- [x] Tests verify password values are not included in exception text.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostEmailSenderTests`

**Dependencies:** Tasks 1 through 3

**Files likely touched:**
- `tests/AuthNet.Tests/SampleHostEmailSenderTests.cs`
- Existing sample-host test helpers if needed

**Estimated scope:** M

## Task 5: Update user/developer docs and sample config docs

**Description:** Document SMTP sample configuration and production-like setup.

**Acceptance criteria:**
- [x] Developer quick start documents sample SMTP configuration.
- [x] User getting started guide explains host apps must provide a real email sender in production.
- [x] Sample config shows empty SMTP settings without committing secrets.
- [x] Docs use environment variables for password examples.

**Verification:**
- [x] Manual read-through for stale guidance implying only development sender exists.

**Dependencies:** Tasks 1 through 4

**Files likely touched:**
- `docs/developer/quick-start.md`
- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `samples/AuthNet.SampleHost/appsettings.json`
- `samples/AuthNet.SampleHost/appsettings.Development.json`

**Estimated scope:** S

## Task 6: Update project memory and next-iteration direction

**Description:** Record Slice 12 status and preserve the requested Slice 13 direction for role and permission enhancement.

**Acceptance criteria:**
- [x] `docs/next-iteration-context.md` records Slice 12 when complete.
- [x] `context.md` records current sample email sender state.
- [x] Next recommended slice is role creation, role assignment, and permission-system enhancement.
- [x] Architecture context stays compact and current.

**Verification:**
- [x] Manual read-through confirms future role/permission work is not mixed into Slice 12 scope.

**Dependencies:** Tasks 1 through 5

**Files likely touched:**
- `docs/next-iteration-context.md`
- `docs/architecture-context.md`
- `context.md`

**Estimated scope:** S

## Task 7: Final verification and commit

**Description:** Run full verification, review the diff, and commit Slice 12.

**Acceptance criteria:**
- [x] Focused sample email tests pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 12 changes plus pre-existing unrelated user changes.
- [x] Commit includes only intended Slice 12 changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter SampleHostEmailSenderTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 6

**Files likely touched:** None unless review finds issues

**Estimated scope:** S
