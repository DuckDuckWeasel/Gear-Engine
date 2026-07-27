# Replace the Blackboard component graph with a cloneable plain-C# runtime

This ExecPlan is a living document. Update its Progress, Surprises & Discoveries,
Decision Log, Outcomes & Retrospective, and Artifacts sections as implementation
advances.

## Purpose / Big Picture

Replace the component-owned visual-scripting Blackboard with a reusable runtime that
can be defined, cloned, started, ticked, stopped, and disposed without a `GameObject`.
Unity remains an optional host and callback source, not the owner of execution state.

The target definition hierarchy is:

`BlackboardDefinition -> BlockDefinition -> ActionTrackDefinition -> ActionListDefinition -> IAction`

A template can be stored directly, in a `BlackboardDefinitionAsset`, or in a variable
of an already-running source Blackboard. Starting a Blackboard creates an isolated
runtime clone, comparable to instantiating a prefab. This is a deliberate breaking
replacement; legacy Blackboard, Block, Command, EventHandler, and Variable component
serialization will be removed after all consumers are cut over.

## Progress

- [x] Review the current architecture and record why component ownership violates the
  intended runtime boundary.
- [x] Lock the breaking-replacement policy, template sources, cloning semantics, and
  assembly direction.
- [x] Milestone 1: complete the legacy inventory, behavior-parity matrix, and
  characterization baseline.
- [x] Milestone 2: add definitions, references, validation, stable IDs, and
  cycle-aware graph cloning.
- [x] Milestone 3: add plain variables, variable stores, service contracts,
  messaging, and persistence.
- [x] Milestone 4: add action contexts, action lists, tracks, Blocks, and composite
  execution, then migrate action families.
- [x] Milestone 5: add the plain Blackboard runtime and plain triggers.
- [x] Milestone 6: add the Unity wrapper, callback relays, factories, and VContainer
  composition.
- [x] Milestone 7: rewrite editor authoring for managed definitions.
- [ ] Milestone 8: cut over assets and tests, rename the space-containing test scene,
  and remove legacy components.
- [ ] Milestone 9: complete documentation, the compilation loop, all test suites,
  analyzer checks, the macOS validation gate, and this retrospective.

## Surprises & Discoveries

- The existing `CompositeExecutionRunner`, `CommandTrack`, and `ICompositeTask` are
  already plain C# extraction seams, but their owners are still components.
- `ActionBase` is serializable plain C#, but all actions are automatically injected
  with a `MonoBehaviour`, concrete legacy Blackboard, and concrete Command even when
  they do not require those capabilities.
- `GameStarted` is a component only because it consumes `Start` and a coroutine; its
  actual behavior is a scheduled startup signal.
- The macOS validation script is `.agents/scripts/validate-changes.sh`. The `.cmd`
  shim is a no-op and cannot be used as acceptance evidence.
- This worktree began detached and was moved to
  `codex/blackboard-runtime-refactor` before implementation.
- Several tracked binary dependencies were Git LFS pointer text in the worktree.
  `git lfs pull` restored the assemblies required for Unity compilation.
- The legacy `GameStarted` characterization did not assign its handler to the owning
  Block, and action-list tests compared `ActionWrapper` values directly to actions.
  Both were stale test assumptions, not production behavior failures.
- Unity's generated root solution contains repeated project display names. Scoped
  owning `.csproj` checks are authoritative for changed C# files; solution-wide
  `dotnet format` is not.
- The required macOS gate needs normal access to Unity licensing and local databases.
  Its sandboxed run timed out; its escalated compilation precheck passed.
- The repository-wide baseline is independently red: 59 unrelated EditMode failures,
  a PlayMode run that exits without XML after loading the SRDebugger backup scene,
  five existing asmdef-audit issues, and a Windows-only analyzer launcher that calls
  `cmd.exe`. These are recorded as pre-existing final-gate blockers rather than
  attributed to the Blackboard characterization changes.
