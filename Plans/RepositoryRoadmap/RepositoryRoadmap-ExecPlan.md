# Stabilize the repository and deliver the result-screen celebration

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept current while the work proceeds.

Repository planning rules live in `PLANS.md` at the repository root. This document must be maintained in accordance with those rules.

## Purpose / Big Picture

The repository currently combines a healthy `main` branch with accumulated local Git state, transient generated files, stale remote branches and pull requests, and an oversized experimental result-screen VFX branch. The build exporter also writes its default output beneath the source-artifact tree, and the repository has no continuous-integration workflow that can run without a licensed Unity environment.

After this roadmap is complete:

- a fresh checkout stays clean after normal local validation and submission work;
- the production campaign result popup shows a subtle racing-flag pattern and one confetti burst without importing an entire demo or vendor collection;
- useful historical work is preserved through annotated archive tags or a documented future task before obsolete local state is pruned;
- the repository has a credential-free static CI gate now and a documented route to Unity compile and test validation later;
- WebGL builds default to the ignored `Builds/` tree instead of tracked submission artifacts; and
- no public history is rewritten.

The user can see the principal behavior by finishing a campaign race and opening the result popup. The background should carry a low-opacity moving racing-flag pattern, one confetti burst should play when the popup binds, reopening the popup should produce exactly one new burst, and the result text and actions should remain readable.

## Progress

- [x] Audit the working tree, branches, worktrees, stashes, pull requests, large Git objects, generated artifacts, result-screen VFX experiment, validation scripts, and build-export path.
- [x] Confirm that `main` matches `origin/main` and that the only tracked local edit was whitespace-only serialization churn in `ProjectSettings/ProjectSettings.asset`.
- [x] Restore the whitespace-only project-settings edit and move the approved generated directories to a recoverable Trash location.
- [x] Create the `codex/repository-hygiene` implementation branch from refreshed `main`.
- [x] Commit this ExecPlan as a focused documentation commit (`cf654e34`).
- [x] Add and commit ignore rules for recurring Python, Firebase, temporary PDF, and media-marker output (`1fd49953`).
- [ ] Create `codex/result-screen-celebration` from the completed hygiene branch.
- [ ] Register Unity 6000.5.9f1 with the Unity CLI and install the Pipeline package so the running Editor can be driven safely.
- [ ] Import only the approved racing-flag texture from `origin/feature/result-screen-vfx`, using repository naming conventions and retaining its Unity metadata.
- [ ] Configure the production campaign result popup with the racing-flag pattern and the existing production confetti prefab.
- [ ] Update `ResultPopupView` so the confetti plays once per binding and is stopped and cleared on unbind or disable.
- [ ] Update `Docs/Game/Campaign.md` with the result-popup presentation behavior.
- [ ] Run focused C# lint, structure verification, project compilation/analyzer validation, and visual verification for the result popup.
- [ ] Commit the production result-screen celebration as one focused feature commit.
- [ ] Create the Git-maintenance report and preserve each useful stash or unmerged branch before local pruning.
- [ ] Stop for explicit approval before pushing archive tags, closing pull requests, or deleting remote branches.
- [ ] Add the static-only validation mode and credential-free CI workflow.
- [ ] Move the submission exporter's default WebGL build path to `Builds/GearEngineWebGL` and document artifact policy and the later licensed Unity CI phase.
- [ ] Validate and commit the CI and artifact-policy milestone.
- [ ] Complete repository integrity checks and record final outcomes.

## Surprises & Discoveries

- Observation: The tracked change in `ProjectSettings/ProjectSettings.asset` changed 138 lines only because of end-of-line whitespace; `git diff --ignore-space-at-eol` produced no semantic diff.
  Evidence: The baseline audit on 2026-09-15 reported `138 138` in `git diff --numstat` and no output from the whitespace-insensitive diff.

- Observation: The 31 visible untracked entries were generated caches, temporary PDF renders, media markers, or failed/duplicate validation evidence. Several result directories also contained ignored Editor logs, so cleanup had to target the obsolete directories rather than only visible files.
  Evidence: `git status --short`, exact file inspection, and comparison with the retained successful or intentional red/green test artifacts.

- Observation: `origin/feature/result-screen-vfx` is one very large commit with more than 300 changed files and over one million changed lines. It imports several third-party collections and demos but does not modify the production campaign result popup.
  Evidence: Branch diff statistics and path grouping from the repository audit.

