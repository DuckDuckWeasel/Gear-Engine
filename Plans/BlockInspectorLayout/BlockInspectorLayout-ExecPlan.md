# Modernize the Block Inspector layout and replace the legacy mushroom icon

This ExecPlan is a living document. Keep the `Progress`, `Surprises & Discoveries`,
`Decision Log`, and `Outcomes & Retrospective` sections current while the work is
implemented.

## Purpose / Big Picture

The current Block Inspector exposes all block settings as one long, mostly ungrouped
IMGUI form. The target design, shown in the approved reference, makes the first screen
easy to scan without changing any block or command behaviour:

1. An identity card presents the block icon, name, tint, enabled state, and description.
2. Three compact controls expose execution, await, and event settings as a single
   summary row.
3. Behaviour and timing settings move into an expanded, titled section with its help
   message immediately below the settings it explains.
4. Callers stays a collapsed section until the user asks for it.
5. The existing command-list and command-detail workflow remains functionally intact.
   Its existing visual identity transition is preserved: an empty `Invoke Action` becomes
   visually identified by its first added action (for example, `SetAnimBool`).

The red mushroom must be replaced by a neutral **flow-graph icon**: three rounded
square nodes connected by two directional lines, rendered in off-white on transparent
background. It communicates a visual-scripting flowchart, works at small sizes, and is
not tied to the legacy Scaffold branding. The same icon will appear in both the Block
Inspector and the Unity Hierarchy, where the current mushroom is also used.

After implementation, opening a selected block will show the new hierarchy while all
existing serialized values, selection, command reordering, nested-action drag/drop,
copy/paste, and Undo behaviour continue to work.

## Amendment — visual QA corrections (2026-07-17)

Visual QA identified four issues in the first implementation:

1. The three execution controls repeat their labels inside the selectable field
   (`Execution: Sequence`, `Await: Wait All`, `Event: Game Started`). Each must instead
   have a compact header above it (`Execution`, `Await`, `Event`) and a value-only popup
   below it (`Sequence`, `Wait All`, `Game Started`).
2. The Description field has excessive empty space. It must start at one line, grow with
   its wrapped text to a fixed maximum of four lines, then become vertically scrollable.
3. The Custom Tint toggle has no clear effect. Replace it with an always-visible tint
   swatch: display the effective block tint and, on a user edit, set `useCustomTint` to
   `true` and persist the selected color. Do not display a separate Custom Tint toggle.
4. The native Inspector object header still displays a mushroom because
   `BlockInspector.cs.meta` points to `Textures/ScriptIcon.png`. Reassign that icon to
   the new flow-graph texture as well.

The Block Inspector remains IMGUI. “Base stylesheet” in this plan means one shared,
cached C# IMGUI style sheet, not a `.uss` file: a UI Toolkit stylesheet cannot style
`EditorGUILayout` controls without rewriting the Inspector to UXML/UI Toolkit.

## Progress

- [x] M1 — Add and register Free/Pro flow-graph editor icon assets.
- [x] M2 — Replace mushroom terminology and hierarchy rendering with the flow-graph
  resource.
- [x] M3 — Restructure the Block Inspector top panel to the approved identity card,
  summary row, and grouped foldouts.
- [x] M4 — Add editor tests and focused documentation.
- [ ] M5 — Perform Unity visual QA and run the repository quality gate. Blocked in this
  environment because the project is open in another Unity instance and PowerShell 7 is
  unavailable for the repository gate.
- [x] M6 — Apply the visual-QA corrections: labelled summary columns, auto-growing
  Description, direct tint swatch, native header icon replacement, and shared IMGUI
  stylesheet.
- [ ] M7 — Perform Unity visual acceptance and run the targeted Editor tests. The editor
  and test assemblies compile with 0 errors; execution remains blocked while the project
  is open in another Unity instance.

## Surprises & Discoveries

- The Block Inspector is an IMGUI custom editor, not UI Toolkit. `BlockInspector` owns
  the top/bottom resizable inspector split and delegates block fields to `BlockEditor`.
