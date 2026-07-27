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
- `ExpandedActionListItem_IncludesVisibleActionPropertyHeight` asserts that an expanded
  action row reserves space for its visible action properties.
- `ActionChildPropertyCheck_ExcludesTheNextActionInTheList` asserts that one action's
  iterator cannot include the next action as a child field.

## Verification

- Unity's own Roslyn response files compile successfully for `ScaffoldEditor`,
  `Game.GearEngine.Editor`, and `Game.GearEngine.Tests` after removing the invalid direct
  editor-type reference from the regression test.
- Scoped `dotnet build` completed for the same three generated projects.
- Scoped C# formatting and style checks passed for the three changed C# files.
- The active project-local Unity log exposed the stale `CS0246` test compilation error
  that prevented the corrected inspector assembly from reloading; the exact Unity compiler
  invocation now completes with zero errors.
- Unity batch-mode Editor tests were not run because the active project lock prevents a
  second Editor instance from opening the project.
