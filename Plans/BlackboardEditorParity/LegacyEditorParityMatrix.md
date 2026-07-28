# Legacy Blackboard Editor Parity Matrix

Status: **Owner dispositions approved; implementation verification in progress**
Historical baseline: `4fe499ed`
Managed-editor baseline: `a27a5fa5` on `codex/blackboard-runtime-refactor`
Inventory date: 2026-07-27

## Owner Decision

On 2026-07-27, the owner approved all required Blackboard milestones with practical
workflow parity as the target: retain the old graph-editor look and interaction model,
keep the managed-definition/runtime improvements, retire E39-E41 from this task, and
leave pre-cutover component-graph migration as separate work.

Current implementation restores or replaces E01-E38 on the managed model. E39-E41
are intentionally retired from this task under that approval.

## Purpose

This document turns the legacy Blackboard editor into an explicit user-workflow
contract for the managed-definition editor. It is the Milestone 0 gate for
`BlackboardEditorParity-ExecPlan.md`.

The comparison found 55 deleted production editor C# files and five deleted editor
test files across the Visual Scripting and Gear editor surfaces. The replacement has
13 production editor C# files and one focused test file with seven authoring tests.
File count and line count are not acceptance measures; they show why controller-level
coverage alone cannot establish editor parity.

## Classification Rules

- **Present**: the current managed editor exposes the designer outcome directly.
- **Partial**: part of the outcome exists, but a normal designer must use raw
  serialized data or cannot complete the legacy workflow.
- **Missing**: the managed editor has no equivalent interaction.
- **Restore**: implement the workflow on managed definitions or authoring metadata.
- **Replace**: implement the same designer outcome through a deliberately different
  managed interaction.
- **Retire proposal**: the historical tool depends on a removed product concept or
  is outside Blackboard authoring. Retirement requires owner approval; it is not a
  completed disposition yet.

No row marked Restore or Replace may be downgraded without an explicit Decision Log
entry in the ExecPlan.

## Executive Findings

1. The managed controller preserves the architectural core of authoring: definition
   CRUD, stable IDs, cloning, Undo, target resolution, validation, grouping metadata,
   and basic execution feedback.
2. The designer-facing workflow is not equivalent. The current window is a vertical
   list and raw serialized-object editor rather than a graph canvas plus focused
   Block/action/variable inspectors.
3. `BlackboardAuthoringMetadata` stores only one selected Block ID. Legacy additive
   and rectangle multi-selection therefore cannot be restored without expanding
   authoring-only selection metadata.
4. `BlockDefinition` no longer contains the legacy author description. If description
   remains an editor concern, it belongs in `BlockAuthoringMetadata`, not Core runtime
   definitions.
5. `ActionDefinition` retains Enabled, Utility, Weight, HasWeightOverride, and
   BlockDuringExecution. The current editor exposes these only through the raw owner
   tree, so practical action authoring parity is missing even though the data survived.
6. The controller can reorder an action only inside its existing track. Legacy nested
   action drag/drop, extraction, merge, and cross-track moves require a managed
   cross-track move operation with atomic group/selection metadata updates.
7. Twenty-seven value/reference drawers were deleted while the corresponding data
   wrappers remain in production actions. The result is a project-wide action
   authoring regression, not merely a retired Variable-component UI.
8. `ActionBase.GetConnectedBlocks(ref List<BlockDefinition>)` remains as a managed
   extension seam, but migrated actions such as Call, Menu, and MenuTimer no longer
   supply the legacy connection information. Graph connection rendering therefore
   needs a managed authoring-time resolution contract and restored action coverage.
9. `PerformInterruption` still serializes target action IDs, but its custom selector
   drawer was deleted. Raw string-list editing is not an acceptable replacement.
10. Current tests prove seven managed controller behaviors. They do not characterize
    canvas input, keyboard/context commands, detail layout, type-menu usability,
    variable/reference drawers, drag/drop, connection rendering, or Play Mode control.

## Workflow Parity Matrix