- Observation: The confetti prefab and its direct dependencies already exist on `main`; only the racing-flag pattern needs to be selectively recovered from the experimental branch.
  Evidence: Asset-path inspection on `main` and comparison with `origin/feature/result-screen-vfx`.

- Observation: Unity 6000.5.9f1 is installed and the project is open, but the Unity CLI registry is empty and the project does not yet contain the Pipeline package. The running Editor is not in Safe Mode.
  Evidence: `ProjectSettings/ProjectVersion.txt`, Unity Editor log path, `unity editors --installed`, `unity status`, and `unity pipeline list`.

- Observation: There is no existing `.github` workflow and no observed workflow history for `main`. The repository's current validation script always attempts Unity compilation even when tests are skipped.
  Evidence: tracked-file inspection, `gh run list --branch main`, and `.agents/scripts/validate-changes.ps1`.

- Observation: The hygiene milestone passes the pragma gate and ignore-behavior checks, while the assembly-definition audit reports 16 pre-existing missing-reference issues in package and vendored assemblies.
  Evidence: `check-pragma-warning-suppressions.ps1` returned `TOTAL:0`; `git check-ignore -v` matched all four new patterns; `check-scripts-asmdef-references.ps1` reported the same package-level baseline independently of the hygiene files.

## Decision Log

- Decision: Execute the roadmap in focused milestones and commits without rewriting existing history.
  Rationale: The repository contains useful published work and Git LFS objects; additive commits and archival refs are safer and easier to review than history surgery.
  Date: 2026-09-15

- Decision: Start from refreshed `main`, use `codex/` branches, and keep later milestones stackable until the user authorizes publication or merge actions.
  Rationale: This keeps each logical unit reviewable while respecting the explicit gate around push, pull-request creation, and merge operations.
  Date: 2026-09-15

- Decision: Use an archive-then-prune policy for unmerged branches and stashes.
  Rationale: The repository has meaningful unfinished work, including an arcade spline driver and FTUE work. Annotated local archive tags retain exact commit identity before refs or stashes are removed.
  Date: 2026-09-15

- Decision: Import only production-ready result-popup assets and behavior from the experimental VFX branch.
  Rationale: Importing demo scenes, recovery data, root controllers, DOTween changes, or complete third-party packs would create a large, weakly related dependency expansion.
  Date: 2026-09-15

- Decision: Use a low-opacity looping racing-flag pattern plus one confetti burst, with no mask transition.
  Rationale: This delivers the selected celebration while keeping result content legible and the production change small.
  Date: 2026-09-15

- Decision: Configure the UIEffect presentation in the prefab and keep C# responsible only for the `ParticleSystem` lifecycle.
  Rationale: `Game.Campaign.asmdef` does not reference Coffee UIEffect. Prefab-only configuration avoids coupling campaign runtime code to the effect package.
  Date: 2026-09-15

- Decision: Add phased CI: static and analyzer-oriented validation without Unity credentials now, then a licensed Unity compile/test job later.
  Rationale: The immediate gate can protect architecture, pragma, and analyzer rules without pretending that an unlicensed runner proves Unity compilation.
  Date: 2026-09-15

- Decision: Do not add an automated test solely for prefab presentation wiring.
  Rationale: Repository rules classify layout and simple UI wiring as visual/static validation work. The code change is small lifecycle glue and will be covered by compilation, lint, and focused visual scenarios.
  Date: 2026-09-15

- Decision: Require a separate explicit approval before changing external GitHub state.
  Rationale: Closing pull requests, deleting remote branches, and pushing archive tags affect shared collaborators and cannot be inferred from local cleanup authorization.
  Date: 2026-09-15

## Outcomes & Retrospective

To be completed when the roadmap finishes. Record what shipped, which external cleanup actions were approved, what remained intentionally deferred, and the exact verification evidence.

## Context and Orientation

`main` is the integration branch and initially matched `origin/main` at commit `bcda1ccf`. `ProjectSettings/ProjectVersion.txt` declares Unity 6000.5.9f1.

The production result popup is `Assets/GearEngine/Prefabs/Campaign/Campaign_ResultPopupView.prefab`. Its runtime view is `ResultPopupView.cs` in the campaign game module. The view currently binds result statistics and actions but owns no celebration lifecycle. `Docs/Game/Campaign.md` is the module-facing campaign documentation.

