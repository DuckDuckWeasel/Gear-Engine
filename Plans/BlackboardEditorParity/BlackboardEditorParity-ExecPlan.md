# Restore Blackboard Editor Workflow Parity on Managed Definitions

This ExecPlan is a living document. Update its Progress, Surprises & Discoveries,
Decision Log, Outcomes & Retrospective, and Artifacts and Notes sections as work
advances.

## Purpose / Big Picture

Restore the designer-facing Blackboard authoring workflow that was lost when the
legacy component graph was replaced, while retaining the completed managed-definition
architecture. A designer must be able to create, inspect, arrange, select, edit,
duplicate, delete, navigate, and run a Blackboard during Play Mode with the same
practical workflow coverage as the legacy Blackboard editor.

The completed runtime refactor remains the foundation:

```text
BlackboardDefinitionAsset / BlackboardBehaviour
    -> BlackboardDefinition + BlackboardAuthoringMetadata
    -> BlackboardAuthoringController
    -> plain Blackboard runtime (only while playing)
```

This work restores editor behavior; it does not restore `Blackboard`, `Block`,
`Command`, `EventHandler`, or Variable `MonoBehaviour` graph nodes. The editor must
operate solely on managed definitions and authoring metadata.

## Progress

- [x] Milestone 0: inventory and owner dispositions are complete in
  `LegacyEditorParityMatrix.md`.
- [x] Milestone 1: create the editor interaction foundation and preserve existing
  managed authoring operations.
- [x] Milestone 2: restore graph-canvas navigation, selection, editing, and
  shortcuts.
- [x] Milestone 3: restore inspectors, type selection, variables, and source
  navigation.
- [x] Milestone 4: restore safe Play Mode execution controls and feedback.
- [ ] Milestone 5: implementation, visual verification, documentation, formatting,
  compilation, and analyzer gates are complete. The corrected focused EditMode
  fixture still needs a final rerun after the owner saves or discards the unrelated
  dirty Main Scene.

## Surprises & Discoveries

- The replacement editor already provides managed add/remove/reorder, duplication,
  clipboard, grouping, layout metadata, validation, target resolution, and
  observational Play Mode feedback through `BlackboardAuthoringController`.
- The current `BlackboardDefinitionWindow` is a scrollable structured editor. It is
  not the legacy graph canvas and does not implement legacy canvas interaction,
  context commands, keyboard commands, or direct execute/stop controls.
- The old Blackboard window and its supporting inspector classes were deleted during
  the breaking cutover. They remain available as read-only historical source at
  commit `4fe499ed`, immediately before this refactor branch began.
- `BlackboardBehaviour` already exposes its plain runtime and the runtime exposes
  execution methods by `DefinitionId`, block name, and `StopAll`; restoring editor
  controls does not require a new component-owned execution path.
- The current authoring metadata stores only one selected Block ID. Restoring legacy
  additive and rectangle selection requires an authoring-only multi-selection model
  with one primary selection.
- The legacy Block description was removed from the managed model. If retained for
  designers, it belongs in `BlockAuthoringMetadata`, not the runtime definition.
- Managed action definitions retain Enabled, Utility, Weight override, and Block
  During Execution data, but the current editor exposes them only through the raw
  serialized owner tree.
- Twenty-seven deleted value/reference drawers still correspond to data wrappers used
  by current managed actions. Their loss affects action authoring as well as the
  retired Variable components.
- The managed connection extension seam remains on `ActionBase`, but migrated
  Block-targeting actions no longer supply authoring connection data. Canvas
  connection parity requires restoring that contract for current actions.
- `PerformInterruption` still stores stable target action IDs, but its deleted custom
  drawer leaves designers editing raw strings. The managed editor needs a same-list
  action selector.
- The retained legacy `*Data` compatibility values still reference plain compatibility
  Variable objects rather than managed `VariableDefinitionBase` IDs. Stable
  `VariableReference` fields can use the new managed picker; converting every retained
  compatibility action input is a separate runtime-data migration, not an editor-only
  change.