| ID | Designer workflow | Legacy evidence | Current evidence | Status | Required disposition | Acceptance evidence |
| --- | --- | --- | --- | --- | --- | --- |
| E01 | Create a Blackboard host from a menu and select/open it. | `BlackboardMenuItems.CreateBlackboard`, `BlackboardEditor`. | `BlackboardAuthoringMenuItems.CreateBlackboardBehaviour`, `BlackboardBehaviourInspector`. | Present | Keep managed implementation; add a compatible menu alias only if the old path remains part of the approved workflow. | EditMode entry-point test and screenshot. |
| E02 | Create and duplicate reusable Blackboard definition assets. | No equivalent asset model; prefab/component duplication was used. | Asset menu, asset Inspector, and `BlackboardDefinitionDuplicationUtility`. | Present, improved | Keep. | Existing ID-regeneration test plus asset duplication test. |
| E03 | Edit Direct, asset-backed, and nested variable-backed sources. | Component source/reference drawers. | `BlackboardAuthoringTargetResolver` and reference drawer. | Present | Keep; add understandable breadcrumb/back navigation. | Existing resolver test plus navigation interaction test. |
| E04 | Detect invalid sources and nested-source cycles. | Component ownership avoided this exact graph; failures were ad hoc. | Resolver uses `EntityId` cycle detection and reports an error. | Present | Keep; make errors navigable. | Resolver cycle test and screenshot. |
| E05 | View Blocks on a graph canvas. | `BlackboardWindow.DrawBlackboardView`, `DrawBlock`. | Blocks are vertical `EditorGUILayout` panels. | Missing | Restore a managed graph canvas. | Screenshot and renderer test seam. |
| E06 | Navigate with pan, pointer-centered zoom, grid, center, and frame/focus. | `EventWindow`, `DoZoom`, `DrawGrid`, `CenterBlackboard`, `CenterBlock`. | Scroll view; metadata Zoom is stored but unused. | Missing | Restore using authoring metadata. | Coordinate, zoom-bound, pan, center, and persistence tests. |
| E07 | Persist Block position, tint, zoom, and scroll without affecting runtime data. | Fields lived on legacy runtime components. | Position/tint/zoom/scroll live in authoring metadata; only tint and textual position are currently exposed. | Partial | Replace with direct canvas interaction while retaining the improved ownership boundary. | Metadata-only mutation and runtime-clone isolation tests. |
| E08 | Show Block-to-Block relationship lines. | `DrawConnections`; legacy actions override `GetConnectedBlocks`. | Managed seam remains, but migrated action overrides/authoring resolution are absent and no lines render. | Missing | Restore a managed connection-source contract and implement it for every supported Block-targeting action. | Connection-resolution tests for Call, Menu, MenuTimer, and any additional matrix-discovered action; screenshot. |
| E09 | Search by Block name and action/command content, then focus the result. | `UpdateFilteredBlocks`, `IsBlockNameMatch`, `IsCommandContentMatch`, search popup. | One toolbar string filters visible Block names/action type names but cannot focus results. | Partial | Restore indexed name/summary/content search and focus navigation. | Search-result and focus tests. |
| E10 | Select one Block, add/remove with modifiers, box-select, deselect, and keep selection. | `StartControlSelection`, modifier handling, drag rectangle, selected Block list. | Metadata stores one `SelectedBlockId`; the window does not select Blocks. | Missing | Restore; expand authoring-only metadata to support ordered multi-Block selection while preserving a primary selection. | Selection-state, box-select, modifier, serialization, and Undo tests. |
| E11 | Drag one or multiple selected Blocks as one Undoable gesture. | `OnMouseDrag` and selected Block list. | Auto-layout exists; manual drag does not. | Missing | Restore; record one complete-object Undo per gesture. | Multi-node move and single-Undo tests. |
| E12 | Add, rename, reorder, duplicate, copy/paste, and delete Blocks. | Blackboard and Block context/toolbars plus keyboard commands. | Controller and row buttons cover single-Block CRUD/reorder/copy/paste/duplicate. | Partial | Keep controller; restore canvas commands, multi-selection, cut, and standard keyboard/context access. | Controller regression plus interaction tests. |
| E13 | Use Unity standard Copy, Cut, Paste, Duplicate, Delete, and SoftDelete commands. | `OnValidateCommand`, `OnExecuteCommand`. | No command validation/execution handlers. | Missing | Restore with focus-safe routing to graph selection. | Event-command tests. |
| E14 | Use right-click canvas and node menus. | Context menus include structural and Play Mode commands. | Buttons only; no canvas/node context menus. | Missing | Restore. | Menu enablement and action tests. |
| E15 | Undo/Redo all structural and layout operations. | Unity component Undo APIs. | Controller and raw changes record owning object; canvas operations do not exist. | Partial | Preserve current complete-object pattern and extend it to every new gesture/command. | Undo/Redo test per operation family. |
| E16 | Edit Block name, description, tint, execution method, await/order mode, repeat policy, and trigger. | `BlockEditor` focused inspector and summary controls. | Name/tint/trigger have basic controls; execution settings are raw details; description is absent from managed model. | Partial | Restore focused detail UI; store author description in authoring metadata unless a runtime requirement is proven. | Detail editing, conditional-field, metadata isolation, and screenshot evidence. |
| E17 | Add/remove/reorder named action tracks. | Legacy Blocks/Invoke Action groups represented command/action grouping. | Controller and row buttons support tracks. | Present at basic level | Keep; expose in selected-Block detail and drag/drop surface. | Existing managed operation test plus detail UI test. |
| E18 | Move actions within and across tracks by drag/drop. | `CommandListAdaptor` nested/standalone drag targets, extraction, merge, reorder hysteresis. | Up/down buttons reorder only inside one track. | Missing | Restore with a controller operation that atomically updates definition order, groups, and selection. | Same-track, cross-track, empty-track, group-cleanup, and Undo tests. |
| E19 | Group/ungroup selected actions and preserve empty/presentation state where meaningful. | Invoke Action group presentation and extraction behavior. | Authoring group metadata supports group/ungroup only within a track. | Partial | Replace component grouping with managed track/group metadata; define empty-group behavior explicitly. | Existing grouping test plus empty/extraction/preservation tests. |
| E20 | Multi-select actions with click, range, Select All/None, copy/cut/paste/delete. | `BlockEditor`, `CommandEditor`, `CommandListAdaptor`, `InvokeActionCommandEditor`. | Toggle selection and single-action buttons; no range selection, cut, or list command routing. | Partial | Restore. | Range-selection and list-command tests. |
| E21 | See a readable action name, summary, warning/error badge, category, and help text. | `InvokeActionEditorUtility`, `CommandInfoAttribute`, selector popups. | Action type name only; GenericMenu uses full CLR type name. | Missing | Restore presentation derived from `CommandInfoAttribute` and safe `GetSummary` handling. | Display-name, summary-failure, severity, category, and tooltip tests. |
| E22 | Expand/collapse an action and edit only its relevant serialized fields. | `CommandEditor`, `InvokeActionCommandEditor`, property-visibility tests. | Raw owner tree is the only complete field editor. | Missing | Restore managed-reference property drawing, foldout state, `IsPropertyVisible`, and reorderable-array behavior. | Property visibility/height/foldout tests and screenshot. |
| E23 | Toggle action Enabled and edit Utility, Weight override, and Block During Execution when applicable. | Invoke Action headers and execution-setting summaries. | Fields remain on `ActionDefinition`; no focused controls. | Missing | Restore conditional controls based on execution and order modes. | Port composite-option/weight/utility visibility tests. |
| E24 | Add actions through a searchable, categorized menu with help. | `CommandSelectorPopupWindowContent`. | GenericMenu filtered by the window's global search; full type names, no categories/help/local search field. | Partial | Replace with a searchable managed-type picker. | Filtering, category, type validity, and selection tests. |
| E25 | Add/clear triggers through a searchable, categorized menu and edit trigger properties. | `EventSelectorPopupWindowContent`, `EventHandlerEditor`. | Basic type menu and Clear; properties only in raw owner tree. | Partial | Restore searchable picker and focused trigger detail. | Picker and serialized-property tests. |
| E26 | Add, rename, reorder, duplicate, delete, sort, and inspect variables. | `VariableListAdaptor` and variable context menu. | Add/rename/reorder/delete; no duplicate, sort, value field, or context menu. | Partial | Restore current managed variable workflow; sorting may be a context command rather than a permanent control. | CRUD/duplicate/sort/value/Undo tests. |
| E27 | Edit variable initial value with the correct control for primitive, Unity object, collection, and Blackboard-definition types. | 27 custom value/data drawers. | Variable rows show type/key/scope only; values are buried in raw details. No replacement custom drawers exist. | Missing | Replace the drawer family with a small managed drawer system covering all current definition and retained data-wrapper types. | Type matrix tests and screenshots for primitive, Unity object, collection, and nested definition. |
| E28 | Choose local/public/global scope and understand Play Mode global-value behavior. | Variable row scope and read-only global runtime display. | Scope popup exists; runtime value behavior is not shown. | Partial | Restore clear runtime-vs-default presentation without mutating definitions. | Scope and Play Mode display tests. |
| E29 | Select a compatible variable from action fields instead of editing IDs/references manually. | `VariableReferenceDrawer`, `VariableDataDrawer`, typed drawers, `AnyVariableAndDataPairDrawer`. | No general replacement drawer; retained action data wrappers still exist. | Missing | Restore a definition-ID-backed compatible-variable picker with source/type filtering and missing-reference diagnostics. | Compatibility, source, missing-ID, and assignment tests. |
| E30 | Find all usages of a variable. | `VariableListAdaptor.FindUsage`. | No current command. | Missing | Restore through a managed definition traversal and navigable results. | Traversal test across triggers/actions/nested data. |
| E31 | Select a Block target in Call/Menu-style actions without typing a fragile name. | `BlockReferenceDrawer`/`BlockEditor.BlockField`. | Target fields are raw `StringData`; no Block picker. | Missing | Restore a managed Block picker. Preserve runtime name compatibility initially; evaluate stable-ID migration separately. | Picker and rename-diagnostic tests. |
| E32 | Configure Perform Interruption by selecting actions, not raw IDs. | `PerformInterruptionDrawer`. | `targetActionIds` remains, but the drawer was deleted. | Missing | Replace with a managed same-list action selector using stable IDs. | Selection, stale-ID, self-exclusion, and reorder-preservation tests. |
| E33 | Automatic action/group metadata synchronization after reorder, add, remove, and cross-group moves. | Invoke Action editor utilities and tests. | Definition IDs remove parallel ID arrays, but selection/group metadata cleanup covers only current controller operations. | Partial, architecturally improved | Replace with invariant-preserving controller operations and validation after every structural mutation. | Port reorder/move/empty-group metadata tests. |
| E34 | Execute a selected Block, stop it, stop all, and optionally execute from a selected action during Play Mode. | Blackboard and Block context menus; Block editor Play From Selected. | Feedback only; no controls. Runtime supports Block execution by ID/name and first action index. | Missing | Restore Execute/Stop/Stop All. Include Execute From Selected only when action-to-runtime index mapping is deterministic. | PlayMode isolation/control tests and screenshot. |
| E35 | See Block/action status, progress, waiting explanation, success/failure, and nondeterministic execution guidance. | Executing icons, command-row fills, progress and waiting helpers. | Block state and action status labels only. | Partial | Restore actionable managed feedback; do not synthesize false linear progress. | Port deterministic/waiting/progress formatting tests plus PlayMode evidence. |
| E36 | Keep editor selection and transient feedback out of runtime clones. | Legacy mixed these concerns in components. | Managed architecture separates them. | Present, improved | Keep as a non-regression invariant. | Existing metadata isolation plus new selection/feedback isolation tests. |
| E37 | Show a Blackboard marker in the Unity Hierarchy. | `HierarchyIcons` for legacy Blackboard components. | No BlackboardBehaviour hierarchy marker. | Missing | Replace for `BlackboardBehaviour`, with a preference only if an equivalent setting remains justified. | Hierarchy callback test seam and screenshot. |
| E38 | Open the editor after asset changes and refresh an already-open target safely. | `AssetModProcessor`, window refresh hooks. | Undo/selection/update hooks resolve the target; no asset-postprocess refresh path. | Partial | Verify stale-reference/domain-reload behavior. Add a managed refresh hook only if characterization demonstrates a failure. | Domain reload/asset reimport manual evidence and resolver tests. |
| E39 | Export API reference documentation from editor metadata. | `ExportReferenceDocs`. | No replacement. | Outside core Blackboard authoring | Retire proposal or separate documentation-tool task; owner decision required. | Decision Log entry. |
| E40 | Generate new Variable component types from an Editor wizard. | `GenerateVariableHelper`, `GenerateVariableWindow`. | Components are intentionally removed; managed types are code-defined and catalogued. | Obsolete architecture | Retire proposal; if extensibility is still required, create a separate managed-definition generator task. | Decision Log entry. |
| E41 | Edit legacy Save Menu assets and create Save Menu objects. | `SaveMenuEditor`, `SaveMenuItems`. | Save-manager/menu component stack was removed. | Removed product surface | Retire proposal or separate save-product restoration task; owner decision required. | Decision Log entry. |

