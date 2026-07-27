# Block Inspector layout

The Block Inspector organizes a selected Block into four visual areas:

1. **Identity card** — flow-graph icon, unique Block Name, direct Tint swatch, and
   Description.
2. **Execution summary** — compact Execution, Await, and Event headers above their
   value-only controls. It uses three columns when space permits and stacks the controls
   vertically in a narrow Inspector.
3. **Behaviour & Timing** — expanded by default; contains auto-selection and selected
   event-handler settings with their help text.
4. **Callers** — collapsed by default; computes its read-only caller list only when
   expanded.

The Commands list and selected command editor retain their current interactions. A new
Invoke Action initially displays `Invoke Action`; after its first action is added, the
list item uses that action's type name as its visual summary. The detail editor remains
headed `Invoke Action` so the enclosing collection is clear.

Dragging a standalone action into an Invoke Action accepts the drop over the full destination
group, including its header, a collapsed group, and a single-action row. The active destination
uses a blue outline and the `Drop action into Invoke Action` label. Dragging a nested action to a
specific child still inserts at that child position; dragging a standalone action over the upper
or lower half of a child shows an insertion line and uses that exact index. Dropping elsewhere in
a destination group appends it.

## Description and tint

Description starts as a single line. It grows with wrapped content up to four lines and
then uses a vertical scroll view. This keeps empty blocks compact without hiding longer
documentation.

All Inspector scroll views are vertical-only. The block and command layouts avoid fixed
minimum widths so a narrow Inspector reflows instead of exposing a horizontal scrollbar.

The identity card always shows the tint swatch. Editing it sets the serialized tint and
enables `useCustomTint`; there is no separate Custom Tint toggle in the Inspector.

## Shared IMGUI stylesheet

`Scripts/Editor/BlockInspectorStyleSheet.cs` is the shared IMGUI stylesheet for the
Block Inspector. It owns cached styles and the spacing/Description height constants used
by the identity card, section cards, summary columns, and Description text area. Use it
for future Block Inspector visual work instead of adding inline styles or spacing values.

## Flow-graph icon

`EditorResources/Icons/flow_graph.png` is the light-skin icon and
`EditorResources/Icons/Pro/flow_graph.png` is its dark-skin counterpart. Both are
registered through `ScaffoldEditorResources.FlowGraph` and are used by the Block
Inspector and the Unity Hierarchy.

`Scripts/Editor/BlockInspector.cs.meta` separately assigns the Free flow-graph texture
to Unity's native `(Block Inspector)` object header. It is not controlled by
`ScaffoldEditorResources`.

After adding or renaming an editor icon, select
`EditorResources/ScaffoldEditorResources.asset` and choose **Sync with EditorResources
folder**. Review and commit the resulting
`Scripts/Editor/ScaffoldEditorResourcesGenerated.cs` change together with the asset and
its `.meta` files.