- `BlockEditor` already owns the serialized properties and the command `ReorderableList`.
  Reusing that object keeps selection, Undo, copy/paste, and command drag/drop intact.
- `ScaffoldEditorResources` is generated from the filenames below
  `Assets/3rdParty/ScaffoldVisualScripting/EditorResources/`; its generated partial
  class must be refreshed after adding or renaming an icon.
- `CommandListAdaptor` previously registered Invoke Action destinations only over the
  expanded child rows. A single-action or collapsed target therefore required a precise drop
  on a small area, which made moving an action into a group difficult.
- The three summary controls had a 150-pixel minimum width each. In a narrow Inspector,
  that forced the containing scroll view to create horizontal overflow instead of adapting
  the layout.
- `ScaffoldMushroom` is consumed by `HierarchyIcons` as well as the Inspector resource
  set. Replacing only the visible Inspector image would leave the old brand in the
  Hierarchy.
- The worktree already contains user changes in `BlockEditor.cs`,
  `CommandListAdaptor.cs`, and related Scaffold files. This feature must be integrated
  with those changes and must not discard or overwrite them.
- The existing `CommandListAdaptor` and `InvokeActionCommandEditor` already implement
  the requested action-summary transition: an empty item reads `Invoke Action`, and a
  single added action supplies the visible label. No duplicate implementation was added.
- `dotnet build ScaffoldEditor.csproj --no-restore` completed with 0 errors. Unity
  batch-mode tests cannot start while the user has the project open. The repository
  validation script also requires PowerShell 7, which is not installed in this shell.
- The remaining mushroom is `Textures/ScriptIcon.png`, assigned as the icon of the
  `BlockInspector` MonoScript in `Scripts/Editor/BlockInspector.cs.meta`. It is separate
  from `ScaffoldEditorResources` and the Hierarchy icon.
- `dotnet build ScaffoldEditor.csproj --no-restore` and
  `dotnet build Scaffold.EditorTests.csproj --no-restore` both complete with 0 errors
  after the visual-QA amendment.

## Decision Log

- **Decision:** Implement only the approved top-panel hierarchy in this iteration;
  preserve the existing Commands list and selected-command detail split.
  **Rationale:** The supplied approved reference ends at `Callers`. The current command
  interaction contains bespoke nested action groups, drag/drop targets, and deferred
  selection/copy/paste handling. Altering it without a separately approved command
  workspace design risks behaviour regressions unrelated to this visual request.

- **Decision:** Use a project-owned 32 x 32 transparent flow-graph raster icon in Free
  and Pro variants, rather than a Unity built-in icon or an external icon package.
  **Rationale:** Unity built-in icon names are version-dependent and a project asset gives
  predictable hierarchy and inspector rendering. The icon remains available offline and
  retains consistent visual ownership.

- **Decision:** Name the resource `flow_graph` and expose it as
  `ScaffoldEditorResources.FlowGraph`; remove editor-only references to
  `ScaffoldMushroom` in this scope.
  **Rationale:** The public resource name, preference copy, and implementation should
  describe what the icon means, not the retired artwork.

- **Decision:** Keep the existing `BlockInspector` resize divider and separate command
  scroll area.
  **Rationale:** It gives users control over property versus command-detail space and is
  independent of the visual hierarchy being changed.

- **Decision:** Treat the command-list label as an action summary, not an immutable
  command-class name. An empty command reads `Invoke Action`; after its first action is
  added, the list row reads that action's display name. The details panel continues to
  identify the editable container as `Invoke Action`.
  **Rationale:** This is the actual authoring mental model: users scan what the block
  will do (`SetAnimBool`), while the editor still exposes the enclosing command and its
  action collection when selected. It also avoids presenting a generic list of identical
  `Invoke Action` rows after commands are populated.

- **Decision:** Add a `BlockInspectorStyleSheet` C# class in the `ScaffoldEditor` assembly
  as the single source of truth for IMGUI spacing, colors, borders, and cached styles.
  **Rationale:** The current Inspector is IMGUI. A shared C# style registry gives all
  inspector sections the requested consistent design without a costly and risky UI
  Toolkit rewrite.

