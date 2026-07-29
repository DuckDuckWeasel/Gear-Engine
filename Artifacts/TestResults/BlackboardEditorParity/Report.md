# Blackboard Editor Parity Regression Report

## Context

Verify the managed Blackboard editor against the legacy-facing contracts identified
from the supplied screenshots and historical `BlockInspector` implementation.

## NUnit Result

- Result file: no result file.
- Passed: unavailable.
- Failed: unavailable.
- Skipped: unavailable.
- Total executed: unavailable.
- The focused fixture was discovered before the latest execution-source and
  Play Mode transition regressions were added.
- The updated editor and Gear Engine regression fixtures compile successfully.

Unity requested Save or Don't Save before switching away from the modified
`Main Scene`. The run was cancelled to preserve the user's unsaved scene, so no
current pass/fail claim is made.

## Compile and Static Gates

- `Scaffold`: zero errors.
- `Scaffold.VisualScripting.Editor`: zero errors.
- `Scaffold.VisualScripting.Editor.Tests`: zero errors.
- `Game.GearEngine`: zero errors.
- `Game.GearEngine.Editor`: zero errors.
- `Game.GearEngine.Tests`: zero errors.
- Live Unity Tundra compilation: passed.
- Generated `Assembly-CSharp-Editor` build: zero warnings and zero errors.
- Generated `Assembly-CSharp` build: zero errors; pre-existing package/project
  warnings remain.
- Scoped C# formatting/style check: passed.
- One-top-level-type structure check: passed for six changed C# files.
- Repository `validate-changes.cmd`: passed.

## Regression Coverage Added

- Production selectors exclude private nested test implementations.
- Fallback display labels remove managed implementation suffixes.
- Authoring zoom clamps to the legacy 0.25-1.0 range.
- The lower action preview resolves only when exactly one action is selected.
- The split preserves minimum main-list and lower-preview heights.
- The action catalog requires legacy `CommandInfo` menu metadata and excludes
  undocumented implementation wrappers.
- Nested managed-property expansion survives temporary `SerializedObject`
  reconstruction.
- Compatibility value pickers filter managed variables by expected value type.
- Unity object pickers exclude incompatible object values.
- Compatibility references read and write the selected managed runtime cell.
- GameObject target references resolve managed Unity object cells.
- Target variable menus exclude scalar and non-GameObject global variables.
- Action preview focus is released when the selected action identity changes,
  preventing an active text editor from being reused by the next field.
- Entered Play Mode and Entered Edit Mode are the lifecycle boundaries that trigger
  an authoring-target rebind.
- Play From Selected resolves the selected action's flattened runtime task index
  across multiple action tracks.
- Runtime feedback resolves action state through the Block's flattened task runner,
  matching the runner that actually executes the actions.
- The active action row receives a dedicated execution marker without marking
  completed or pending rows as current.
- Drag insertion chooses the nearest before/after edge of an action row.
- Same-track downward dragging adjusts the insertion index after removing the source
  action.
- Inline action deletion removes the requested action and clears its selection.
- Side-panel width clamping preserves a dedicated center Blackboard area at narrow
  and wide editor-window widths.
- Action-row presentation uses distinct idle, hovered, selected, and
  selected-hovered tint strengths.
- Action controls remain contextual to the hovered row.
- An unspecified compatibility value source resolves its direct value when the
  retained variable wrapper has no key or definition ID.
- A configured compatibility wrapper continues to resolve its Blackboard variable.
- The editor drawer normalizes unspecified compatibility sources to the same mode
  used by runtime resolution.
- Equal side-panel rectangles keep the Blackboard graph centered at narrow and wide
  window widths.
- The managed-property renderer ignores a stale Unity owner during Play Mode
  transitions instead of constructing an invalid `SerializedObject`.

## Visual Inspection

- The selected action inspector renders below the upper Block/list scroll.
- The trigger selector contains `None`, `Tag Event`, `Bindable`, `Blackboard
  Enabled`, `Blackboard Message`, `Game Started`, and `Polling`.
- `Test Trigger` and `Definition` suffixes are absent.
- `Target` and `Duration` expand and remain expanded after repaint.
- Searching **Add Action** for `Show UI Focus` returns one canonical result.
- The already serialized duplicate action is retained to avoid destructive migration.
- Retained data wrappers render a compact source/value row instead of exposing
  raw Scope, Key, and Value internals.
- The source selector exposes Direct Value, Blackboard Variable, and
  Scriptable Object modes.