- Unity's source-structure gate requires the filename to match the single top-level
  type. The Undo-aware authoring operations therefore remain in
  `BlackboardAuthoringController.cs`; splitting that controller into partial files
  would violate the gate.
- The first managed-editor reconstruction preserved capability but diverged in
  presentation: action properties were rendered inline, the main list and action
  detail shared one scroll, private nested test types entered selectors, and managed
  implementation suffixes leaked into labels. Historical `BlockInspector` source and
  the supplied screenshots made these regressions concrete.
- The managed action catalog initially treated every public `IAction` implementation
  as designer-addable. The legacy selector required `CommandInfo`, so presentation
  wrappers such as `ShowUIFocusAction` appeared beside the canonical Scaffold action
  and produced two identically named entries.
- Nested managed fields were drawn through a newly created `SerializedObject` on each
  repaint. Unity stores `SerializedProperty.isExpanded` on that temporary property
  tree, so disclosure state disappeared unless the editor persisted it separately.

## Decision Log

- The definition model, stable IDs, cloning rules, VContainer ownership, and current
  `Scaffold.VisualScripting.Core -> Authoring -> Unity/Editor` dependency direction
  remain unchanged.
- Use supported Unity IMGUI and `Handles` APIs for the graph surface. Do not adopt
  the deprecated GraphView API or add a package solely for this editor.
- `BlackboardAuthoringController` remains the only editor mutation boundary. Every
  mutating UI path must enter through it or an equally Undo-aware managed operation
  that is added to it deliberately.
- Play Mode controls may call the existing plain runtime through a selected
  `BlackboardBehaviour`; they must be disabled outside Play Mode and must never
  serialize runtime state into a definition or its metadata.
- Legacy serialized component graphs remain unsupported by this plan. A migration
  tool, if still needed after the inventory, is a separate explicitly approved task.
- No legacy editor feature may be silently omitted. Every removed editor entry must
  be marked in the parity matrix as **Restored**, **Replaced**, or **Intentionally
  retired**, with an owner-approved reason for the last status.
- The owner approved practical workflow parity, retention of the managed
  architecture, retirement of the reference exporter/Variable component generator/
  Save Menu editor from this task, and separate treatment of pre-cutover component
  graph migration on 2026-07-27.
- Restore the legacy two-region detail model: the upper Block/list content owns its
  scroll and the selected-action inspector remains in a resizable lower preview.
- Production type selectors accept only public, constructible managed definitions and
  present designer names rather than implementation class suffixes.
- Action selectors additionally require `CommandInfo`, matching the legacy
  designer-menu contract. Existing undocumented serialized actions remain intact for
  compatibility but are not offered for new authoring.
- Nested managed-property expansion is keyed by authoring owner, runtime managed
  instance, and direct property name so repainting does not collapse user-opened
  fields.

## Outcomes & Retrospective

The managed editor now restores the graph-first workflow with legacy Scaffold node
textures and iconography while preserving managed definitions and the plain runtime.
It includes grid navigation, pan/zoom, selection and multi-move, context/keyboard
commands, relationship lines, focused Block/action/variable authoring, categorized
type pickers, runtime controls, source Back navigation, and hierarchy entry points.

The live Unity 6000.5.3f1 compile completed successfully. Visual inspection confirmed
the graph and detail pane render together. The focused EditMode fixture initially
reported 12 passing tests and one failed gesture-Undo regression; the implementation
was corrected by isolating the drag gesture into its own Undo group. A final rerun is
still required because the already-open user scene contains unrelated unsaved changes
and Unity refuses to enter its test scene without a save/discard decision.

The repository assembly-reference and pragma gates both report `TOTAL:0`. The analyzer
test/build gate reports `BUILD_EXIT:0` and `TOTAL:0`. Deterministic C# formatting and
one-type-per-file verification pass for all 20 changed C# files. The project batch
Unity wrapper could not acquire the open project's lock, so the successful live Unity
compile is the authoritative compilation result for this run.