- **Decision:** A tint edit always enables Custom Tint; there is no separate visible
  toggle in the identity card.
  **Rationale:** The selected color is the primary user intent. This removes the
  confusing state where a tint control appears but has no observable effect. The existing
  serialized boolean remains for backward compatibility and is set automatically on edit.

## Outcomes & Retrospective

Implemented the approved Inspector hierarchy, a shared Free/Pro flow-graph icon, and
preference migration from the legacy mushroom visibility key. The editor assembly builds
with zero errors. The amendment adds the shared IMGUI style sheet, value-only summary
popups with labels above, auto-growing Description, direct tint editing, and the native
header icon reassignment. Manual Unity visual verification, targeted test execution, and
the PowerShell quality gate remain for a Unity session where the project is not already
open.

## Context and Orientation

### Existing ownership

| Path | Current responsibility | Planned responsibility |
|---|---|---|
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlockInspector.cs` | Hosts the custom Inspector, scroll areas, selected command editor, and resize divider. | Keep the host, sizing, selection cache, and divider; call the reorganized block drawing surface. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlockEditor.cs` | Draws name, block settings, event handler, callers, command list, and command toolbar. | Split the top form into explicit identity, execution-summary, behaviour/timing, and callers drawing helpers while retaining serialized edits and command operations. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/CommandListAdaptor.cs` | Draws the specialized command list, nested Invoke Action groups, selection, and drag/drop. | Preserve the existing summary transition: an empty Invoke Action shows `Invoke Action`; one with actions displays its first action as the row/group identity. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/ScaffoldEditorResources.cs` | Generates and binds editor texture resources by filename. | Regenerate/bind the new `flow_graph` Free and Pro textures. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/ScaffoldEditorResourcesGenerated.cs` | Generated properties for editor texture resources. | Regenerate so it exposes `FlowGraph` and no obsolete mushroom property. Do not manually add a pragma suppression. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/HierarchyIcons.cs` | Draws the flowchart icon next to eligible GameObjects. | Render `FlowGraph`; rename comments and internal symbols to remove mushroom wording. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/ScaffoldEditorPreferences.cs` | Stores the hierarchy icon visibility preference. | Rename user-facing label and persisted key only through a migration-safe read fallback. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlockInspector.cs.meta` | Assigns Unity's native object-header icon to the `BlockInspector` script. | Change its texture GUID from the mushroom `ScriptIcon.png` to the Free flow-graph texture. |
| `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlockInspectorStyleSheet.cs` | New file. | Own cached GUI styles, section/card drawing helpers, spacing, and Description min/max line metrics for the IMGUI Inspector. |

### Visual specification

Use Unity Pro skin controls and spacing; do not imitate the mockup by introducing a
runtime UI framework or consumer-app styling.

| Area | Required layout and interaction |
|---|---|
| Inspector title | Keep Unity's native Inspector chrome. The first content label is `Block Inspector`. |
| Identity card | One bordered container. Left: 32 x 32 flow-graph icon. Right: `Block Name` text field, a direct-edit tint swatch, then Description. The swatch always shows the effective tint; changing it writes `tint` and enables `useCustomTint`. Keep unique-name normalization through `Flowchart.GetUniqueBlockKey`. |
| Description | Start at one text line. Measure wrapped text using the shared text-area style; grow up to four lines plus vertical padding. Above that cap, keep the fixed maximum height and render the text area inside a vertical scroll view that retains a per-inspector scroll position. |
| Execution summary | One horizontal container with three equal-width columns. Each column has a `miniLabel` header (`Execution`, `Await`, `Event`) above a value-only popup. Disable only the Await value popup when Sequence makes it inapplicable. Clicking Event must open the existing event-selector popup. |
| Behaviour & Timing | Expanded foldout by default for the current inspector session. Its body contains `Suppress All Auto Selections`, event-handler fields relevant to the selected event, and timing fields such as `Wait For Frames`. Render the existing help message directly inside this section. |
| Callers | Collapsed foldout by default. When expanded, calculate and show the existing caller list as read-only. |
| Commands and below | Preserve the present `CommandListAdaptor`, toolbar, selected-command editor, scroll behaviour, resize divider, keyboard shortcuts, context menu, and nested action drag/drop. The list row starts as `Invoke Action` when its action collection is empty; after an action is added it adopts that first action's name, while the selected details editor remains headed `Invoke Action`. |