Coffee UIEffect is vendored under `Assets/3rdParty/UIEffect`. The production confetti prefab is `Assets/Lana Studio/Hyper Casual FX/Prefabs/Confetti/Confetti_directional_multicolor.prefab`. The experimental branch contains a racing-flag texture at `Assets/3rdParty/UIEffect/UIEffectPresets/Textures/FlagPattern.png`; it must be imported selectively and renamed to comply with this repository's asset naming rules.

The repository validation entry point is `.agents/scripts/validate-changes.ps1`, wrapped by `.agents/scripts/validate-changes.sh` on macOS and Linux. It currently performs assembly-definition audit, pragma gate, Unity compilation, EditMode tests, PlayMode tests, and analyzer validation. `-SkipTests` omits tests but still requires Unity compilation. The new `-StaticOnly` mode will run only gates that do not require a Unity Editor or license.

`Assets/GearEngine/Scripts/Editor/SubmissionBuildExporter.cs` resolves its default WebGL output below `Artifacts/Submission/Build/GearEngineWebGL`. The environment variable `GEAR_ENGINE_BUILD_PATH` is an intentional override and must keep working. The repository already ignores `/Builds/`, so the new default is `Builds/GearEngineWebGL`.

An archive tag is a local Git tag that points to an exact commit and carries an explanatory annotation. It preserves discoverability after a branch ref or stash entry is removed. A remote ref is a shared branch or tag stored on the Git hosting service; changing it requires the explicit external-state approval described below.

## Plan of Work

### Milestone 1: Repository hygiene and roadmap

Restore the whitespace-only project-settings edit. Move the exact approved generated directories out of the repository into a recoverable Trash directory. Add ignore patterns for `__pycache__/`, `.firebase/`, the repository-root `/tmp/`, and `Artifacts/TestResults/**/.media-start` so the same local outputs do not reappear.

Commit this ExecPlan separately from `.gitignore`. Validate the documentation and ignore changes with `git diff --check`, an ignore-behavior probe, and the repository's static checks. No Unity test is warranted because this milestone changes no runtime or Unity asset behavior.

### Milestone 2: Production result-screen celebration

Create `codex/result-screen-celebration` from the completed hygiene branch. Register `/Applications/Unity/Hub/Editor/6000.5.9f1/Unity.app` with the Unity CLI, install the Pipeline package into the project, and wait until the running Editor exposes its live command surface. Use live Unity commands for prefab and asset mutations because a Unity Editor is already open.

Recover only the flag-pattern texture and metadata from `origin/feature/result-screen-vfx`. Rename it to a convention-compliant texture name such as `T_RacingFlagPattern.png` through Unity's `AssetDatabase`, preserving the asset GUID. Do not import demo scenes, recovery folders, root animation controllers, mask-transition assets, DOTween changes, or full vendor packs.

Edit `Campaign_ResultPopupView.prefab` through the running Editor. Add the UIEffect components needed for a looping, unscaled-time pattern on the existing background image. Keep opacity low enough that result labels and buttons remain readable. Instantiate the existing confetti prefab as a child of the popup, preserve it as a prefab instance, configure all particle systems as non-looping and not `playOnAwake`, and assign the root particle system to the view's serialized field.

Update `ResultPopupView.cs` with a serialized `ParticleSystem`. When binding begins, stop and clear any previous state and play the hierarchy once. On unbind and disable, stop and clear the hierarchy. Include the particle reference in existing hierarchy validation. Do not add a dependency on Coffee UIEffect to the campaign C# assembly.

Update `Docs/Game/Campaign.md` to describe the flag-pattern background, one-shot confetti lifecycle, and prefab ownership. Run the Unity C# lint skill in fix and check modes only against the changed C# file, then run its structure verifier. Run the repository validation gate without broad tests unless a real regression or deterministic domain-logic change is discovered. Use live Play Mode and screenshot evidence for the four visual scenarios listed under Validation and Acceptance.

### Milestone 3: Archive and prune local Git state

Create an English maintenance report under `Plans/RepositoryRoadmap/` that records each branch, stash, pull request, tip commit, evidence, and disposition. Before dropping a stash, create an annotated local tag pointing to its stash commit, verify the tag resolves and the diff remains inspectable, then remove the stash entry. Use stable names beneath `archive/2026-09-15/stash-*`.

