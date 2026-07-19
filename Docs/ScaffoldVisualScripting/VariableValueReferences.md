# Scaffold action value references

Every Scaffold `*Data` action input uses the same source model:

1. **Blackboard Variable** selects a variable of the exact compatible Scaffold type. It is a typed Unity object reference, so it supports global-scoped variables and compatible variable MonoBehaviours from the scene; it does not depend on an open Blackboard window.
2. **Direct** stores the value or Unity object directly on the action.
3. **ScriptableObject** selects a typed value asset such as `AnimatorValueSO` or `FloatValueSO`.

`VariableDataDrawer<T>` is the single drawer implementation for this model. Each concrete drawer only binds one `*Data` type to that generic drawer and remains in its own file under `Scripts/Editor/VariableTypes/`.

Each ScriptableObject value class is also in its own file under `Scripts/VariableTypes/`. A field only accepts its matching asset type, so `AnimatorData` accepts `AnimatorValueSO` and cannot accept `FloatValueSO`.

Existing scenes and prefabs remain compatible. The serialized `Unspecified` source preserves the original behavior: use the Blackboard Variable when one exists; otherwise use the direct value.
