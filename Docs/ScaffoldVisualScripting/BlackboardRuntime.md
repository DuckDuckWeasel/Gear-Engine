# Blackboard runtime

## Purpose

The Blackboard runtime separates reusable authoring data from mutable execution state.
It can be instantiated by code without a `GameObject`; Unity hosting is optional.

## Assembly boundaries

- `Scaffold.VisualScripting.Core` owns definition IDs, runtime-instance IDs, the
  definition hierarchy, validation, graph cloning, and plain runtime contracts. It may
  serialize Unity value types and explicit `UnityEngine.Object` references, but it
  contains no `MonoBehaviour`.
- `Scaffold.VisualScripting.Authoring` owns `BlackboardDefinitionAsset` and the
  Direct, ScriptableObject, and BlackboardVariable template reference.
- `Scaffold.VisualScripting.Unity` owns the optional wrapper, Unity service adapters,
  VContainer registrations, and callback relays.
- `Scaffold.VisualScripting.Editor` owns managed-graph inspectors, the Blackboard
  window, authoring operations, and execution feedback.

Dependencies point from Editor/Unity to Authoring to Core. Core never depends on the
wrapper or editor.

## Definition ownership

The reusable graph is:

`BlackboardDefinition -> BlockDefinition -> ActionTrackDefinition -> ActionListDefinition -> IAction`

Definitions carry stable IDs. Runtime cloning preserves these IDs so internal
references keep working, while every clone receives a new Blackboard runtime-instance
ID. Editor duplication uses `DefinitionIdRegenerator` to create a distinct authoring
identity.

`BlackboardDefinitionVariable` stores another definition template. It never stores a
live runtime. Variable-backed references require an already-running source that
implements `IBlackboardDefinitionVariableSource`.

## Clone semantics

`SerializedGraphCloner` deep-clones managed objects, actions, definitions, arrays,
lists, dictionaries, and cycles. It intentionally retains the identity of
`UnityEngine.Object` references. Delegates, `[NonSerialized]` fields, and
`[BlackboardTransient]` fields reset to their defaults.

`BlackboardDefinitionValidator` rejects missing ownership nodes, null actions, missing
or duplicate definition IDs, missing definition-valued variables, and nested template
cycles with graph paths in each diagnostic.

## Variable ownership

Variables are serialized `VariableDefinition<T>` values and become mutable
`VariableCell<T>` objects only when a runtime clone is created. Built-in definitions
cover Boolean, integer, float, string, Unity vectors, quaternion, color, matrix,
explicit Unity object references, nested Blackboard definitions, and managed
collections. Projects can add another typed definition without adding a component.

Each runtime owns independent local and public cells. A public reference may explicitly
address another running Blackboard by runtime-instance ID and stable definition ID.
Injected-global cells are the only intentionally shared variable state and are supplied
through `IGlobalVariableStore`; they never create hidden `GameObject` instances.

`BlackboardVariableSet` registers public cells, resolves stable references, exposes
definition-valued templates, resets owned cells, and unregisters public cells when
disposed. Save records address the runtime instance and definition IDs rather than
scene names. Event bus, Blackboard registry, time, frame scheduling, save service,
logging, value serialization, and text substitution are explicit injected contracts.

## Action execution

`IAction.Execute` receives an immutable `ActionExecutionContext` and a completion
callback carrying `ActionExecutionStatus`. The context exposes only the current plain
Blackboard, Block, track, action list, flow controller, scheduler, time source, event
bus, save service, logger, runtime ID, and stable execution IDs. Core actions never
receive a `MonoBehaviour` or concrete legacy Command.

`ActionList` and `Block` are plain runtime composites. They use the shared
`CompositeExecutionRunner` Strategy engine for Sequence, Selector, Parallel, Parallel
Selector, Utility Selector, WaitAll, WaitAny, WaitNone, ordered/random/shuffled
selection, weights, repeat prevention, interruption, utility reevaluation, and
execution feedback. `BlockFlowController` maps action jumps and stop requests to the
owning plain Block.

