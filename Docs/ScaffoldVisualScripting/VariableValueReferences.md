# Scaffold action value references

Every Scaffold `*Data` action input uses the same source model:

1. **Blackboard Variable** selects a compatible serializable variable value from the
   managed definition graph. The selected value is cloned with its owning Blackboard;
   it is not a `MonoBehaviour` or a live Blackboard runtime reference.
2. **Direct** stores the value or Unity object directly on the action.
3. **ScriptableObject** selects a typed value asset such as `AnimatorValueSO` or `FloatValueSO`.

`VariableValueReference` is the shared resolver for this compatibility model. New
Core-facing actions should prefer stable `VariableReference` values resolved by
`BlackboardVariableSet`, especially for public or injected-global access.

Each ScriptableObject value class is also in its own file under `Scripts/VariableTypes/`. A field only accepts its matching asset type, so `AnimatorData` accepts `AnimatorValueSO` and cannot accept `FloatValueSO`.

The serialized `Unspecified` source preserves the value-selection behavior: use the
managed Blackboard variable when one exists; otherwise use the direct value. The
component-owned graph itself is intentionally not migration-compatible.