## Required Implementation Slices

The workflow rows resolve into five bounded implementation slices:

1. **Canvas and selection:** E05-E15, E37-E38.
2. **Block, track, and action detail:** E16-E25, E33.
3. **Variables and references:** E26-E32.
4. **Play Mode control and feedback:** E34-E36.
5. **Explicit product decisions:** E39-E41 and legacy serialized-content migration.

Slices 1-4 are required for Blackboard editor parity. Slice 5 contains no implicit
authorization to restore removed product systems; each row requires a separate owner
decision.

## Exhaustive Deleted Production File Classification

This section ensures every deleted production editor source is accounted for. Files
are historical paths under
`Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/` unless another root is
shown.

### Managed Blackboard surface: Replace

| Deleted file | Covered workflow rows | Managed direction |
| --- | --- | --- |
| `BlackboardEditor.cs` | E01, E03, E26-E28 | Managed host/asset Inspectors and shared editor launcher. |
| `BlackboardWindow.cs` | E05-E15, E34-E36 | Managed graph canvas and Play Mode controls. |
| `BlockEditor.cs` | E16-E25, E31, E34 | Selected-Block detail pane and managed pickers. |
| `BlockInspector.cs` | E16-E25 | Selected-Block/action detail pane. |
| `BlockInspectorStyleSheet.cs` | E16, E21-E23 | New managed editor styles; no runtime ownership. |
| `BlockReferenceDrawer.cs` | E31 | Managed Block picker. |
| `CommandEditor.cs` | E20-E25 | Managed action detail renderer and context commands. |
| `CommandListAdaptor.cs` | E18-E23, E33-E35 | Managed track/action list, drag/drop, grouping, metadata, and feedback. |
| `EventHandlerEditor.cs` | E25 | Managed trigger detail renderer. |
| `EventWindow.cs` | E06, E09-E14 | Testable canvas input router. |
| `InvokeActionEditorSelection.cs` | E20-E23, E33-E35 | Stable-ID selection and managed feedback helpers. |
| `InvokeActionEditorUtility.cs` | E18-E24, E33-E35 | Managed display, drag/drop, validation, and feedback helpers. |
| `VariableEditor.cs` | E26-E29 | Managed variable/value renderer. |
| `VariableListAdaptor.cs` | E26-E30 | Managed variable list and context commands. |
| `VariableReferenceDrawer.cs` | E29 | Compatible variable picker. |
| `AnyVariableAndDataPairDrawer.cs` | E27, E29 | Unified retained-data-wrapper/variable-reference drawer. |
| `HierarchyIcons.cs` | E37 | BlackboardBehaviour hierarchy marker. |