- The custom production analyzers require ordinary documentation to live in module
  docs rather than XML comments, properties before serialized fields, single-line
  statements/signatures, methods under 16 lines, and acyclic declaration ordering.
  Cycle-aware graph traversal therefore uses instance callbacks for recursive edges
  while retaining readable, bounded methods.
- The repository's one-top-level-type filename rule cannot represent both an untyped
  base and a generic type with the same filename. The variable hierarchy therefore
  uses `VariableDefinitionBase`/`VariableDefinition<T>` and
  `VariableCellBase`/`VariableCell<T>`, keeping generic public APIs concise while
  making every source filename deterministic.
- Unity imports and executes the final Core tests cleanly, while direct post-change
  `dotnet build` attempts for the generated runtime project and scoped Core project
  remain silent until explicitly terminated. The generated Editor project builds
  cleanly; the runtime-project stall is recorded as tooling evidence rather than a
  compiler failure or pass.
- The action migration needs a temporary compatibility edge until the breaking
  cutover: Core execution is fully context-driven and host-free, while the existing
  component runner still invokes a legacy overload on the Gear action bridge. That
  overload and its concrete component context are quarantined to the legacy path and
  are not part of the target Core API.
- Coroutine-host usage was spread across scheduled, tween, dialog, reflection, and UI
  input actions. Routing it through `IFrameScheduler.ScheduleRoutine` reduced direct
  `StartCoroutine` usage to the temporary legacy bridge and actual engine-facing UI
  components.
- The first Milestone 4 analyzer pass found 112 unique diagnostics in changed legacy
  action files. Decomposing long methods, enforcing declaration call order, removing
  stale XML comments, and normalizing serialized member names brought the complete
  changed-C# formatter/analyzer/structure gate to zero diagnostics.
- Lifecycle signals must be filtered by runtime-instance ID because one injected
  event bus may serve several independent Blackboards. Local messages therefore carry
  an explicit target ID, while broadcasts intentionally leave the target empty.
- The legacy `BlackboardEnabled` and `GameStarted` handlers both defer execution by
  frames. A reusable scheduler-owned deferred callback preserves that timing and
  cancels pending work when a runtime is disabled or disposed.
- VContainer selected `SystemRandomSource(int)` during the first Unity adapter run.
  Registering `IRandomSource` through an explicit factory prevents the optional seed
  from being treated as a dependency.
- Scheduler state must be runtime-owned. VContainer resolves a fresh
  `UnityFrameScheduler` for every Blackboard, while one injected
  `UnityCoroutineRunner` remains the narrow engine callback receiver used by those
  schedulers.
- Unity 6 treats `Object.GetInstanceID` as an obsolete compile error. Authoring-source
  cycle detection uses `EntityId`, matching the current editor object-identity API.
- Managed-reference Undo is reliable when the owning asset or component is recorded
  as a complete object before mutation. Graph operations therefore mutate definitions
  through one Undo-aware controller rather than relying on component add/remove APIs.

## Decision Log

- This is a breaking replacement. No automatic serialized migration and no
  backward-compatible component wrappers will ship.
- Core may use Unity serialization attributes, Unity value types, and explicit
  `UnityEngine.Object` references, but no Core type may derive from `MonoBehaviour`.
- Runtime cloning deeply isolates managed definitions, state, collections, actions,
  triggers, and variables while preserving referenced Unity object identities.
- Definition IDs are stable across runtime clones. Each clone receives a distinct
  runtime-instance ID. Editor duplication regenerates definition IDs.
- Root `BlackboardBehaviour` hosts accept Direct or ScriptableObject templates.
  BlackboardVariable templates require an already-running source Blackboard.
- Definitions stored in variables are templates, never live Blackboard instances.
- Local and public variable cells are owned by one runtime clone. Public cross-runtime
  access requires an explicit runtime-instance address; injected-global cells are the
  only deliberately shared cells and are supplied by an injected store.
- Persistence records runtime-instance and definition IDs. It rejects data captured
  for another runtime instead of falling back to names or scene searches.
