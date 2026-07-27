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