### Searchable selectors: Replace

| Deleted file | Covered workflow rows | Managed direction |
| --- | --- | --- |
| `PopupContent/CommandSelectorPopupWindowContent.cs` | E21, E24 | Searchable categorized action picker. |
| `PopupContent/EventSelectorPopupWindowContent.cs` | E25 | Searchable categorized trigger picker. |
| `PopupContent/VariableSelectPopupWindowContent.cs` | E26-E29 | Searchable categorized variable-type/reference picker. |

### Retained data-wrapper drawers: Replace with a unified managed drawer system

All of the following deleted drawers map to E27 and E29. The underlying data wrappers
still exist and are used by managed actions, so these files are not classified as
obsolete merely because Variable components were removed.

- `VariableTypes/AnimatorVariableDrawer.cs`
- `VariableTypes/AudioSourceVariableDrawer.cs`
- `VariableTypes/BooleanVariableDrawer.cs`
- `VariableTypes/ButtonVariableDrawer.cs`
- `VariableTypes/CharacterVariableDrawer.cs`
- `VariableTypes/CollectionVariableDrawer.cs`
- `VariableTypes/Collider2DVariableDrawer.cs`
- `VariableTypes/ColliderVariableDrawer.cs`
- `VariableTypes/ColorVariableDrawer.cs`
- `VariableTypes/FloatVariableDrawer.cs`
- `VariableTypes/GameObjectVariableDrawer.cs`
- `VariableTypes/IntegerVariableDrawer.cs`
- `VariableTypes/MaterialVariableDrawer.cs`
- `VariableTypes/Matrix4x4VariableDrawer.cs`
- `VariableTypes/ObjectVariableDrawer.cs`
- `VariableTypes/QuaternionVariableDrawer.cs`
- `VariableTypes/Rigidbody2DVariableDrawer.cs`
- `VariableTypes/RigidbodyVariableDrawer.cs`
- `VariableTypes/SpriteVariableDrawer.cs`
- `VariableTypes/StringDataMultiDrawer.cs`
- `VariableTypes/StringVariableDrawer.cs`
- `VariableTypes/TextureVariableDrawer.cs`
- `VariableTypes/TransformVariableDrawer.cs`
- `VariableTypes/VariableDataDrawer.cs`
- `VariableTypes/Vector2VariableDrawer.cs`
- `VariableTypes/Vector3VariableDrawer.cs`
- `VariableTypes/Vector4VariableDrawer.cs`

