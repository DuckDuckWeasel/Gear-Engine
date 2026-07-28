# Scaffold Visual Scripting

## Editor workflow

Open `Window > Scaffold > Blackboard`, or use **Open Blackboard Editor** from a
`BlackboardBehaviour` or `BlackboardDefinitionAsset` Inspector.

The Blackboard editor uses the original Scaffold visual language: a grid canvas,
colored Block nodes, relationship lines, graph search, pan/zoom, multi-selection,
rectangle selection, context menus, and Unity-standard editing shortcuts. Selecting
a Block opens the managed detail pane for triggers, execution settings, tracks,
actions, grouping, values, and variables.

The editor supports Direct definitions, reusable definition assets, and nested
Blackboard-definition variables. Use the editor's Back control after opening a nested
source.

## Architecture

The visual workflow is intentionally separate from runtime ownership:

```text
BlackboardDefinitionAsset / BlackboardBehaviour
    -> BlackboardDefinition + BlackboardAuthoringMetadata
    -> BlackboardAuthoringController
    -> plain Blackboard runtime during Play Mode
```

Definitions and stable IDs are reusable data. Authoring metadata stores graph
positions, tint, selection, viewport, and presentation groups. The controller is the
single Undo-aware mutation boundary. The canvas and detail pane render and route
input; they do not own graph data or create legacy Blackboard, Block, Command,
EventHandler, or Variable components.

VContainer continues to construct runtime services. Editor execution controls target
the already-running plain runtime exposed by the selected `BlackboardBehaviour`;
they do not create a second execution path.

## Play Mode

For a live `BlackboardBehaviour`, the toolbar and Block/action menus can execute the
selected Block, execute from a selected action, stop the selected Block, or stop all.
Outside Play Mode, for an asset-only target, or before runtime initialization, these
commands are disabled and the editor explains why.

Runtime feedback is observational. It never serializes execution state into the
definition or authoring metadata.

## Legacy status

The editor experience has been restored on top of managed definitions. The retired
component-owned graph and its serialized component GUIDs remain unsupported. Existing
pre-cutover component graphs require deliberate reconstruction or a separately scoped
migration tool.

The reference-document exporter, legacy Variable-component generator, and removed
Save Menu editor are intentionally outside this restoration.

See [BlackboardRuntime.md](BlackboardRuntime.md) for runtime architecture and
[BlockInspector.md](BlockInspector.md) for detailed authoring controls.