## Plan of Work

### M1 — Create the flow-graph resource

1. Create the following 32 x 32 PNG assets with transparent backgrounds:
   - `Assets/3rdParty/ScaffoldVisualScripting/EditorResources/Icons/flow_graph.png`
   - `Assets/3rdParty/ScaffoldVisualScripting/EditorResources/Icons/Pro/flow_graph.png`
2. Use the same simple node-and-connector silhouette in both files. Tune the Free variant
   for Unity's light skin and the Pro variant for dark skin; retain at least 2 px of
   transparent padding so the icon is not clipped by Hierarchy rows.
3. Set import settings to `Texture Type: Editor GUI and Legacy GUI`, alpha transparency
   enabled, no mipmaps, point or bilinear filtering consistent with the existing editor
   icon assets, and generate/commit the `.meta` files.
4. Delete only the old `EditorResources/Icons/**/fungus_mushroom.png` assets and their
   metas after `FlowGraph` loads correctly. Do not delete the unrelated runtime texture
   `Textures/Mushroom.png` unless a separate asset audit confirms it is unused.
5. Select `EditorResources/ScaffoldEditorResources.asset` and invoke **Sync with
   EditorResources folder**. Confirm the regenerated
   `ScaffoldEditorResourcesGenerated.cs` contains `flow_graph` and the `FlowGraph`
   accessor, then review the generated diff before saving it.

### M2 — Replace shared mushroom usage safely

1. In `HierarchyIcons.cs`, replace `ScaffoldEditorResources.ScaffoldMushroom` with
   `ScaffoldEditorResources.FlowGraph`; update comments, local identifiers, and class
   documentation from “mushroom” to “flowchart icon”. Keep the hierarchy callback,
   visibility preference behaviour, cached IDs, and `EntityId` sorting unchanged.
2. In `ScaffoldEditorPreferences.cs`, change the preference label to **Hide Flowchart
   Icon in Hierarchy**. Introduce a new `hideFlowchartIcon` field and a new persisted key,
   but on initial read fall back to the legacy `hideMushroomInHierarchy` key when the new
   key is absent. Save to the new key thereafter. This preserves users' existing hidden
   setting across the rename.
3. Update every code reference to use the renamed preference. Use `rg -n -i
   'mushroom|ScaffoldMushroom|hideMushroom' Assets/3rdParty/ScaffoldVisualScripting` to
   verify that only intentionally retained migration support or unrelated runtime art
   remains.

### M3 — Implement the approved IMGUI hierarchy

1. In `BlockEditor.cs`, keep `serializedObject.Update()` at the beginning and
   `serializedObject.ApplyModifiedProperties()` at the end of the block draw. Do not
   change field names, the `Block` runtime model, or the `CommandListAdaptor` constructor.
2. Replace the separate `DrawBlockName` top row with a dedicated identity-card helper
   called from `DrawBlockGUI`. It must draw the flow-graph texture with fixed dimensions,
   retain the existing unique-name check, and bind `useCustomTint`, `tint`, and
   `description` through `SerializedProperty`.
3. Add private IMGUI layout/style helpers in `BlockEditor` (or one tightly scoped editor
   style helper in the same assembly) for card backgrounds, section headers, muted help
   content, compact summary popups, and consistent 8 px padding. Cache styles; do not
   allocate new `GUIStyle` instances every repaint.
4. Replace the vertical `Execution Method` / disabled `Await Mode` rows and the separate
   `Execute On Event` row with the three-control execution summary. Retain all existing
   serialized-property bindings, tooltips, disabled-state logic, and event popup sizing.