The experiment established that the managed-runtime cutover did not require sacrificing
the designer experience. The durable boundary is to keep the old interaction language
in editor-only views and metadata, keep all mutations in one Undo-aware controller,
and address the runtime exclusively through stable definition IDs. Reusing the legacy
visual resources was low risk; reusing the legacy component ownership would have
undone the architectural improvement. The remaining compatibility `*Data` reference
model is a data-migration concern, not a reason to regress the editor architecture.

The regression review added another durable lesson: functional parity and visual
parity need separate acceptance evidence. A replacement can expose the same fields and
still feel wrong when selection, scroll ownership, preview placement, labels, zoom
bounds, or node/connection language changes. The focused regression layer now covers
these deterministic contracts, while interactive screenshots record the IMGUI result.

The latest regression established two additional editor contracts. Public interface
implementation is not sufficient evidence that a type belongs in a designer menu;
explicit authoring metadata is the stable boundary. Likewise, temporary serialized
views cannot own interaction state that users expect to survive repaint. The managed
architecture remains valid, but editor eligibility and transient UI state must be
modeled deliberately.

## Context and Orientation

The completed replacement editor lives at:

- `Assets/3rdParty/ScaffoldVisualScripting/Runtime/Editor/BlackboardDefinitionWindow.cs`
- `Assets/3rdParty/ScaffoldVisualScripting/Runtime/Editor/BlackboardAuthoringController.cs`
- `Assets/3rdParty/ScaffoldVisualScripting/Runtime/Editor/BlackboardAuthoringTargetResolver.cs`
- `Assets/3rdParty/ScaffoldVisualScripting/Runtime/Editor/BlackboardExecutionFeedback.cs`
- `Assets/3rdParty/ScaffoldVisualScripting/Runtime/Editor/BlackboardBehaviourInspector.cs`
- `Assets/3rdParty/ScaffoldVisualScripting/Tests/Authoring/Editor/BlackboardAuthoringControllerTests.cs`

The original canvas and inspector behavior is historical source, not production
dependencies:

- `git show 4fe499ed:Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlackboardWindow.cs`
- `git show 4fe499ed:Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlackboardEditor.cs`
- `git show 4fe499ed:Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlockInspector.cs`
- `git show 4fe499ed:Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/VariableListAdaptor.cs`
- `git show 4fe499ed:Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/PopupContent/CommandSelectorPopupWindowContent.cs`

`DefinitionId` is a stable identifier assigned to serialized definitions. It allows
the editor to target the matching runtime Block in Play Mode without retaining runtime
objects in assets. `BlackboardAuthoringMetadata` is serialized editor-only state such
as node positions, tint, zoom, scroll position, groups, and selection. A graph canvas
is the editor view that renders those definitions as moveable nodes; it does not own
or execute the graph.

The legacy editor contained broader Scaffold tools (for example Save Menu, legacy
variable generation, and component-specific drawers). The mandatory scope of this
plan is the Blackboard authoring workflow. Milestone 0 inventories every other
deleted editor entry and records whether it remains a supported product workflow;
none may be assumed to be restored merely because it was historical code.

## Parity Contract

The following Blackboard workflows are mandatory. The matrix created in Milestone 0
must state the historical evidence, replacement implementation location, automated
coverage, and manual/visual evidence for each row.