Implementation should favor shared type/value/reference logic with thin property
drawer entry points where Unity requires concrete attributes. It must not recreate
Variable components.

### Gear editor integration: Replace

| Deleted file | Covered workflow rows | Managed direction |
| --- | --- | --- |
| `Assets/GearEngine/Scripts/Game/GearEngine/Editor/InvokeActionCommandEditor.cs` | E18-E24, E33-E35 | Selected-track/action detail and managed composite settings. The component itself remains retired. |
| `Assets/GearEngine/Scripts/Game/GearEngine/Editor/PerformInterruptionDrawer.cs` | E32 | Managed same-action-list stable-ID selector. |

### Approval-required retirement proposals

| Deleted file | Covered workflow row | Proposal |
| --- | --- | --- |
| `AssetModProcessor.cs` | E38 | Do not restore a component-upgrade processor. Add only a managed refresh hook if a current failure is reproduced. |
| `ExportReferenceDocs.cs` | E39 | Retire from this parity task; create a separate documentation-tool task if required. |
| `GenerateVariableHelper.cs` | E40 | Retire component generator. |
| `GenerateVariableWindow.cs` | E40 | Retire component generator UI. |
| `SaveMenuEditor.cs` | E41 | Retire with removed Save Menu product surface unless separately restored. |
| `SaveMenuItems.cs` | E41 | Retire with removed Save Menu product surface unless separately restored. |

