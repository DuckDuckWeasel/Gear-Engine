# Blackboard graph editor

The Blackboard window restores the graph-first Scaffold authoring workflow while
editing managed definitions rather than component-owned nodes.

## Open and navigate

Open **Window > Scaffold > Blackboard**, use **Open Blackboard Window** on a
`BlackboardBehaviour` or `BlackboardDefinitionAsset`, or click the flow-graph icon in
the Hierarchy. Direct, asset-backed, and nested Blackboard-variable sources all open
in the same window. Nested sources expose **Back** to return to their source
Blackboard.

The canvas uses the established Scaffold grid and node textures. Pan with the middle
mouse button or Alt-drag, zoom around the pointer with the scroll wheel, and use
**Frame** or **Layout** from the toolbar. Search matches Block names and action
summaries; **Focus** centers the first result.

## Selection and graph commands

Click a Block to select it, use Shift/Cmd/Ctrl for additive selection, or drag on
empty canvas space to box-select. Dragging a selected Block moves the complete
selection as one Undo operation.

The canvas supports Add, Copy, Cut, Paste, Duplicate, Delete, Select All, and Frame
All through its context menu and Unity's standard keyboard commands. Copied managed
graphs regenerate every definition ID while preserving explicit Unity object
references.

Call, Menu, and Menu Timer actions expose their current Block relationships as lines
on the canvas. Missing targets remain unresolved instead of creating guessed links.

## Block inspector

The right pane edits the selected Block:

- name, authoring description, and optional tint;
- execution, await, order, and repeat settings;
- categorized searchable trigger selection and trigger properties;
- named action tracks and their execution settings;
- readable action names, summaries, Enabled state, Utility/Weight options, and
  serialized managed properties;
- action reorder, cross-track drag/drop, grouping, duplication, deletion, range
  selection, and interruption targets.

The Block list and the selected-action inspector use separate vertical regions. The
upper region owns the Block, trigger, track, and action-list scroll. Selecting exactly
one action shows its help and editable properties in the lower preview region, so
expanding action data no longer shifts or hides the main list. Drag the horizontal
divider to allocate more space to either region.

Type selectors expose only public, constructible managed definitions. Actions also
require `CommandInfo` menu metadata, matching the legacy command selector and keeping
internal `IAction` wrappers out of **Add Action**. Existing serialized wrappers are
preserved for compatibility, but they cannot be added again through the picker.
Fallback labels remove implementation suffixes such as `TriggerDefinition`,
`ActionDefinition`, and `VariableDefinition`, and the trigger selector includes an
explicit **None** option.

Nested managed properties retain their disclosure state when the detail pane repaints
or recreates its temporary `SerializedObject`. Foldouts such as **Target** and
**Duration** therefore remain expanded while their child fields are edited.

The **Variables** tab creates, duplicates, sorts, reorders, and deletes managed
variable definitions while exposing their initial values. Stable
`VariableReference` fields use a scope-aware managed-variable picker. Block-targeting
actions use a Block popup instead of a raw name field.

Retained value-reference fields use one compact source/value row instead of exposing
their internal Scope, Key, and Value fields. Blackboard Variable mode lists only
compatible managed definitions and persists the stable definition ID. Direct
`VariableProperty` outputs and `AnyVariableAndDataPair` use the same picker; the pair
shows only the value field matching the selected variable type. GameObject target
menus list only injected-global Unity object variables currently configured with a
GameObject.

`Block During Execution` is an Utility Selector option and is hidden for other
execution methods. `Indent Level` remains derived structural metadata and is not
designer-facing.

## Play Mode

For a live `BlackboardBehaviour`, the toolbar and context menu expose Execute, Stop,
Stop All, and Execute From Here. These controls call the existing plain runtime by
stable definition ID and are disabled for assets, Edit Mode, or unavailable runtimes.
Node and action status is observational and never writes runtime state into the
definition.

## Architecture

`BlackboardAuthoringController` is the only mutation boundary. It records Undo,
marks the asset or component dirty, and maintains selection/group metadata.
`BlackboardAuthoringMetadata` owns graph positions, description, tint, zoom, scroll,
and selection; none of this state is cloned into the runtime.

The editor reuses `ScaffoldEditorResources` for familiar visuals, but it does not
restore legacy Blackboard, Block, Command, EventHandler, or Variable components.
