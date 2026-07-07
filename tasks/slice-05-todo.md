# Slice 05 Todo: CI and Publication Metadata Readiness

## Task 1: Add local verification script

**Description:** Add a PowerShell script that runs the canonical local verification path from a clean command: restore, build, test, Release build, and pack.

**Acceptance criteria:**
- [x] Script works from the repository root.
- [x] Script uses the project-local `.dotnet\dotnet.exe` when present.
- [x] Script exits non-zero on restore, build, test, or pack failure.

**Verification:**
- [x] `.\scripts\verify.ps1`

**Dependencies:** None

**Files likely touched:**
- `scripts/verify.ps1`

**Estimated scope:** S

## Task 2: Add package output inspection to local verification

**Description:** Extend the verification script to assert that exactly the intended five package artifacts are created.

**Acceptance criteria:**
- [x] Script checks for the five expected `.nupkg` files.
- [x] Script rejects missing or extra AuthNet package artifacts.
- [x] Script leaves generated packages under ignored `artifacts/packages`.

**Verification:**
- [x] `.\scripts\verify.ps1`

**Dependencies:** Task 1

**Files likely touched:**
- `scripts/verify.ps1`

**Estimated scope:** S

## Task 3: Add GitHub Actions build workflow

**Description:** Add a workflow for pushes and pull requests that restores, builds, and tests the solution on a hosted runner.

**Acceptance criteria:**
- [x] Workflow runs on pull requests and pushes to `master`.
- [x] Workflow uses a pinned .NET 10 SDK.
- [x] Workflow does not require secrets.

**Verification:**
- [x] Workflow YAML review confirms commands match local verification intent.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `.github/workflows/ci.yml`

**Estimated scope:** S

## Task 4: Add CI package artifact checks

**Description:** Extend CI to build Release and pack the five intended packages, then assert expected package files exist.

**Acceptance criteria:**
- [x] CI creates exactly the five intended package artifacts.
- [x] Sample and test projects are not packaged.
- [x] CI does not publish or upload packages unless explicitly chosen.

**Verification:**
- [x] Workflow command review.
- [x] Local verification script passes.

**Dependencies:** Task 3

**Files likely touched:**
- `.github/workflows/ci.yml`
- Possibly `scripts/verify.ps1`

**Estimated scope:** S

## Task 5: Record publication metadata decisions

**Description:** Create Slice 05 documentation that records what metadata is confirmed, what is still owner-required, and what should happen before public NuGet publication.

**Acceptance criteria:**
- [x] Repository URL requirement is documented.
- [x] License decision requirement is documented.
- [x] XML documentation decision is documented.
- [x] Publishing remains out of scope.

**Verification:**
- [x] Manual doc review confirms no guessed public metadata.

**Dependencies:** None

**Files likely touched:**
- `docs/slice-05/ci-and-publication-readiness.md`

**Estimated scope:** S

## Task 6: Add safe package metadata improvements

**Description:** Add metadata improvements that do not require external owner decisions, such as package release notes or neutral package docs references, while leaving license and repository URL deferred if unconfirmed.

**Acceptance criteria:**
- [x] Package metadata still packs without warnings where possible.
- [x] No fake repository URL or license is introduced.
- [x] Any remaining NuGet publication blocker is documented.

**Verification:**
- [x] Local pack succeeds.
- [x] Package metadata is inspected.

**Dependencies:** Task 5

**Files likely touched:**
- `Directory.Build.props`
- `docs/slice-05/ci-and-publication-readiness.md`

**Estimated scope:** S

## Task 7: Update developer docs and README

**Description:** Update contributor-facing docs to use the new verification script and mention the CI workflow.

**Acceptance criteria:**
- [x] README points to `scripts/verify.ps1`.
- [x] Developer quick start lists the verification script as canonical.
- [x] Existing direct commands remain available for troubleshooting.

**Verification:**
- [x] Manual doc review.

**Dependencies:** Tasks 1 through 4

**Files likely touched:**
- `README.md`
- `docs/developer/quick-start.md`

**Estimated scope:** S

## Task 8: Update architecture, next-iteration context, and `context.md`

**Description:** Record the CI and publication-readiness state in compact project memory.

**Acceptance criteria:**
- [x] Architecture context references the canonical verification script and CI workflow.
- [x] Next iteration context records Slice 05 status and likely next work.
- [x] `context.md` reflects the current verification and publication blockers.

**Verification:**
- [x] Manual read-through confirms context docs are compact and synchronized.

**Dependencies:** Tasks 1 through 7

**Files likely touched:**
- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 9: Final verification and local review

**Description:** Run the completed verification path and review for CI, package, and documentation drift.

**Acceptance criteria:**
- [x] Local verification script passes.
- [x] `git diff --check` passes.
- [x] CI workflow does not require secrets or publish packages.
- [x] Package metadata blockers are documented.

**Verification:**
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`

**Dependencies:** Tasks 1 through 8

**Files likely touched:** None unless review finds issues

**Estimated scope:** S

## Task 10: Commit Slice 05

**Description:** Commit the completed CI and publication metadata readiness slice.

**Acceptance criteria:**
- [x] Commit includes only intended Slice 05 changes.
- [x] Commit message describes CI and publication readiness.
- [x] Working tree is clean except unrelated user-owned changes.

**Verification:**
- [x] `git status --short`
- [x] `git log --oneline -1`

**Dependencies:** Task 9

**Files likely touched:** Git metadata only

**Estimated scope:** XS