- New assemblies are introduced beside the legacy implementation during bounded
  milestones. This preserves a compilable repository while features move. The legacy
  types are deleted during the explicit breaking cutover, not retained as adapters.
- The legacy `IAction.Execute(Action)` overload and Gear component-context injection
  exist only to keep characterized component consumers compiling before Milestone 8.
  New runtime execution exclusively uses Core `IAction.Execute` with
  `ActionExecutionContext`; the compatibility edge will be deleted with
  `InvokeActionCommand` and the legacy component graph.
- All dependency injection uses VContainer and explicit services. New mutable static
  service locators are forbidden.
- Existing execution semantics are retained unless they exist only because of
  component ownership.
- A newly created plain runtime is enabled but not started. `Start` binds its triggers,
  raises enabled and started lifecycle events, and is idempotent. `Disable` detaches
  trigger sources, cancels deferred trigger work, and interrupts executing Blocks.
- Trigger definitions own no subscriptions. Their runtime bindings attach on enable,
  detach on disable, and dispose symmetrically. Bindable sources may return payloads
  into a stable variable reference; polling conditions evaluate through the immutable
  trigger context.
- `BlackboardRuntimeServices` is an owned disposable scope supplied through
  `IBlackboardRuntimeServicesFactory`. A Blackboard disposes that scope and its
  scheduler when its lifetime ends.
- `BlackboardBehaviour` is an optional lifecycle adapter, not a second runtime path.
  It resolves a template and delegates construction to the same `BlackboardFactory`
  used by scripts.
- Layout, tint, grouping, zoom, scroll, and selection are authoring metadata stored on
  the definition asset or wrapper. They are not cloned into the runtime graph and
  cannot affect execution.

## Outcomes & Retrospective

Milestone 1 established a compilable and lint-clean baseline for the affected
Blackboard surface. Focused evidence is 2/2 event-handler tests, 3/3 variable tests,
19/19 block/track tests, and 39/39 action-list tests.

Milestone 2 introduced `Scaffold.VisualScripting.Core` and
`Scaffold.VisualScripting.Authoring` beside the legacy graph. Its pure EditMode fixture
passes 14/14 cases covering all three template sources, managed isolation, preserved
Unity object identity, stable definition IDs, unique runtime IDs, managed cycles,
transient reset, validation failures, and editor ID regeneration. The full
retrospective will record the final runtime boundary, migrated surface, verification
evidence, deliberate behavior changes, and follow-up work.

Milestone 3 introduced typed plain definitions and runtime cells, isolated local and
public stores, explicitly injected globals, stable public addresses, runtime
registries, messaging, text substitution, persistence, and service contracts for
time, scheduling, logging, events, and save/load. Its pure EditMode fixture passes
13/13 cases, and the Milestone 2 definition/cloning regression fixture remains 14/14.

Milestone 4 introduced the immutable `ActionExecutionContext`, status completion,
plain `ActionBase`, `ActionList`, `ActionTrack`, `Block`, flow controller, composite
tasks, execution IDs, transient execution state, and the shared Strategy runner.
Sequence, Selector, Parallel, Parallel Selector, Utility Selector, all await modes,
ordering, weights, repeat prevention, interruption, flow jumps, multi-track
flattening, utility reevaluation, and execution feedback are covered by 16/16 Core
tests. The Gear bridge passes 7/7 tests, including delay and IEnumerator scheduling
and disposal without a `GameObject`; the legacy behavior matrix remains 39/39. All 62
changed C# files pass formatter, analyzer, and one-top-level-type verification.

Milestone 5 introduced `BlackboardFactory`, the clone-owned plain `Blackboard`
lifecycle, block lookup/execution/stop/reset, runtime ticking, variable lookup,
targeted and broadcast messaging, text substitution, save/load/delete, registry
ownership, and deterministic disposal. Plain GameStarted, BlackboardEnabled, message,
polling, and bindable triggers attach through injected contracts and contain no Unity
lifecycle method. Its pure EditMode fixture passes 10/10 cases without a `GameObject`,
`AddComponent`, coroutine, or `[UnityTest]`; the complete replacement-Core regression
namespace passes 53/53 through this milestone.