- `Block During Execution` is shown only for Utility Selector tracks.
- Derived `Indent Level` metadata is hidden.
- Editing `Wait.Duration`, then selecting `Debug Log`, leaves the log-message
  field empty; the duration text is no longer copied into the next inspector.
- Entering Play Mode keeps the Block graph and detail panel correctly bound without
  reopening the Blackboard window.
- Returning to Edit Mode restores the same correctly bound editor state.
- Block identity, execution, trigger, and the Action Tracks header remain fixed while
  only the track/action list scrolls vertically.
- The authoring pane no longer renders a horizontal scrollbar.
- The editor window and side panes use wider minimum widths.
- Action rows hide Move Up, Move Down, and Delete while idle.
- Hovering an action adds a subtle highlight and exposes its three controls without
  exposing controls on neighboring rows.
- The play menu exposes Play From Start and Play From Selected; unavailable runtime
  operations remain disabled.
- Block and Variables authoring render only in the left pane.
- The Blackboard graph renders independently in the center pane.
- The selected action inspector renders independently in the right pane.
- The three-pane layout remains correctly bound in Play Mode.
- The Game Started block executes all four enabled actions.
- The final Debug Log action writes `Wait 5 seconds` to the Unity Console.
- While the block is executing, the current action has a cyan pulse, a solid cyan
  left rail, and a play marker; the marker advances with execution.
- Dragging an action renders a labeled ghost card at the pointer and a cyan insertion
  line at the selected before/after edge.
- The center Blackboard pane remains between equal-width authoring and inspector
  panes at the current narrow dock width.
- A repeated Play Mode to Edit Mode transition completes with zero Console errors.
- The dirty Main Scene was preserved.

## Artifacts

- `../../VisualTests/BlackboardEditorParity/ActionPreviewBelowList.jpeg`
- `../../VisualTests/BlackboardEditorParity/ActionPreviewBelowList.evidence.json`
- `../../VisualTests/BlackboardEditorParity/TriggerSelectorFiltered.jpeg`
- `../../VisualTests/BlackboardEditorParity/TriggerSelectorFiltered.evidence.json`
- `../../VisualTests/BlackboardEditorParity/TargetExpanded.jpeg`
- `../../VisualTests/BlackboardEditorParity/TargetExpanded.evidence.json`
- `../../VisualTests/BlackboardEditorParity/DurationExpanded.jpeg`
- `../../VisualTests/BlackboardEditorParity/DurationExpanded.evidence.json`
- `../../VisualTests/BlackboardEditorParity/CanonicalUIFocusMenu.jpeg`
- `../../VisualTests/BlackboardEditorParity/CanonicalUIFocusMenu.evidence.json`
- `../../VisualTests/BlackboardEditorParity/CompatibilityVariableDrawer.png`
- `../../VisualTests/BlackboardEditorParity/CompatibilityVariableDrawer.evidence.json`
- `../../VisualTests/BlackboardEditorParity/ActionTextFieldIsolation.png`
- `../../VisualTests/BlackboardEditorParity/PlayModeStableWithActionDelete.png`
- `../../VisualTests/BlackboardEditorParity/PlayModeStableWithActionDelete.evidence.json`
- `../../VisualTests/BlackboardEditorParity/EditModeRestoredAfterPlay.png`
- `../../VisualTests/BlackboardEditorParity/EditModeRestoredAfterPlay.evidence.json`
- `../../VisualTests/BlackboardEditorParity/ThreePaneEditorLayout.png`
- `../../VisualTests/BlackboardEditorParity/ThreePaneEditorLayout.evidence.json`
- `../../VisualTests/BlackboardEditorParity/ThreePanePlayModeLayout.png`
- `../../VisualTests/BlackboardEditorParity/ThreePanePlayModeLayout.evidence.json`
- `../../VisualTests/BlackboardEditorParity/FixedHeaderActionsScroll.jpg`
- `../../VisualTests/BlackboardEditorParity/FixedHeaderActionsScroll.evidence.json`
- `../../VisualTests/BlackboardEditorParity/ActionRowHoverControls.jpg`
- `../../VisualTests/BlackboardEditorParity/ActionRowHoverControls.evidence.json`
- `../../VisualTests/BlackboardEditorParity/BlackboardExecutionAndCenteredLayout.png`
- `../../VisualTests/BlackboardEditorParity/BlackboardExecutionAndCenteredLayout.evidence.json`
- `../../VisualTests/BlackboardEditorParity/RunningActionIndicator.jpg`
- `../../VisualTests/BlackboardEditorParity/RunningActionIndicator.evidence.json`
