# Invoke Action Reorder Handle Layout Fix

## Icon Layout Issue

The foldout for an Action Invoker row was drawn at the left edge of a Unity
`ReorderableList` element. Unity uses that edge for its reorder handle, causing the
two directional icons to overlap.

## Icon Layout Fix

The Action Invoker editor now reserves an 18-pixel reorder-handle gutter before
drawing the foldout and action name. `InvokeActionEditorUtility.GetActionRowContentRect`
owns the reusable row geometry so trailing controls and the left handle remain disjoint.

## Missing Action Properties

The custom Action Invoker header previously replaced Unity's managed-reference
`PropertyField` with manual child traversal. That traversal could allocate the child
height without drawing the underlying serialized fields, leaving expanded actions blank.

## Property Rendering Fix

Expanded actions now keep the custom header and render only the visible serialized child
properties within the row bounds. Child traversal is constrained to the current action's
serialized property path, so it cannot spill into the next action or beyond the list.

## Expanded Row Header Alignment

The row content rectangle inherited the full height of an expanded `ReorderableList`
element. This vertically centered the foldout and action name, then positioned the child
properties after the entire row, making them overlap the next action. The content helper
now accepts an explicit height, and Action Invoker headers always request exactly one
Inspector line. The left foldout and name are therefore anchored to the same top row as
the right-side weight and enabled controls.

## Explicit Header Restoration

The populated Action Invoker row owns its complete IMGUI header again. The renderer draws
the foldout, the same action display name used by the compact command list, the validation
badge, the enabled toggle, and the existing weight controls while preserving the
reorder-handle gutter. Expanded action properties begin below that header, and a collapsed
action occupies one row.

Selecting a nested action also changes the detailed Inspector title to the selected
action's display name. When no nested action is selected, the container retains the
`Action Invoker` title.

## Nested Expansion State

Expansion now belongs consistently to the managed-reference property at
`actions[index].action`. Adding an action or selecting a different action expands that
nested property once. Repeated selection synchronization does not reopen an action that
the user manually collapsed.

## Unity Assembly Reload Fix

The initial regression test referenced `InvokeActionCommandEditor` directly. The generated
IDE project allowed that reference, but Unity's asmdef graph does not expose the editor
assembly to the test assembly. Unity therefore kept the previously compiled inspector and
reported `CS0246` in its project-local `Logs/Editor.log`. The test now discovers the custom
editor through `UnityEditor.Editor.CreateEditor` and reflects on the returned runtime type,
preserving coverage without crossing the asmdef boundary.

## Regression Coverage

- `GetActionRowContentRect_ReservesTheReorderHandleBeforeTheFoldout` asserts that the
  action header begins after the handle gutter, ends before trailing controls, remains at
  the row top, and uses a single-line height even when the expanded element is taller.
- `ActionInvokerInspector_SeparatesActionPropertiesFromTheHeader` asserts that the
  dedicated action-header renderer remains part of the custom editor.
- `ExpandedActionListItem_IncludesVisibleActionPropertyHeight` asserts that an expanded
  action row reserves space for its visible action properties.
- `ActionInvokerListItem_CollapsesToItsHeaderHeight` asserts that a collapsed nested action
  occupies exactly one Inspector line.
- `ActionChildPropertyCheck_ExcludesTheNextActionInTheList` asserts that one action's
  iterator cannot include the next action as a child field.
- `SelectedActionListItem_ExpandsNestedActionOnFirstSynchronization` and
  `AddedActionListItem_ExpandsNestedAction` assert that selection and creation expand the
  managed-reference property rather than its wrapper.
- `SelectedActionListItem_RemainsCollapsedAfterSelectionSynchronization` asserts that
  repeated synchronization preserves a manual collapse.
- The standalone and selected nested title tests compare the Inspector title directly
  with `InvokeActionEditorUtility.GetDisplayName`, guaranteeing parity with the main list.

## Verification

- Focused EditMode fixture
  `GearEngine.GearEngine.Tests.Editor.InvokeActionEditorSelectionTests`: 45 passed,
  0 failed, 0 skipped, and 0 inconclusive.
- NUnit XML, Editor log, and the contextual report are under
  `Artifacts/TestResults/20260727-083508/`.
- Scoped C# lint completed in `fix` and `check` modes for
  `InvokeActionCommandEditor.cs` and `InvokeActionEditorSelectionTests.cs`; formatting,
  style, and analyzer checks are clean.
- Both changed C# files pass one-top-level-type source-structure validation.
- Unity 6000.5.3f1 imported the final source, completed script compilation in 9 seconds,
  and reloaded assemblies without C# errors or warnings.
- The repository validation wrapper was invoked but could not run because PowerShell 7
  (`pwsh`) is not installed. The scoped lint, structure, Unity compilation, focused tests,
  and static repository checks provide the available equivalent evidence.
- The worktree was registered and opened in Unity Hub. A final Inspector screenshot could
  not be captured because macOS automation continued routing the older main-checkout Unity
  window instead of the already-open worktree instance.