5. Extract the current event-handler inspector body into the `Behaviour & Timing`
   section. Ensure `EventHandlerEditor` is created only for a non-null handler, destroyed
   in the same GUI cycle, and still marks `SelectedBlockDataStale` when changed.
6. Move `suppressAllAutoSelections`, timing/event-specific controls, and the explanatory
   HelpBox inside the expanded `Behaviour & Timing` foldout. Preserve their values and
   the current undo/dirty behaviour.
7. Keep the existing callers string cache and content, but render it through the new
   collapsed-by-default `Callers` foldout. Do not call `FindObjectsOfType` unless the
   foldout opens.
8. Leave command list drawing exactly after the grouped block settings. Preserve the
   current `CommandListAdaptor` rule where an empty `Invoke Action` has the generic
   label and a populated item summarizes its first action. Keep the right-click
   context-menu workaround, keyboard commands, deferred `actionList`, null command
   cleanup, and `SelectedBlockDataStale` update.
9. In `BlockInspector.cs`, remove the old standalone call to `DrawBlockName` once the
   identity card owns name drawing. Preserve scroll positions, selected-command caching,
   `BlockViewHeight`, resize clamping, and selected command inspector rendering.

### M4 — Tests and documentation

1. Add `Assets/3rdParty/ScaffoldVisualScripting/Tests/Editor/Scaffold.EditorTests.asmdef`
   (Editor only, `UNITY_INCLUDE_TESTS`) referencing `UnityEngine.TestRunner`,
   `UnityEditor.TestRunner`, `Scaffold`, and `ScaffoldEditor`. Keep all dependencies
   explicit.
2. Add `ScaffoldEditorResourceTests.cs` that loads `ScaffoldEditorResources.FlowGraph`
   under both Pro and light skin test conditions (restore the previous skin value in
   `TearDown`) and verifies that the replacement resource is non-null and has the
   expected dimensions.
3. Add `BlockEditorLayoutTests.cs` using a temporary `GameObject`, `Flowchart`, and
   `Block`. Verify that the custom editor can render the no-handler and configured-event
   paths without exceptions, that setting a duplicate block name still resolves through
   `GetUniqueBlockKey`, and that command selection remains bound after the updated block
   panel draws. Add focused coverage for the command summary state: an empty
   `InvokeActionCommand` resolves to `Invoke Action`, and after a first action is added
   it resolves to that action's display/type name. Destroy all temporary Unity objects in
   `TearDown`.
4. Add `Docs/ScaffoldVisualScripting/BlockInspector.md`. Document the four visual
   sections, the flow-graph icon purpose and asset locations, the unchanged command
   workflow, and how to regenerate `ScaffoldEditorResourcesGenerated.cs` after a future
   editor icon change.

### M5 — Validate and hand off

1. Open a flowchart with blocks with and without event handlers. Verify at narrow and
   wide Inspector widths that fields neither overlap nor clip, the event popup opens,
   Await disables correctly for Sequence, Custom Tint exposes its swatch only when
   enabled, and Callers remains lazy/collapsed by default.
2. Verify the Hierarchy shows the flow-graph icon beside flowchart GameObjects, and the
   renamed preference hides it and retains the prior hidden state after restart.
3. Verify command operations: selecting, reordering, adding, duplicating, deleting,
   copying/pasting, context-clicking, keyboard shortcuts, selecting a nested action, and
   dragging a nested action within/between Invoke Action groups.
4. Run the new Editor tests, then run `.agents/scripts/validate-changes.cmd` from the
   repository root. Resolve all compiler, analyzer, and test failures before commit.
5. Capture a Pro-skin Inspector screenshot matching the approved layout, with the
   flow-graph icon visible, and link it in the implementation handoff.

### M6 — Apply the visual-QA corrections and base stylesheet