| Area | Required designer outcome |
| --- | --- |
| Entry points | Create a Blackboard host or definition asset, select an existing host/asset, and open the Blackboard editor from its Inspector and supported menu path. |
| Sources | Edit Direct, ScriptableObject-backed, and nested Blackboard-variable templates; show clear errors for invalid sources and cycles. |
| Canvas | See Blocks in a navigable graph surface with a grid, persisted zoom/scroll, clear labels, tint, and visible runtime state in Play Mode. |
| Navigation | Pan, zoom around pointer, frame/center the graph, search Blocks, and focus a selected or searched Block. |
| Selection | Click-select, additive select, box-select, deselect, and retain selection in `BlackboardAuthoringMetadata`. |
| Graph editing | Add, rename, move, reorder, duplicate, copy, paste, cut, and delete Blocks with Undo/Redo. Deep copies must regenerate definition IDs while retaining explicit Unity-object references. |
| Block detail | Edit trigger, tracks, actions, action order, groups, and serialized action data through a selected-Block inspector/detail pane. |
| Type selection | Add action, trigger, and variable types through searchable, valid managed-type menus; invalid or abstract types cannot be added. |
| Variables | Add, rename, reorder, inspect, and delete variables, including supported Unity-object values and Blackboard-definition references. |
| Context and shortcuts | Expose graph commands in a right-click context menu and support Unity standard Copy, Cut, Paste, Duplicate, Delete/SoftDelete, and Undo/Redo commands. |
| Play Mode | Show execution state and provide Execute selected Block, Stop selected Block, and Stop All only for a live selected `BlackboardBehaviour`; controls must not mutate serialized authoring data. |
| Safety | Display validation problems, preserve managed Undo, never create legacy graph components, and keep editor metadata out of runtime cloning. |

## Plan of Work

### Milestone 0: inventory and executable parity contract

1. Create `Plans/BlackboardEditorParity/LegacyEditorParityMatrix.md` from the
   historical commit and the current editor. Inventory the Blackboard window,
   Blackboard/Block/Command/Event/Variable inspectors and drawers, menu entries,
   shortcuts, and the deleted Gear editor integrations that depended on them.
2. For every row, capture: legacy class/method, designer outcome, current status,
   target managed implementation, regression-test type, visual/manual test, and
   disposition (`Restored`, `Replaced`, or `Intentionally retired`).
3. Treat all rows in the Parity Contract as `Required`; do not downgrade one without
   explicit owner approval recorded in the Decision Log and matrix.
4. Use the legacy editor tests as characterization specifications. Translate their
   observable behavior into managed-editor tests; do not revive their dependencies on
   deleted component types.
5. Record legacy content migration separately: enumerate any scenes, prefabs, or
   ScriptableObjects that still contain removed component GUIDs. Do not implement a
   migration path under this plan unless it is explicitly approved.

**Milestone exit:** the matrix has no unclassified deleted Blackboard editor feature,
all required workflows have acceptance evidence, and the restored user workflow is
unambiguous enough to implement without referring to a past chat.

### Milestone 1: managed editor interaction foundation

1. Split `BlackboardDefinitionWindow` into small responsibilities: window shell,
   graph canvas renderer, input/selection controller, context-command presenter,
   selected-definition detail pane, and Play Mode control presenter. Preserve its
   serializable window source and the target resolver.
2. Keep the existing `BlackboardAuthoringController` as the mutation authority. Add
   narrowly named operations only where the parity matrix requires a missing atomic
   operation, each with `Undo.RecordObject`/complete-object Undo semantics matching
   existing mutations.
3. Make graph rendering read `BlockAuthoringMetadata` for position, tint, selection,
   scroll, and zoom. Do not add layout fields to `BlackboardDefinition`,
   `BlockDefinition`, or runtime classes.
4. Extend authoring metadata with ordered selected Block IDs and a primary selection,
   preserving existing serialized `SelectedBlockId` data during the transition. Add
   optional Block author description to `BlockAuthoringMetadata` rather than Core.
5. Add a canvas test seam that converts a screen point to graph coordinates and
   resolves node hit tests without relying on global Unity Editor state. This keeps
   navigation and selection logic deterministic under EditMode tests.
6. Preserve the present structured serialized-detail editing temporarily behind the
   selected node; replace it only after the graph canvas can expose every editable
   managed field.