Milestone 6 introduced `Scaffold.VisualScripting.Unity`, the optional
`BlackboardBehaviour`, VContainer composition, per-runtime `UnityFrameScheduler`
instances, one narrow coroutine callback receiver, Unity time/logging/PlayerPrefs
ports, pointer/physics/render callback relays, and plain UI/input signal adapters.
Wrapper-created and script-created instances use the same factory and retain isolated
runtime IDs, variables, execution state, and schedulers. Initialization failures log
through `Debug.LogError` and disable the wrapper. The final Unity adapter fixture
passes 5/5 PlayMode tests and the complete replacement-Core regression remains 53/53.
All 26 changed C# files pass formatter, analyzer, and one-top-level-type verification;
the Core forbidden-API scan and Unity error parser are empty.

Milestone 7 introduced `Scaffold.VisualScripting.Editor` and an Undo-aware managed
authoring controller. The new Blackboard window and inspectors edit Direct,
ScriptableObject, and nested variable-provided definitions without creating Block,
action, trigger, track, action-list, or variable components. The editor supports add,
remove, reorder, copy/paste, duplication with regenerated IDs, action grouping,
selection, search, validation, automatic layout, tint, source navigation, raw
serialized detail editing, and play-mode execution feedback. Its focused EditMode
fixture passes 7/7 cases, and all 19 changed C# files pass formatting, analyzers, and
one-top-level-type verification.

## Context and Orientation

The legacy runtime is under
`Assets/3rdParty/ScaffoldVisualScripting/Scripts`. `Blackboard`,
`Block`, `Command`, `EventHandler`, and `Variable` are component types in
`Scripts/Components`. Component-based triggers are under `Scripts/EventHandlers`.
The current plain action API and most action implementations are under
`Assets/GearEngine/Scripts/Game/GearEngine/Core/Actions`. The component-hosted action
list is `InvokeActionCommand` under
`Assets/GearEngine/Scripts/Game/GearEngine/Presentation/UI/Tags/Input`.

The complete baseline inventory is maintained in
`Plans/BlackboardRuntimeRefactor/LegacyInventory.md`. The architectural findings and
target boundary are maintained in
`Docs/ScaffoldVisualScripting/BlackboardArchitectureReview.md`.

Definitions are reusable serialized data. Runtime objects are mutable execution
instances produced from definitions. A scheduler owns delayed and per-frame work. A
trigger converts a signal into block execution. A relay is the smallest possible
`MonoBehaviour` that receives a callback Unity cannot deliver to a plain object.

## Plan of Work

### Milestone 1: planning baseline and characterization

Materialize this ExecPlan, inventory actions, variable types, handlers, editor
operations, serialized references, and Unity callback dependencies, and add a
behavior-parity matrix. Add characterization tests for composite execution, flow
control, interruption, variables, messages, and startup timing. Commit only after the
focused baseline passes.

### Milestone 2: definitions, references, and cloning

Create `Scaffold.VisualScripting.Core` and
`Scaffold.VisualScripting.Authoring`. Implement definition types, stable definition
IDs, runtime-instance IDs, Direct/ScriptableObject/BlackboardVariable references,
validation, and a cycle-aware serialized graph cloner. The cloner must preserve
`UnityEngine.Object` identity, preserve definition IDs, clone cycles safely, and clear
delegates and explicitly transient fields. Add pure NUnit tests for all reference
sources, isolation, identity, ID, invalid graph, and cycle behavior.

### Milestone 3: plain variables and services

Replace component variables with typed serialized definitions and runtime cells.
Implement local, public, and injected-global stores plus stable variable references.
Introduce interfaces for time, scheduling, events, logging, save/load, registry,
substitution, and messaging. Persistence addresses runtime-instance IDs and definition
IDs rather than GameObjects or names.