## Deleted Test Classification

| Deleted test file | Disposition |
| --- | --- |
| `Assets/3rdParty/ScaffoldVisualScripting/Tests/Editor/ScaffoldEditorResourcesTests.cs` | Port the user-visible layout, icon, and refresh outcomes that remain applicable; do not restore legacy texture ownership. |
| `Assets/3rdParty/ScaffoldVisualScripting/Tests/Editor/VariableValueReferenceTests.cs` | Port source selection, assignment, and type compatibility to managed definition-ID references. |
| `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/InvokeActionCommandTests.cs` | Runtime composite behavior is covered by Core tests; port only editor metadata/order/value invariants not already covered. |
| `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/InvokeActionEditorSelectionTests.cs` | Port display, selection, drag/drop, conditional controls, feedback, and layout behavior to managed editor helpers. |
| `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/InvokeActionPropertyVisibilityTests.cs` | Port property-visibility behavior to the managed action detail renderer. |

## Approval Gate

The owner approved implementation of required rows E01-E38 on 2026-07-27 and
confirmed that:

1. Practical workflow parity, rather than pixel-identical legacy visuals, is the
   target.
2. The managed architecture and lack of legacy component restoration remain fixed.
3. E39-E41 are retired from this parity task or moved into separately scoped work.
4. Automatic migration of pre-cutover component graphs remains a separate decision.