**Milestone exit:** an asset or wrapper opens one managed editor shell with a graph
canvas and detail pane, retains the existing target modes, and compiles without
legacy component references.

### Milestone 2: canvas interaction and structural editing

1. Render node cards for every Block, including name, trigger summary, track/action
   summary, tint, selection state, and current Play Mode execution indicator.
2. Implement panning, pointer-centered zoom, grid rendering, fit/center, search
   focus, and persisted viewport state. Clamp zoom and scroll to documented usable
   bounds and retain them in `BlackboardAuthoringMetadata`.
3. Implement click, additive click, empty-space deselect, and drag-rectangle
   selection. Moving one or several selected nodes updates only their metadata and
   records a single Undo operation per gesture.
4. Implement canvas and node context menus for Add Block, Copy, Cut, Paste,
   Duplicate, Delete, Execute, Stop, and Stop All. Enable actions only when their
   target and Play Mode conditions are valid.
5. Handle Unity ValidateCommand/ExecuteCommand events for Copy, Cut, Paste,
   Duplicate, Delete, and SoftDelete. Keyboard behavior must act on the graph
   selection and never on an unrelated Inspector field.
6. Use the controller for all structural effects. Copy/paste must deep-clone managed
   content, preserve explicit `UnityEngine.Object` references, and regenerate every
   new definition ID.
7. Add a managed authoring-time connection-source contract and implement it for every
   current Block-targeting action identified by the parity matrix, including Call,
   Menu, and MenuTimer. Render relationship lines from resolved definitions only;
   unresolved names/IDs must produce diagnostics rather than guessed connections.

**Milestone exit:** the canvas supports the complete navigation, selection, and
structural-editing rows of the Parity Contract with Undo/Redo and focused EditMode
coverage.

### Milestone 3: inspectors, menus, variables, and target navigation

1. Build a selected-Block detail inspector that supports trigger selection, track
   add/remove/reorder, action add/remove/reorder, grouping, and serialized managed
   field editing. Keep it synchronized with the selected canvas Block.
2. Build searchable managed-type pickers for actions, triggers, and variables from
   `BlackboardManagedTypeCatalog`. Filter non-instantiable, abstract, incompatible,
   and legacy component types before displaying them.
3. Restore the variable workflow: list/search, add, rename, reorder, duplicate,
   delete, sort, find references, and inspect every supported
   `VariableDefinitionBase`.
4. Replace the deleted drawer family with shared managed rendering for current
   variable definitions and retained action data wrappers. Provide type-compatible
   variable and Block pickers so designers do not edit definition IDs, component
   references, or Block names as raw values.
5. Add a managed drawer for `PerformInterruption` that selects actions from the same
   action list by stable definition ID, excludes the action itself, and reports stale
   IDs.
6. Preserve Direct, asset-backed, and nested variable template navigation through
   `BlackboardAuthoringTargetResolver`. Breadcrumbs or an equivalent Back control
   must make nested navigation understandable and return the user to the source.
7. Reintroduce compatible current menu/Inspector entry points. Add deprecated menu
   aliases only where the parity matrix confirms they are still a supported user
   workflow; aliases must invoke the managed editor rather than instantiate legacy
   prefabs/components.
8. Give validation errors a direct navigation target where possible, so a designer
   can select or focus the offending definition.

**Milestone exit:** every current managed Block, action, trigger, and variable can be
created and fully edited through the graph editor without exposing raw serialized
details as the only authoring path.

### Milestone 4: Play Mode controls and execution feedback

1. Add an editor-only execution-control presenter for a resolved
   `BlackboardBehaviour`. It calls the existing plain runtime by `DefinitionId` and
   uses `StopBlock(DefinitionId)` and `StopAll()` for control actions.
2. Enable controls only when `EditorApplication.isPlaying`, the source resolves to a
   live `BlackboardBehaviour`, `IsRuntimeAvailable` is true, and the selected Block
   has a matching runtime Block. Report a concise visible reason when unavailable.
