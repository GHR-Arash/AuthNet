# Slice 20 Todo: Committed Package Consumer Sample

## Task 1: Decide package-consumer restore strategy

**Description:** Choose how the committed sample will restore AuthNet packages from local package artifacts without affecting normal root solution restore.

**Acceptance criteria:**

- [x] Decision is documented in the Slice 20 plan or sample README.
- [x] Restore strategy uses locally packed packages from `artifacts/packages`.
- [x] Strategy does not require public NuGet publication.
- [x] Strategy does not make `AuthNet.slnx` restore fail before packages are generated.

**Verification:**

- [x] Manual review confirms the sample is not added to root solution restore unless package prerequisites are handled.

**Dependencies:** None

**Files likely touched:**

- `tasks/slice-20-plan.md`
- `samples/AuthNet.PackageConsumer/README.md`

**Estimated scope:** Small

## Task 2: Add package-consumer sample project

**Description:** Add a committed ASP.NET Core sample project under `samples/` that references AuthNet through package references.

**Acceptance criteria:**

- [x] Sample project is committed under `samples/AuthNet.PackageConsumer` or an equivalent clear path.
- [x] Sample project references `AuthNet.AspNetCore` version `0.1.0` as a package.
- [x] Sample project has no `ProjectReference` to AuthNet source projects.
- [x] Sample `bin` and `obj` outputs remain ignored.

**Verification:**

- [x] `Select-String -Path samples\AuthNet.PackageConsumer\AuthNet.PackageConsumer.csproj -Pattern "PackageReference Include=\"AuthNet.AspNetCore\""`
- [x] `Select-String -Path samples\AuthNet.PackageConsumer\AuthNet.PackageConsumer.csproj -Pattern "ProjectReference"` returns no matches.

**Dependencies:** Task 1

**Files likely touched:**

- `samples/AuthNet.PackageConsumer/AuthNet.PackageConsumer.csproj`
- `samples/AuthNet.PackageConsumer/appsettings.json`

**Estimated scope:** Small

## Task 3: Add minimal package-consumer host code

**Description:** Add minimal ASP.NET Core host code that compiles against package-consumed AuthNet APIs and standard ASP.NET authentication/authorization.

**Acceptance criteria:**

- [x] Sample calls `AddAuthNet`.
- [x] Sample calls `UseAuthentication`.
- [x] Sample calls `UseAuthorization`.
- [x] Sample calls `MapAuthNet`.
- [x] Sample includes at least one host-owned protected endpoint or page.
- [x] Sample configuration uses placeholders for PostgreSQL and email settings without committing secrets.

**Verification:**

- [x] Sample project builds once local packages are restored.

**Dependencies:** Task 2

**Files likely touched:**

- `samples/AuthNet.PackageConsumer/Program.cs`
- `samples/AuthNet.PackageConsumer/appsettings.json`

**Estimated scope:** Small

## Task 4: Add focused package-consumer verification command or script

**Description:** Add a repeatable command or script that verifies the committed sample against locally packed AuthNet packages.

**Acceptance criteria:**

- [x] Verification first ensures AuthNet packages exist in `artifacts/packages`.
- [x] Verification restores the sample from `artifacts/packages`.
- [x] Verification builds the sample without project references.
- [x] Failure output is clear when local packages have not been generated.

**Verification:**

- [x] Focused package-consumer verification command succeeds after packages are generated.

**Dependencies:** Tasks 2, 3

**Files likely touched:**

- `scripts/`
- `docs/developer/quick-start.md`
- `samples/AuthNet.PackageConsumer/README.md`

**Estimated scope:** Medium

## Task 5: Decide whether to integrate focused check into canonical verification

**Description:** Determine whether the package-consumer sample check should run inside `scripts/verify.ps1` after package packing, or remain a documented focused command.

**Acceptance criteria:**