Delete local branches that are fully merged using Git's safe delete mode. For `feature/design-system`, `feature/new-ui`, and `codex/ftue-blackboard-tutorial`, create and verify annotated archive tags before removing their local branch refs. Record the FTUE branch as a future gap-analysis task rather than treating it as shipped behavior. After the result-screen feature is integrated or otherwise accepted, archive its experiment branch disposition as well.

Prepare, but do not execute, the shared cleanup list for merged remote branches and stale pull requests #3, #5, #9, #10, #12, #13, and #14. Stop and ask for explicit approval before pushing archive tags, closing any pull request, or deleting any remote ref.

### Milestone 4: CI and artifact policy

Add a `-StaticOnly` switch to `.agents/scripts/validate-changes.ps1`. Static-only mode must run the assembly-definition audit, pragma gate, and analyzer validation while clearly marking Unity compilation and EditMode/PlayMode tests as intentionally skipped. Preserve all existing default behavior when the switch is absent, and reject conflicting or meaningless combinations if needed for clarity.

Add a GitHub Actions workflow for pull requests and pushes to `main`. It should check out Git LFS pointers without downloading payload objects and invoke the new static-only gate. Do not claim Unity compile or test coverage in this phase.

Change `SubmissionBuildExporter.ResolveBuildPath()` so the default is `Builds/GearEngineWebGL`. Preserve `GEAR_ENGINE_BUILD_PATH` as the override. Update the appropriate documentation with the artifact policy: `Artifacts/` contains curated evidence or submission material, generated local builds belong under ignored `Builds/`, and CI will gain a separate Unity 6000.5.9f1 compile/test job only after required license credentials and runner setup exist.

Because this milestone changes C#, run focused lint and structure verification for `SubmissionBuildExporter.cs`, then the static-only gate and the repository compile/analyzer gate. Do not add tests for the path replacement unless an existing focused exporter test already provides a practical extension point; the behavior remains directly inspectable and is covered by compilation plus explicit path verification.

## Concrete Steps

Run commands from the repository root unless a command names another working directory.

1. Confirm baseline and create hygiene branch:

       git fetch --prune origin
       git status --short --branch
       git switch -c codex/repository-hygiene

2. Apply the exact hygiene changes, then verify:

       git diff --check
       git status --short --branch

3. Commit the plan and ignore policy independently using English Conventional Commit messages.

4. Create the stackable feature branch:

       git switch -c codex/result-screen-celebration

5. Register and connect the installed Editor:

       unity editors add /Applications/Unity/Hub/Editor/6000.5.9f1/Unity.app
       unity pipeline install --project-path "/Users/leonardosilva/Documents/MatheusCohen/Gear Engine"
       unity status --format json
       unity command --project-path "/Users/leonardosilva/Documents/MatheusCohen/Gear Engine" --format json

6. Use the connected Editor's command catalog and `AssetDatabase` APIs to rename/import the pattern and modify the prefab. Do not hand-edit Unity YAML while the Editor is reachable.

7. Patch `ResultPopupView.cs` and `Docs/Game/Campaign.md`, recompile through the running Editor, then run focused lint and validation.

8. Capture visual evidence outside `Assets/` under an ignored artifact location and inspect the screenshots before accepting the milestone.

9. Commit the feature as `feat(campaign): add result popup celebration` after all checks pass.

10. Produce the maintenance report and local archive tags. Verify each tag before removing a stash or unmerged local branch. Stop at the external-state gate.

11. After the user resolves the external-state gate, implement static CI and build-output policy on a focused branch, validate, and commit.

## Validation and Acceptance

### Repository hygiene

- `git diff --check` exits successfully.
- A temporary ignored-file probe confirms every new pattern is effective without leaving files behind.
- `git status --short` contains only intentional roadmap changes before their commits and is clean afterward.
- The removed generated directories are recoverable from `/Users/leonardosilva/.Trash/GearEngineCleanup-20260915` until the user empties Trash.

### Result-screen celebration