1. Create
   `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlockInspectorStyleSheet.cs`.
   Make it an `internal static` IMGUI style sheet with lazily cached `GUIStyle` instances
   for: identity card, section card, section foldout header, summary header, summary
   popup, description text area, muted help content, and a selected summary state. Define
   named constants for outer padding, field spacing, minimum description lines (1), and
   maximum description lines (4). Never construct a `GUIStyle` during every repaint.
2. Refactor `BlockEditor.DrawExecutionSummary` so each of its three equal-width columns
   uses `BlockInspectorStyleSheet.SummaryHeader` to draw the header above the control.
   `DrawEnumSummaryPopup` must pass only the enum display value to the popup; the Event
   popup must pass only the selected event name. Keep the existing tooltips, GenericMenu
   values, Undo record, and disabled Await behaviour.
3. Add a `Vector2 descriptionScrollPosition` field to `BlockEditor` and a
   `DrawAutoGrowingDescription(SerializedProperty)` helper. During layout, calculate the
   content height using `BlockInspectorStyleSheet.DescriptionTextArea.CalcHeight` and the
   available content width. Clamp it between the one-line and four-line heights. Draw a
   direct `EditorGUI.TextArea` at the clamped height. When the calculated height exceeds
   the cap, place it inside `GUILayout.BeginScrollView`/`EndScrollView`; otherwise draw
   without a scrollbar. Keep the same serialized `description` property and Undo flow.
4. Replace the visible `useCustomTint` toggle in the identity card with a color field
   labelled `Tint`. It always renders the block's effective tint. On `EndChangeCheck`,
   record Undo for the Block, write the `tint` SerializedProperty, set
   `useCustomTint.boolValue = true`, apply properties, mark the target dirty, and set
   `SelectedBlockDataStale`. Do not remove either serialized field.
5. Route all top-panel cards, headers, foldouts, popup columns, Description, and help
   spacing through `BlockInspectorStyleSheet`; remove duplicate inline `EditorStyles`
   choices and magic spacing numbers from `BlockEditor`.
6. Update `BlockInspector.cs.meta` to use the Free `flow_graph.png` texture GUID with
   texture file ID `2800000`, replacing the `ScriptIcon.png` GUID. Confirm Unity shows
   the flow graph next to `(Block Inspector)` in the native header. Retain
   `Textures/ScriptIcon.png` because it may be used by other legacy assets; this change
   only removes the BlockInspector script's reference to it.
7. Update `Docs/ScaffoldVisualScripting/BlockInspector.md` with the column-header rule,
   Description expansion/scroll rule, direct tint behaviour, and the distinction between
   the native script-header icon and the shared editor resource icon.

### M7 — Tests and visual acceptance for the amendment

1. Add unit coverage for the pure description-height clamp calculation exposed by the
   stylesheet/helper: empty and one-line text use one line, four lines use the maximum
   without scrolling, and oversized text is clamped to the maximum and reports that a
   scrollbar is required.
2. Add an editor-resource assertion that `BlockInspector.cs.meta` references the
   flow-graph GUID, and retain the Free/Pro `FlowGraph` texture checks.
3. Manually verify at narrow and wide Inspector widths: labels are above the three
   popups; values do not repeat labels; Description starts compact, grows, and scrolls
   after four lines; clicking Tint opens Unity's color picker and changes the block tint;
   and both the native header and identity card no longer show a mushroom.

## Concrete Steps

From `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine`:

1. Confirm the worktree changes in the Scaffold editor files belong to the active work;
   do not reset them. Implement the plan in a focused branch/worktree.
2. Add the two icon files and their metas, sync the resource asset, and confirm the
   generated resource source compiles before starting layout changes.
3. Complete M2 and M3 in small commits or locally reviewable checkpoints; after each,
   open the Inspector once so Unity imports assets and serializes the resource reference.
4. Add M4 tests and docs; use the Unity Test Runner or the repository test script to run
   only the new editor test assembly during iteration.
5. Complete the M5 manual matrix and full quality gate. Update the living sections above
   with results and any Unity-version-specific findings before creating the final commit.

## Validation and Acceptance

- [ ] A selected Block displays an identity card with a non-mushroom flow-graph icon,
  editable unique block name, Custom Tint/tint swatch, and description.