E39-E41 and automatic migration remain outside this implementation.

## Implementation Reconciliation

The owner approved the required editor restoration and the retirement decisions on
2026-07-27. The implementation keeps the managed architecture and restores the
legacy designer-facing interaction language.

| Rows | Final disposition | Implementation and evidence |
| --- | --- | --- |
| E01-E04 | Replaced | Managed GameObject/asset/menu entry points and target resolver. Source creation, Direct/asset/nested navigation, validation, and Back navigation remain on `BlackboardDefinitionWindow`, its launchers, Inspectors, and `BlackboardAuthoringTargetResolver`. |
| E05-E15 | Restored | `BlackboardGraphCanvas`, `BlackboardEditorStyles`, multi-selection metadata, multi-Block clipboard, controller gesture operations, context menus, and standard Unity commands restore the graph surface and editing workflow. Live visual inspection confirmed the old grid and node language. |
| E16-E20 | Restored | `BlackboardDetailPanel` and `BlackboardAuthoringController` provide focused Block metadata, execution settings, triggers, tracks, action drag/drop across tracks, grouping, range selection, multi-selection, and selection-aware deletion. |
| E21-E25 | Replaced | `BlackboardEditorDisplay`, `BlackboardTypeDropdown`, and `BlackboardSerializedPropertyRenderer` provide readable action presentation, searchable categorized managed-type selection, conditional action settings, and focused trigger/action property editing. |
| E26-E28 | Restored | The Variables tab supports managed value editing, add, rename, reorder, duplicate, delete, sorting, scopes, and Unity object values. |
| E29 | Replaced where the managed model supports stable references | `VariableReference` fields use a managed scope-aware picker. Retained compatibility `*Data` wrappers still reference their plain compatibility Variable objects; converting them is a separate runtime-data migration. |
| E30 | Replaced by definition inspection, not a dedicated results window | Managed serialized references are visible and diagnosable in the focused detail pane. A standalone cross-graph usage-results tool is not part of this visual restoration. |
| E31-E33 | Restored/Replaced | The serialized renderer provides Block selection for compatible target data, `PerformInterruption` uses a same-list stable-ID selector with stale-reference warnings, and controller mutations preserve selection/group invariants. |
| E34-E36 | Restored | `BlackboardEditorExecutionController` and the graph/detail menus provide Execute, Execute From Here, Stop, Stop All, and observational runtime feedback without serialized side effects. |
| E37-E38 | Replaced | `BlackboardHierarchyIcons` restores the hierarchy marker. Existing Undo, selection, update, and target-resolution hooks refresh the managed window without restoring the component-upgrade processor. |
| E39 | Intentionally retired | The reference-document exporter remains a separate documentation-tool concern. |
| E40 | Intentionally retired | The legacy Variable-component generator conflicts with managed definition types and remains removed. |
| E41 | Intentionally retired | The removed Save Menu product surface is outside Blackboard editor parity. |

Focused controller coverage is in
`Assets/3rdParty/ScaffoldVisualScripting/Tests/Authoring/Editor/BlackboardAuthoringControllerTests.cs`.
Workflow documentation is in `Docs/ScaffoldVisualScripting/README.md`,
`Docs/ScaffoldVisualScripting/BlockInspector.md`, and
`Docs/ScaffoldVisualScripting/BlackboardRuntime.md`. Compilation and analyzer evidence
is recorded in `Docs/ScaffoldVisualScripting/UnityErrorCheckReport.md`.