### Milestone 4: actions, action lists, Blocks, and composites

Replace the action contract with immutable `ActionExecutionContext` plus a completion
callback carrying execution status. Make `ActionBase` context-scoped and remove stored
component hosts. Extract plain `ActionList`, `ActionTrack`, `Block`, and their transient
state. Reuse the existing composite strategy engine while removing component
assumptions. Migrate actions in three bounded batches: pure/variable/flow,
Unity-reference, then scheduled/tween/dialog actions.

### Milestone 5: Blackboard runtime and triggers

Implement plain Blackboard lifecycle, lookup, execution, ticking, interruption,
reset, substitution, messages, save/load, and disposal. Convert lifecycle and message
triggers to plain definitions. Convert UI subscriptions and polled input into bindable
plain triggers. `GameStarted` is raised by `Blackboard.Start` through the scheduler.

### Milestone 6: Unity wrapper and composition

Create `Scaffold.VisualScripting.Unity`, `BlackboardBehaviour`, required callback
relays, runtime factories, and VContainer registrations. Wrapper-created and
script-created Blackboards use the same factory and services. Initialization failures
are caught, logged with `Debug.LogError`, and leave the wrapper disabled.

### Milestone 7: editor rewrite

Make the Blackboard Window and inspectors edit managed definitions. Preserve graph
creation, removal, reorder, grouping, tracks, copy/paste, undo/redo, search,
validation, selection, layout, tint, source switching, asset navigation, and
play-mode feedback. Keep authoring metadata separate from runtime state. Runtime
cloning preserves IDs; editor duplication regenerates them.

### Milestone 8: breaking cutover

Rebuild the Blackboard prefab and affected scenes around `BlackboardBehaviour`.
Rename `Test Tutorial Scene.unity` to `TestTutorialScene.unity` and update all
references. Update builders and tests to construct definitions. Delete obsolete
component implementations after all consumers compile. Scan every serialized asset
for removed script GUIDs and fail if any remain.

### Milestone 9: final verification and documentation

Update module documentation, `Architecture.md`, this ExecPlan, and its retrospective.
Run lint fix/check/structure for every changed C# batch, the full compilation-error
loop, affected EditMode and PlayMode tests with NUnit XML/log/Report artifacts,
analyzers, and `.agents/scripts/validate-changes.sh`. The final repository must have no
compiler errors, analyzer diagnostics, test failures, removed script GUIDs, or
uncommitted generated changes.

## Concrete Steps

1. Keep each milestone independently compiling and commit it only after its gate is
   clean.
2. Build the new assemblies beside the legacy graph, then redirect consumers in
   bounded batches.
3. Add pure runtime tests before deleting the corresponding component dependency.
4. Update this document after every meaningful discovery, design adjustment,
   verification run, and milestone commit.
5. Run the repository efficiency workflow after changed-scope updates so verification
   remains proportional until the final full gate.

## Validation and Acceptance

- Pure NUnit tests create, clone, start, tick, and execute a Blackboard without a
  `GameObject`, `AddComponent`, coroutine, or `[UnityTest]`.
- Clones from Direct, ScriptableObject, and parent-variable templates never share
  mutable managed state.
- Unity object references remain intentionally shared; definition IDs remain stable;
  runtime-instance IDs differ.
- Missing templates, null actions, duplicate IDs, unresolved variable sources, and
  reference cycles fail deterministically with actionable errors.
- Script-created and wrapper-created instances pass the same execution matrix.
- Sequence, Selector, Parallel, Parallel Selector, Utility Selector, Await, ordering,
  weights, repeat prevention, interruption, flow jumps, multi-track behavior,
  scheduling, execution feedback, stop, and reset retain their current semantics.
- `GameStarted` works with a fake frame scheduler and no Unity lifecycle method.
- Unity relays attach and detach listeners symmetrically.
- The editor authors the complete graph without adding Block, Command, ActionList,
  trigger, or variable components.