3. Retain `BlackboardExecutionFeedback` as the read-only status source. Refresh
   visible state without allocating or serializing transient runtime data.
4. Verify controls do not alter definition IDs, action data, variable definitions,
   layout metadata, or Undo history. Stop operations affect only the selected runtime
   instance.
5. Cover start failure, disabled runtime, disposed runtime, and asset-only targets.

**Milestone exit:** designers can observe, execute, stop, and stop all on a live
selected Blackboard without a second execution path or serialized side effects.

### Milestone 5: documentation, verification, and handoff

1. Update `Docs/ScaffoldVisualScripting/BlackboardRuntime.md` and the module README
   (create it at the Visual Scripting package root if absent) with entry points,
   editor architecture, supported authoring workflows, Play Mode constraints, and
   legacy migration status.
2. Complete the parity matrix with implementation and evidence links. Any remaining
   intentionally retired row must have a product rationale and replacement guidance.
3. Capture before/after editor screenshots for: creating an asset, graph navigation,
   multi-selection/duplication, action/type selection, variable editing, nested
   source navigation, validation display, and Play Mode execute/stop feedback.
4. Run the focused editor tests, managed Core tests impacted by any controller API,
   and the narrow Unity PlayMode fixture for execution controls. Run the project
   validation gate after all focused checks pass.
5. Update this plan's Progress, Surprises & Discoveries, Decision Log, Outcomes &
   Retrospective, and Artifacts and Notes with actual commands and results. Commit
   the completed milestone only after all required gates are clean.

## Concrete Steps

From the repository root, implementation should proceed in this order:

1. Inspect the historical editor at `4fe499ed` with `git show`; do not check out or
   copy its component implementation into the current branch.
2. Author and approve `LegacyEditorParityMatrix.md` before code changes.
3. Add tests for the next parity slice before or alongside its managed implementation.
4. Implement one user-visible slice at a time: canvas navigation/selection, then
   structural commands, then detail editing/type menus/variables, then Play Mode
   controls.
5. For each C# slice, run the repository's C# formatting/analyzer workflow in fix
   and check modes against only the changed files, then resolve all compilation
   errors before proceeding.
6. Run the narrowest tests relevant to that slice. Do not run the full suite until
   the final milestone unless a changed public boundary or a failure requires it.
7. At the end, run `./.agents/scripts/validate-changes.sh` with the test selection
   required by the repository gate, capture its logs, and update the plan.

## Validation and Acceptance

### Automated characterization and regression coverage

- Extend `Scaffold.VisualScripting.Editor.Tests` with focused EditMode tests for:
  - coordinate transforms, hit testing, zoom bounds, pan, and selection;
  - multi-selection, box selection, move, and metadata-only layout persistence;
  - context and keyboard Copy/Cut/Paste/Duplicate/Delete behavior;
  - Undo/Redo for every mutating canvas gesture and detail operation;
  - filtered type menus and rejected invalid types;
  - variable editing and target resolution, including nested templates and cycles;
  - retained action data-wrapper controls, compatible variable/Block pickers,
    connection resolution, and the Perform Interruption stable-ID selector;
  - execution-control enablement and the guarantee of no serialized mutation.
- Keep existing controller tests as regression coverage for ID regeneration, Unity
  object reference preservation, grouping, and target resolution.
- Add a narrow PlayMode test under `Scaffold.VisualScripting.Unity.Tests` for running
  an editor-command-selected Block against one `BlackboardBehaviour`, stopping it,
  and proving another runtime remains unaffected.
- Do not reintroduce deleted legacy editor test assemblies or component types merely
  to make a test compile; port the asserted workflow to the managed API.

### Manual and visual acceptance

On Unity 6000.5.3f1, record screenshots or a short capture showing all mandatory
Parity Contract rows. A reviewer must be able to confirm:

1. A Blackboard asset and a BlackboardBehaviour both open the same managed editor.
2. A designer can arrange a graph with pan/zoom, grid, selection, and persisted
   layout, then undo/redo the arrangement.
3. A designer can create, select, edit, copy/cut/paste/duplicate/delete Blocks,
   actions, triggers, and variables without new legacy components appearing on the
   GameObject.
4. Context-menu and keyboard commands behave on the selected graph data.
5. Nested sources can be opened and safely navigated back from.
6. During Play Mode, the editor displays feedback and Execute/Stop/Stop All affect
   only the selected live Blackboard; asset-only and non-Play-Mode controls are
   disabled with an explanation.

### Definition of Done

- Every parity-matrix row is Restored, Replaced with equal designer outcome, or
  explicitly approved as Intentionally retired.
- All mandatory Parity Contract rows are Restored or Replaced; none is silently
  retired.
- The completed editor uses only managed definitions, metadata, the plain runtime,
  and declared assembly dependencies; no legacy graph component or serialized GUID
  returns.
- Tests are green, Unity reports zero compilation errors, analyzer diagnostics are
  zero, and the project validation gate is clean for this change.
- Documentation and visual evidence are updated and linked from the matrix.

## Idempotence and Recovery

- The parity matrix is documentation-only and can be revised without affecting Unity
  assets.
- All editor mutations must be Undoable. If a canvas gesture produces unexpected
  serialized data, use Unity Undo first, then restore the affected asset from version
  control only after confirming the exact target.
- Do not bulk-convert, delete, or rewrite existing authoring assets as part of this
  plan. The current breaking cutover has no automatic migration promise.
- If a canvas implementation proves incompatible with the current Unity editor APIs,
  keep the existing structured editor operational, record the constraint here, and
  replace only the rendering/input layer; do not bypass the controller or move state
  into runtime definitions.
- If an old feature depends intrinsically on a removed component model, mark it
  blocked in the matrix and request a product decision rather than recreating hidden
  component ownership.

## Artifacts and Notes

- Primary parity inventory: `Plans/BlackboardEditorParity/LegacyEditorParityMatrix.md`
  (inventory and owner dispositions approved).
- Editor tests: `Assets/3rdParty/ScaffoldVisualScripting/Tests/Authoring/Editor/`.
- Play Mode control tests: `Assets/3rdParty/ScaffoldVisualScripting/Tests/Unity/PlayMode/`.
- User documentation: `Docs/ScaffoldVisualScripting/BlackboardRuntime.md` and the
  Visual Scripting module README.
- Compile and analyzer report:
  `Docs/ScaffoldVisualScripting/UnityErrorCheckReport.md`.
- Visual evidence: retain screenshots and test reports under the repository's
  existing `Logs/Tests/BlackboardRuntimeRefactor/` convention or a new clearly named
  `Logs/Tests/BlackboardEditorParity/` directory, and link them from the matrix.

## Interfaces and Dependencies

- `Scaffold.VisualScripting.Editor` may reference only its existing declared
  `Core`, `Authoring`, and `Unity` assemblies plus Unity editor APIs. It must not add
  a reverse dependency from Core or Authoring to Editor.
- `BlackboardAuthoringController` is the authoring mutation API. New editor controls
  should prefer operations such as `AddBlock`, `MoveBlock`, `CopyBlock`,
  `PasteBlock`, `DuplicateBlock`, `RemoveBlock`, `AddAction`, `SetTrigger`, and
  `AddVariable`; add missing methods there only when the operation is reusable and
  testable outside an `EditorWindow`.
- `BlackboardAuthoringTargetResolver` remains responsible for Direct, asset-backed,
  and nested-variable definition sources and cycle detection.
- `BlackboardExecutionFeedback` remains observational. Execution controls use the
  existing `BlackboardBehaviour.Runtime` plain runtime API and must be editor-only.
- VContainer remains responsible for runtime construction; no editor path may create
  a second runtime factory or mutable service locator.
