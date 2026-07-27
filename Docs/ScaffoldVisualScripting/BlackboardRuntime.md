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
- `Scaffold.VisualScripting.Unity` will own the optional wrapper and callback relays.

Dependencies point from Unity to Authoring to Core. Core never depends on the wrapper.

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

During the pre-cutover milestones, the Gear action bridge retains a legacy execution
overload so the characterized component runner continues compiling. This overload is
not a Core API and is deleted with the legacy component graph during the breaking
cutover.