- [x] Decision is documented.
- [x] If integrated, `scripts/verify.ps1` still restores/builds/tests/packs in a valid order.
- [x] If not integrated, developer docs clearly list the focused verification command.
- [x] Existing package output verification remains intact.

**Verification:**

- [x] `.\scripts\verify.ps1` succeeds if modified.
- [x] Focused package-consumer verification succeeds.

**Dependencies:** Task 4

**Files likely touched:**

- `scripts/verify.ps1`
- `docs/developer/quick-start.md`
- `docs/slice-03/package-consumption-smoke.md`

**Estimated scope:** Small

## Task 6: Document package-consumer sample usage

**Description:** Add sample-local and developer documentation explaining how to pack AuthNet locally, restore the sample, build it, and what the sample proves.

**Acceptance criteria:**

- [x] Sample README explains the local package source workflow.
- [x] Developer quick start lists focused package-consumer verification.
- [x] Docs state the sample is package-consumption proof, not public NuGet publication.
- [x] Docs state runtime PostgreSQL setup is not required for build-only package verification.

**Verification:**

- [x] Manual doc read-through confirms commands match the actual sample path and package version.

**Dependencies:** Tasks 4, 5

**Files likely touched:**

- `samples/AuthNet.PackageConsumer/README.md`
- `docs/developer/quick-start.md`
- `docs/slice-03/package-consumption-smoke.md`

**Estimated scope:** Medium

## Task 7: Update user and package-readiness docs

**Description:** Update package-related documentation to distinguish the committed package-consumer sample from ignored generated package artifacts.

**Acceptance criteria:**

- [x] User docs mention the package-consumer sample where useful.
- [x] Package-consumption smoke docs no longer describe the only smoke app as ignored if a committed sample exists.
- [x] Package-readiness docs list `AuthNet.Api` as packable if still current.
- [x] Docs continue to state generated `.nupkg` files stay ignored.

**Verification:**

- [x] Manual doc read-through confirms package names and commands match current project state.

**Dependencies:** Task 6

**Files likely touched:**

- `docs/users/getting-started.md`
- `docs/slice-03/package-consumption-smoke.md`
- `docs/slice-03/package-readiness.md`

**Estimated scope:** Medium

## Task 8: Update compact context

**Description:** Update project memory to record the committed package-consumer sample and its verification command.

**Acceptance criteria:**

- [x] `docs/architecture-context.md` records the sample if it changes the project/package map.
- [x] `docs/next-iteration-context.md` records Slice 20 completion when implementation is complete.
- [x] `context.md` records the committed sample and latest verification result.
- [x] Future admin APIs remain deferred and outside Slice 20.

**Verification:**

- [x] Manual read-through confirms compact docs are current and not expanded into a full spec.

**Dependencies:** Tasks 2-7

**Files likely touched:**

- `docs/architecture-context.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** Small

## Task 9: Run focused and full verification

**Description:** Run the package-consumer verification path, canonical full verification, and whitespace checks.

**Acceptance criteria:**

- [x] Focused package-consumer sample restore/build passes.
- [x] `.\scripts\verify.ps1` passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 20 changes plus pre-existing unrelated user changes.

**Verification:**

- [x] Focused package-consumer verification command.
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1-8

**Files likely touched:**

- All Slice 20 files

**Estimated scope:** Small

## Task 10: Final review and commit

**Description:** Review the Slice 20 diff for package-consumption correctness, ensure unrelated local changes are not staged, and commit the completed slice.

**Acceptance criteria:**

- [x] Diff contains only intended Slice 20 sample, docs, script, and task changes.
- [x] Pre-existing unrelated local changes remain unstaged.
- [x] Commit message describes the package-consumer sample outcome.
- [x] Working tree after commit contains only pre-existing unrelated local changes.

**Verification:**

- [x] `git diff --cached --name-only`
- [x] `git commit -m "Add package consumer sample"`
- [x] `git status --short`

**Dependencies:** Task 9

**Files likely touched:**

- Git index only

**Estimated scope:** Small
