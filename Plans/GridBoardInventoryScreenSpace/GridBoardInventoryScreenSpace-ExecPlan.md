# Move the Gear workspace to screen space

This ExecPlan is a living document.

## Purpose / Big Picture

The Board, Inventory, Trash zone, gear visuals, and drag preview must be native Unity UI
owned by each campaign screen. Setup and Roguelike expose the complete interactive workspace;
Active Race exposes a read-only Board. Track, cars, and race environment remain world-space.

## Progress

- [x] Isolate tutorial cleanup on `codex/tutorial`.
- [x] Create `codex/grid-board-inventory-screen-space` from `main`.
- [x] Replace world-space drag coordinates and physics target lookup with UI coordinates.
- [x] Convert gear, grid slot, Board, Inventory, and Trash prefabs to canvas-less UI.
- [x] Nest the workspace into Roguelike and Active Race prefabs. Keep Setup inventory in
  its screen prefab and compose the interactive Board, Trash, and drag overlay through a
  sibling `BoardView` instance in Main Scene.
- [x] Remove shared Gear presentation objects and overrides from Main Scene.
- [x] Add automated functional, asset, responsive-layout, and visual verification.
- [x] Run scoped quality gates and prepare the verified refactor milestone.

## Surprises & Discoveries

- The generated Unity solution contains duplicate project display names, so C# lint and
  compilation must target affected `.csproj` files instead of the solution.
- Every generated Gear item points to the existing `BaseGearView` GUID. The prefab must be
  replaced at the same path instead of creating a new base prefab.
- `CoreGearConfig` references a separate `CoreGearView`; referenced-prefab validation exposed
  it and the prefab was migrated without changing its GUID.
- Setup and Roguelike contain pre-existing missing scripts in nested button prefabs. Unity
  cannot save those prefab roots through its API, so their workspace additions were applied
  as targeted serialized prefab changes and validated by asset-loading tests.
- The repository validation wrapper requires PowerShell 7, which is not installed on this
  machine. Its scoped lint, compilation, affected tests, and diff checks were run directly.

## Decision Log

- Use `Image`, `CanvasRenderer`, and `RectTransform` for every Gear workspace visual.
- Keep `BoardLayoutSO` as view configuration, expressed in reference-resolution pixels.
- Inject `IDragService` and the drag overlay into each `Draggable`; do not use the static
  drag-service registry for the new flow.
- Use one canvas-less `PFB_GearWorkspace` nested beneath each owning screen Canvas.
- Apply Safe Area to the workspace root and use the 1080x1920 portrait composition baseline.

## Outcomes & Retrospective

The Gear workspace is now screen-space UI owned by each campaign screen. Setup and Roguelike
bind the complete interactive workspace; Active Race binds only the Board contained in its
read-only workspace. Main Scene no longer owns or injects shared Board, Inventory, or Trash
objects. Track, cars, and the rest of race presentation remain unchanged in world space.

Affected verification passed with 31 EditMode tests and 7 PlayMode tests. Visual tests also
passed at 1080x1920, 1080x2400, and 1080x1680, with every workspace element inside Safe Area.
The generated Unity logs contain no compilation errors or relevant exceptions. Scoped C#
formatting and analyzers pass in fix and check modes for all 28 affected files.

## Context and Orientation

`BoardViewComponent`, `GearInventoryViewComponent`, and `TrashDropZoneViewComponent` are the
three presentation views. `GearView` renders one Gear. `Draggable` builds `DragPayload` and
resolves an `IDragTarget`. Campaign views currently receive shared scene instances through
serialized overrides in `Main Scene`; those references will move into their prefab-owned
`GearWorkspaceView`.

## Plan of Work

1. Introduce screen-coordinate drag payloads, UI-only target discovery, local RectTransform
   board conversion, explicit drag context, and an isolated UI material per Gear.
2. Add `GearWorkspaceView`, responsive Safe Area behavior, and tests for layout and input.
3. Replace UI prefabs at their existing paths, create the reusable workspace prefab, embed it
   in the three campaign prefabs, and remove obsolete Main Scene presentation objects and
   overrides.
4. Validate serialization, compilation, EditMode/PlayMode behavior, and portrait screenshots.

## Concrete Steps

- Run scoped C# lint in `fix` and `check` modes for every changed C# file.
- Run focused EditMode and PlayMode fixtures through the Unity test wrapper.
- Run `.agents/scripts/validate-changes.sh` and inspect all generated reports.

## Validation and Acceptance

- No Gear workspace prefab contains `SpriteRenderer`, Collider, `SortingGroup`, nested Canvas,
  `PhysicsRaycaster`, or `Physics2DRaycaster`.
- Setup and Roguelike own interactive workspace instances; Active Race owns a read-only one.
- Main Scene has no shared Board, Inventory, or Trash presentation objects.
- Dragging between Inventory, Board, and Trash uses UI raycasting and screen coordinates.
- 1080x1920, tall portrait, and short portrait layouts remain inside Safe Area.

## Idempotence and Recovery

The migrated prefabs and scenes are the authoritative serialized state. Existing `.meta` files
and Gear prefab GUIDs remain untouched, so the change can be recovered through Git without
deleting or regenerating Gear item assets.

## Artifacts and Notes

Unity test reports belong under `Artifacts/TestResults/`. Visual captures belong under
`Artifacts/VisualTests/GearWorkspaceScreenSpace/` with evidence sidecars.

## Interfaces and Dependencies

- `DragPayload` carries `Vector2 ScreenPosition`.
- `Draggable.Configure(IDragService, RectTransform)` supplies drag lifecycle and overlay.
- `GearWorkspaceView.BindInteractive(...)` and `BindReadOnly(...)` own subview binding.
- `BoardScreenPositionUtility` converts pointer coordinates into Board-local UI coordinates.
- Existing gameplay models, save data, LiveOps schemas, and Gear item references do not change.