- The changed C# file passes the Unity C# lint script in `fix`, `check`, and structure-verification modes.
- The running Editor recompiles with zero errors and the repository validation gate completes without analyzer errors.
- First open: the pattern is visible at low opacity and one confetti burst starts.
- Settled state: the confetti stops naturally while the pattern continues in unscaled time.
- Reopen: the prior particles are cleared and exactly one new burst starts.
- Readability: result statistics, upgrade action, and continue action remain visually clear; no errors appear in the Editor log or Console.
- The feature diff contains only the selective pattern asset and metadata, the production prefab, `ResultPopupView.cs`, campaign documentation, and any unavoidable Pipeline package manifest/lock update. It contains none of the excluded demo/vendor content.

### Git archive and pruning

- Every removed stash or unmerged local branch has a verified local archive tag and a recorded original ref and tip commit.
- Fully merged local branches are removed only with safe deletion.
- `git fsck` and `git lfs fsck` complete successfully after local pruning.
- No remote tag is pushed, pull request is closed, or remote branch is deleted without the explicit approval checkpoint.

### CI and artifact policy

- The static-only validation mode runs the assembly-definition, pragma, and analyzer stages and does not invoke Unity compilation or Unity tests.
- The GitHub workflow triggers for pull requests and pushes to `main`, checks out LFS pointers without pulling binary payloads, and runs the static-only gate.
- Existing non-static validation behavior remains unchanged when `-StaticOnly` is absent.
- `SubmissionBuildExporter` resolves the default path to `Builds/GearEngineWebGL` and still honors `GEAR_ENGINE_BUILD_PATH`.
- Documentation explicitly distinguishes curated `Artifacts/` content from ignored generated `Builds/` output and records the deferred licensed Unity CI phase.

## Idempotence and Recovery

The hygiene cleanup is recoverable from the dedicated Trash directory. Re-running the ignore checks is safe. Do not empty that Trash directory as part of this roadmap.

The VFX asset import is selective and additive. If prefab editing fails, close the prefab editing scope without saving and repeat through the live Editor. If C# compilation forces Safe Mode, read only compiler-error lines from the narrowest Editor log, fix the source, restart the specific Gear Engine Editor process by PID, and re-establish the Pipeline connection. Never stop all Unity processes by name.

Archive tags must be created and resolved before any source ref is removed. If a local branch was removed accidentally after its tag was verified, recreate it with `git branch <name> <archive-tag>`. If a stash was dropped after its archive tag was verified, inspect or recreate it from the tag's commit. Do not use `git reset --hard` or rewrite public history.

The CI switch is additive. If the workflow fails because of a runner dependency, preserve the local static-only command as the source of truth and adjust only the workflow bootstrap. The exporter path keeps an environment override so existing release automation can continue using an explicit destination.

## Artifacts and Notes

Expected versioned artifacts:

- `Plans/RepositoryRoadmap/RepositoryRoadmap-ExecPlan.md`
- `.gitignore`
- the renamed racing-flag texture and its `.meta` file
- `Assets/GearEngine/Prefabs/Campaign/Campaign_ResultPopupView.prefab`
- the campaign `ResultPopupView.cs`
- `Docs/Game/Campaign.md`
- `Plans/RepositoryRoadmap/GitMaintenanceReport.md`
- `.agents/scripts/validate-changes.ps1`
- a workflow below `.github/workflows/`
- `Assets/GearEngine/Scripts/Editor/SubmissionBuildExporter.cs`
- the appropriate repository documentation for artifact and CI policy

Expected local or generated evidence is not committed unless explicitly selected as a curated baseline. Visual captures belong outside `Assets/`, and generated builds belong under ignored `Builds/`.

## Interfaces and Dependencies

At completion, `ResultPopupView` has one new serialized `ParticleSystem` dependency owned by its production prefab. It does not expose a new public runtime API and does not add Coffee UIEffect as a campaign assembly dependency.

The validation script has one new command-line interface: `-StaticOnly`. Without it, all current semantics remain in force. With it, Unity compilation and Unity Test Framework stages are skipped explicitly while the static repository gates still execute.

`SubmissionBuildExporter` continues to accept `GEAR_ENGINE_BUILD_PATH`. Only its fallback destination changes.

External dependencies remain the vendored UIEffect package, the existing confetti asset, Unity 6000.5.9f1, PowerShell 7, Git, Git LFS, and GitHub Actions. No new runtime package is planned.

---

Revision history:

- 2026-09-15: Initial roadmap created from the repository audit and approved implementation decisions; recorded completion of the recoverable baseline cleanup.
- 2026-09-15: Recorded the two focused hygiene commits and the pre-existing assembly-definition audit baseline.