- Undo/redo, copy/paste, reorder, grouping, source switching, asset editing,
  validation, and play-mode feedback remain functional.
- No Core type derives from `MonoBehaviour`; no Core execution path uses
  `GetComponent`, `AddComponent`, `StartCoroutine`, `GameObject.Find`, or a mutable
  Singleton.
- Rebuilt serialized assets contain no missing scripts or removed legacy GUIDs.
- Every changed C# batch passes lint fix/check/structure.
- Every Unity test run produces NUnit XML, an Editor-log summary, and a contextual
  `Report.md`.
- The final macOS gate passes with zero compiler errors, analyzer diagnostics, test
  failures, or uncommitted generated changes.

## Idempotence and Recovery

New runtime work is introduced in separate folders and assemblies until the cutover,
so a failed intermediate migration can be repaired without resurrecting deleted
serialization. Definition validation occurs before runtime creation. Clone operations
do not mutate source templates. Subscription and scheduled-work handles are disposed
symmetrically. Every destructive deletion is deferred until replacement consumers
compile and asset references are known.

If a milestone gate fails, retain the milestone as uncommitted work, update
Surprises & Discoveries with the failure, repair the smallest owning scope, and rerun
the same gate. Never skip forward to destructive cutover while an earlier milestone is
red.

## Artifacts and Notes

- Current architecture review:
  `Docs/ScaffoldVisualScripting/BlackboardArchitectureReview.md`
- Baseline inventory and parity matrix:
  `Plans/BlackboardRuntimeRefactor/LegacyInventory.md`
- Unity test artifacts will live under
  `Logs/Tests/BlackboardRuntimeRefactor/<Milestone>/<Platform>/`.
- Compilation audit:
  `Docs/UnityErrorCheckReport.md`
- Focused Milestone 1 results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone1/`
- Milestone 2 Core definition and cloning results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone2/CoreDefinitions/`
- Milestone 3 variable and service results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone3/VariablesAndServices/`
- Milestone 3 definition regression results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone3/DefinitionRegression/`
- Milestone 4 plain action/composite runtime results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone4/ActionRuntime/`
- Milestone 4 Gear context and scheduler bridge results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone4/GearActionBridge/`
- Milestone 4 legacy behavior-parity results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone4/LegacyInvokeActionRegression/`
- Milestone 6 Unity adapter results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone6/UnityAdapterFinal/`
- Milestone 6 replacement-Core regression results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone6/CoreRegression/`
- Milestone 7 managed authoring results:
  `Logs/Tests/BlackboardRuntimeRefactor/Milestone7/AuthoringFinal/`
- Full repository gate baseline on 2026-07-27:
  compilation precheck passed; EditMode reported 248 passed and 59 failed; PlayMode
  exited without results; the asmdef and analyzer infrastructure blockers listed in
  Surprises & Discoveries remain outside the affected milestone scope.

## Interfaces and Dependencies

The target public dependencies point in one direction:

`Scaffold.VisualScripting.Core <- Scaffold.VisualScripting.Authoring <- Scaffold.VisualScripting.Unity`

Editor assemblies depend on the model assemblies they edit. Game actions depend on
Core, not the Unity wrapper. Composition roots depend on VContainer and register
runtime factories, schedulers, time sources, variable stores, event buses, save
services, loggers, and relay factories. Core exposes contracts for these capabilities
and never resolves mutable global state.

The primary public model and runtime types are `BlackboardDefinition`,
`BlockDefinition`, `ActionTrackDefinition`, `ActionListDefinition`, `IAction`,
`ActionExecutionContext`, `Blackboard`, `Block`, `BlackboardDefinitionReference`,
`BlackboardDefinitionVariable`, `BlackboardDefinitionAsset`,
`IBlackboardFactory`, `IFrameScheduler`, `ITimeSource`, `IBlackboardEventBus`,
`IBlackboardSaveService`, `IBlackboardLogger`, and variable-store contracts.
