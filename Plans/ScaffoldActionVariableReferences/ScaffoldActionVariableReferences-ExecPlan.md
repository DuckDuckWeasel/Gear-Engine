# Unify Scaffold action value references

This ExecPlan is a living document.

## Purpose / Big Picture

Scaffold actions currently use a mixture of `*Data` fields and raw Unity object fields. A `*Data` field supports either a typed Blackboard variable or a direct value. This work makes every action input use the same three-source value model: a typed Blackboard variable, a direct value/reference, or a compatible ScriptableObject value asset. Designers will select the source in the Inspector, and actions will resolve the selected value consistently at runtime.

## Progress

- [x] Read the repository architecture, action workflow, and Tutorial reference implementation.
- [x] Inventory Scaffold actions and current value-field patterns.
- [x] Add the reusable three-source reference contract and editor drawer to Scaffold Visual Scripting.
- [x] Extend all built-in `*Data` value types with compatible ScriptableObject source support.
- [x] Review raw action fields; retain command settings, runtime state, and output-only variable targets outside the value-source contract.
- [x] Add focused EditMode regression tests for all three sources and type compatibility.
- [ ] Run the repository validation gate and resolve diagnostics.

## Surprises & Discoveries

- The Tutorial package provides the intended ScriptableObject-reference pattern (`TutorialVariableReference<TValue, TAsset>`), but it does not include a typed Blackboard-variable option.
- Existing Scaffold `*Data` structures already provide the Blackboard-variable and direct-value paths. Extending them is the lowest-risk way to update the majority of actions without altering their execution logic.
- The original `VariableDataDrawer<T>` accepted typed scene references correctly. The unified drawer keeps that behavior rather than relying on `BlackboardWindow`, which would make non-Blackboard inspectors and scene/global variable references unavailable.
- This repository has unrelated local changes. Only files necessary for this feature will be modified.

## Decision Log

- Use the Tutorial package's ScriptableObject-value pattern as the source-model reference, combined with the existing typed Blackboard `VariableBase<T>` model.
- Keep current `*Ref` and `*Val` serialized fields so existing scenes/prefabs retain their values. Add source selection and compatible ScriptableObject fields alongside them.
- Retain output-only action fields as writable Blackboard variables; they are output sinks rather than value inputs.

## Outcomes & Retrospective

- Added the three-source contract to every built-in typed `*Data` input and verified the affected runtime, editor, and EditMode test assemblies compile without errors.
- The complete validation gate could not run in this environment because PowerShell 7 (`pwsh`) is not installed. The project remains open in Unity, so no second Unity process was started for EditMode execution.

## Context and Orientation

`Assets/3rdParty/ScaffoldVisualScripting/Scripts/VariableTypes/` contains `VariableBase<T>` types and `*Data` serialized structs. `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/VariableTypes/VariableDataDrawer.cs` renders the shared source selector. Scaffold actions are in `Assets/GearEngine/Scripts/Game/GearEngine/Core/Actions/ScaffoldActions/`.

The model must resolve an input from exactly one source:

1. **Blackboard variable**: a typed `VariableBase<T>` reference, including global-scoped variables and compatible scene MonoBehaviours.
2. **Direct value**: a serializable primitive, struct, or Unity object assigned directly on the action.
3. **ScriptableObject value**: an asset that stores a value of `T`, accepted only when its payload type matches the action field's type.

## Plan of Work

Create shared generic ScriptableObject value containers and source-selection metadata in Scaffold Visual Scripting. Update the generic property drawer to show the three choices and only allow a compatible asset in ScriptableObject mode. Add type-specific value-asset subclasses for every existing `*Data` family used by action inputs.

Then migrate all legacy raw action input references to existing or newly added typed `*Data` fields. Keep behavior and summary strings unchanged except for resolving through `.Value`. Do not migrate fields that are output targets: those require mutable Blackboard variables.

## Concrete Steps

1. Add runtime source and value-asset base types under Scaffold Visual Scripting variable types.
2. Extend built-in `*Data` structures with source metadata and type-matched ScriptableObject assets.
3. Move the generic `VariableDataDrawer<T>` into its own file and give it three explicit sources without using the active Blackboard window.
4. Audit raw fields and keep non-value configuration, runtime state, and output target variables outside the input value model.
5. Add tests that prove each source resolves and accepts writes using the selected source.
6. Run targeted tests and `.agents/scripts/validate-changes.sh -SkipTests`; resolve all reported warnings/errors.

## Validation and Acceptance

- A representative action such as `CrossfadeAnim` can select an Animator from a Blackboard variable, directly from the scene, or from an `Animator` value ScriptableObject.
- Every action input no longer exposes a raw Unity reference where a typed variable model is required.
- Existing `*Data` references and direct values remain serialized and evaluate identically.
- Regression tests cover Blackboard, direct, and ScriptableObject sources.
- The repository quality gate passes.

## Idempotence and Recovery

The migration is additive for existing `*Data` serialized members. If a converted action needs to be reverted, retain its `FormerlySerializedAs` attribute and restore only the action field type; existing saved direct values remain in their original serialized field when names are preserved.

## Artifacts and Notes

- Implementation source: `Assets/3rdParty/ScaffoldVisualScripting/Scripts/VariableTypes/` and `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/VariableTypes/VariableDataDrawer.cs`.
- Migrated actions: `Assets/GearEngine/Scripts/Game/GearEngine/Core/Actions/ScaffoldActions/`.
- Tests: `Assets/3rdParty/ScaffoldVisualScripting/Tests/EditMode/`.

## Interfaces and Dependencies

- `VariableBase<T>` supplies typed Blackboard values.
- A new value-asset base supplies typed ScriptableObject values.
- Existing `*Data.Value` consumers remain the stable runtime interface for actions.