Scheduled actions use `IFrameScheduler.Schedule`, `ScheduleNextFrame`, or
`ScheduleRoutine`. Returned handles are owned by the executing action and disposed
when it completes or is interrupted. Unity coroutine hosts are not part of the Core
execution path.

## Runtime lifecycle

`BlackboardFactory` validates and clones a template, builds isolated variable cells,
constructs plain Blocks and trigger bindings, and registers the runtime by its unique
instance ID. Script-created and wrapper-created instances use this same
factory. No runtime construction step requires a `GameObject`.

`Blackboard` exposes `Start`, `Enable`, `Disable`, `Tick`, block execution and stop
operations, `StopAll`, `Reset`, variable lookup, messaging, substitution,
save/load/delete, and `Dispose`. A runtime starts only once. Disable detaches triggers,
cancels scheduled trigger callbacks, and interrupts active Blocks. Dispose additionally
removes registry and public-variable registrations and disposes all owned execution
state.

The injected frame scheduler advances before trigger polling and Block reevaluation.
This gives delayed callbacks one deterministic execution point and keeps Utility
Selector reevaluation in the same runtime tick.

## Trigger model

`TriggerDefinition` is reusable authoring data. It creates one plain
`ITriggerBinding` per runtime clone. Bindings attach on enable, detach on disable, and
dispose symmetrically.

Built-in Core trigger definitions cover:

- GameStarted and BlackboardEnabled signals with scheduler-owned frame deferral.
- Targeted or broadcast Blackboard messages.
- Polled conditions, including rising-edge and while-true behavior.
- Bindable signal sources for UI or other observer-style publishers, with an optional
  payload destination addressed by a stable variable reference.

The signal-source and polling-condition contracts contain no Unity API. A concrete
adapter may retain an explicit Unity object reference, but the subscription and
decision logic remain plain C#. Physics, render, and pointer callbacks are forwarded
by the Unity assembly.

## Unity hosting and composition

`BlackboardBehaviour` is the optional scene wrapper. `Awake` resolves its Direct,
ScriptableObject, or already-running BlackboardVariable source and delegates creation
to `BlackboardFactory`. `OnEnable`, `Start`, `Update`, `OnDisable`, and `OnDestroy`
only forward lifecycle into the plain runtime. Initialization exceptions are reported
with `Debug.LogError`, clear the partial runtime, and disable the wrapper.

`BlackboardRuntimeInstaller` registers cloning, validation, event, registry, variable,
time, persistence, logging, random, scheduler, and factory services with VContainer.
`IBlackboardRuntimeServicesFactory` creates one owned service scope per runtime.
Consequently, two Blackboards produced by one container share only deliberately
singleton services such as registries and the global store; each owns a distinct
`UnityFrameScheduler` and disposes it with the runtime.

`UnityFrameScheduler` is plain C#. It owns next-frame and delayed callbacks and
delegates only IEnumerator execution to one injected `UnityCoroutineRunner`. The
runner is a `MonoBehaviour` because Unity must host coroutines; no Core type depends on
it.

## Unity callback adapters

Physics, pointer, render, and generic callback relays are intentionally thin
`MonoBehaviour` receivers. They forward stable local Blackboard messages and hold no
execution graph or trigger state.

Button, input-field, and toggle signal sources are plain serializable adapters that
retain explicit Unity UI references. They return disposable listener registrations,
so attach and detach are symmetric. `UnityKeyTriggerCondition` is also plain and
allows input polling through the Core polling-trigger contract.

## Managed editor authoring

`Scaffold.VisualScripting.Editor` edits the managed definition graph directly. Its
Undo-aware controller owns block, track, action, trigger, and variable creation,
removal, reorder, duplication, copy/paste, grouping, layout, tint, and selection
operations. These operations never add a graph node as a component.

Open the editor from `Window > Scaffold > Blackboard`, from a
`BlackboardBehaviour` Inspector, or from a `BlackboardDefinitionAsset` Inspector.
The editor restores the original graph-first workflow while keeping the managed
definition model:

- the legacy grid, node textures, color language, and graph navigation;
- pan, pointer-centered zoom, frame, search, click/additive/rectangle selection,
  and multi-node movement;
- standard Copy, Cut, Paste, Duplicate, Delete, SoftDelete, context-menu, and
  Undo/Redo commands;
- a focused Block inspector for execution settings, triggers, tracks, actions,
  grouping, serialized values, variables, and reference pickers;
- searchable categorized action, trigger, and variable type selection;
- relationship lines for managed actions that expose Block destinations through
  `IBlockConnectionSource`; and
- Direct, asset-backed, and nested definition navigation with a Back control.

The visual layer is split into the window shell, graph canvas, detail panel,
serialized-property renderer, type picker, connection resolver, display helpers,
and execution controller. `BlackboardAuthoringController` remains the single
Undo-aware mutation boundary. This separation allows the editor to look and behave
like the previous version without reintroducing component-owned graph state.

The Blackboard window resolves Direct definitions, definition assets, and nested
templates stored in variables. Variable navigation follows the serialized
`SourceBehaviour` chain and detects wrapper-reference cycles with Unity `EntityId`
values. Inspectors expose explicit source switching and open the same window for
assets and wrappers.

`BlackboardAuthoringMetadata` stores graph layout, tint, grouping, zoom, scroll, and
selection beside the owning asset or wrapper. Runtime cloning starts at
`BlackboardDefinition`, so editor metadata never becomes execution state. Editor
block/action duplication deep-clones managed content, preserves explicit Unity object
references, and regenerates definition IDs. Runtime cloning continues to preserve
those IDs.

During Play Mode, the window reads status from the wrapper's plain runtime by stable
definition ID. A live `BlackboardBehaviour` can execute the selected Block, execute
from a selected action, stop the selected Block, or stop all Blocks. The controls are
disabled with an explanation when there is no compatible live runtime. Feedback and
controls operate on runtime instances only: they do not write transient execution
state into definitions, authoring metadata, assets, or Undo history.

Retained legacy `*Data` wrappers keep their serialized shape, but the editor presents
them through one compact Direct Value, Blackboard Variable, or Scriptable Object
selector. Blackboard Variable mode filters definitions by the wrapper value type and
stores the stable definition ID in the compatibility reference. Older key-only
references are upgraded when the editor can resolve an unambiguous compatible
definition.

Before a retained Gear action executes, its serialized compatibility references bind
once to the owning `BlackboardVariableSet`. Reads and writes then use the managed
runtime cell rather than an isolated inline value. Direct `VariableProperty` fields
and `AnyVariableAndDataPair` use the same typed picker contract. Unity object
definitions are accepted only when their configured initial object is assignable to
the requested compatibility type.

## Breaking cutover

The component-owned graph is no longer supported. Blackboard, Block, Command,
EventHandler, Action Invoker, save-manager, hidden-global, and editor node components
were removed together with their serialized GUIDs. Existing serialized component
graphs require deliberate reconstruction as managed definitions; no migration shim is
included.

The rebuilt `Blackboard.prefab` contains only `BlackboardBehaviour` as the optional
Unity host. `TestTutorialScene.unity`, the execution-matrix scene, scene builders, Gear
actions, and tutorial integration all use the same managed definition and runtime
APIs. Compatibility variable value classes that remain under the original namespace
are serializable plain C# values and do not derive from `MonoBehaviour`.

## Verification

The Core test namespace creates, clones, starts, ticks, executes, stops, and disposes
Blackboards without `GameObject`, `AddComponent`, coroutine hosts, or `[UnityTest]`.
Static checks reject engine-owned execution in Core and scan scenes, prefabs, and
assets for removed component GUIDs.

The repository gate runs the asmdef reference audit, pragma policy, Unity compilation,
and the four Visual Scripting assembly analyzer builds on macOS. Focused EditMode and
PlayMode runs write NUnit XML, Editor logs, and contextual reports under
`Logs/Tests/BlackboardRuntimeRefactor/`.