- [ ] Execution, Await, and Event are visible as one compact summary row and preserve
  every pre-existing serialized value and disabled-state rule.
- [ ] `Execution`, `Await`, and `Event` appear as headers above their value-only popups;
  no value repeats its header text.
- [ ] Description uses one line when short, grows only to four lines, then scrolls; it
  does not leave unused vertical space.
- [ ] Inspector scroll views are vertical-only. At narrow widths, the Execution, Await,
  and Event controls stack without producing horizontal overflow.
- [ ] Tint is a direct-edit swatch that enables Custom Tint on change and immediately
  affects the Block.
- [ ] Behaviour & Timing defaults expanded; Callers defaults collapsed and computes only
  when opened.
- [ ] Existing event settings, help text, command list, command details, selection,
  reordering, nested-action drag/drop, context menu, and keyboard commands work as
  before.
- [ ] Dragging a standalone action over an Invoke Action header, collapsed group,
  single-action row, or expanded child area moves it into that group. The full destination
  displays a visible drop highlight; precise child drops show an insertion line and control
  the resulting insertion position.
- [ ] An empty Invoke Action is labelled `Invoke Action` in the list; adding its first
  action changes the list summary to that action's name, while the detail panel remains
  headed `Invoke Action`.
- [ ] The Unity Hierarchy uses the flow-graph icon; the renamed preference preserves a
  user's previous hide choice.
- [ ] Unity's native `(Block Inspector)` object header uses the flow-graph icon rather
  than `Textures/ScriptIcon.png`.
- [ ] New editor tests pass, analyzer diagnostics are clean, and
  `.agents/scripts/validate-changes.cmd` passes.
- [ ] `Docs/ScaffoldVisualScripting/BlockInspector.md` documents the layout and resource
  regeneration workflow.

## Idempotence and Recovery

- Re-running **Sync with EditorResources folder** is safe; it deterministically rebuilds
  `ScaffoldEditorResourcesGenerated.cs` from asset filenames. Review its diff and never
  hand-edit a generated accessor to work around a failed sync.
- If Unity loses a reference in `ScaffoldEditorResources.asset`, select the asset and run
  **Sync with EditorResources folder** again, then save assets. Confirm the Free and Pro
  fields both point to the new PNGs.
- If the new preference appears to reset user state, restore the fallback read of the old
  key before shipping; do not delete the old-key migration logic until an explicit
  versioned cleanup is approved.
- If visual work exposes an existing command-list regression, revert only the relevant
  layout checkpoint and keep the current command implementation untouched; do not use
  destructive Git commands on the shared dirty worktree.

## Artifacts and Notes

- Visual reference: the user-supplied screenshot from 2026-07-17 22:30:30.
- Plan: `Plans/BlockInspectorLayout/BlockInspectorLayout-ExecPlan.md`.
- New documentation: `Docs/ScaffoldVisualScripting/BlockInspector.md`.
- New icon assets:
  `Assets/3rdParty/ScaffoldVisualScripting/EditorResources/Icons/flow_graph.png` and
  `Assets/3rdParty/ScaffoldVisualScripting/EditorResources/Icons/Pro/flow_graph.png`.

## Interfaces and Dependencies

- `UnityEditor.Editor`, `EditorGUILayout`, `EditorGUI`, `ReorderableList`, and
  `SerializedObject` are existing Unity editor APIs; no new third-party UI dependency is
  needed.
- `ScaffoldEditorResources` is the existing asset-backed texture registry. Its generated
  `FlowGraph` accessor is the only new resource-facing API.
- `Flowchart.GetUniqueBlockKey`, `Flowchart.SelectedCommands`, `Block.CommandList`,
  `CommandListAdaptor`, and `EventHandlerEditor` are existing contracts and must retain
  their current behaviour.
- The new `Scaffold.EditorTests` test assembly references only `Scaffold`,
  `ScaffoldEditor`, and Unity test assemblies, preserving explicit assembly boundaries.
