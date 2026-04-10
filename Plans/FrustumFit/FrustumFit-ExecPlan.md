# FrustumFit: Camera-frustum-relative responsive scaling for world-space objects

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

Repository policy for ExecPlans is defined in `PLANS.md` at the repository root. This document must be maintained in accordance with that file.

---

## Purpose / Big Picture

After this work, a developer can drop a single `FrustumFit` component onto any world-space `GameObject` that has a `Renderer`, configure **fill percentages** (what fraction of the camera frustum the object should occupy) and a **fill mode** (how it fills that target box), and the component will calculate and apply the correct `localScale` so the object appears at exactly the intended size relative to the visible screen area.

The system is **deliberately stateless with respect to resize detection**: `FrustumFit` exposes a public `Apply()` method and optional `_applyEveryFrame` flag so it can be triggered by any external coordinator, a screen-resize event, or just on demand. Automatic resize detection and event-bus coordination are explicitly deferred to a future milestone (see Decision Log).

A developer can see it working when: a world-space Quad with `FrustumFit` set to `fillX = 1, fillY = 1, mode = Fit` fills the visible camera area exactly at its configured depth, and the same component set to `fillX = 0.5, fillY = 0.5` fills exactly a quarter of the screen area, regardless of the camera's field of view, aspect ratio, or whether the projection is perspective or orthographic. Adding the same component to a second object with different fill settings scales that second object independently.

Running `powershell -NoProfile -ExecutionPolicy Bypass -File ".\.agents\scripts\validate-changes.ps1" -SkipTests` from the repository root completes with a clean gate, and EditMode tests for the math layer pass.

---

## Progress

- [ ] Author ExecPlan at `Plans/FrustumFit/FrustumFit-ExecPlan.md`.
- [ ] Milestone 1 — Core math types: `FrustumFillMode`, `FrustumFitAxes`, `FrustumBounds`, `FrustumFitMath`.
- [ ] Milestone 2 — `FrustumFit` MonoBehaviour.
- [ ] Milestone 3 — EditMode tests for `FrustumFitMath`.
- [ ] Milestone 4 — Documentation (`Docs/FrustumFit.md`).

---

## Surprises & Discoveries

*(Fill in as implementation progresses.)*

---

## Decision Log

- **Decision: Resize detection is out of scope for this plan.**
  Rationale: The stakeholder explicitly stated resize-event integration ("implement in the future ways to update it") is deferred. `FrustumFit.Apply()` is public and idempotent so a future `ScreenResizeWatcher` + coordinator can call it without touching this component.
  Author: Initial ExecPlan draft.

- **Decision: Expose a `FrustumFitAxes` selector (`XY`, `XZ`, `YZ`) rather than auto-detecting mesh orientation.**
  Rationale: Unity's default Plane primitive is a 10×10 flat mesh on the XZ plane; after rotating it to face a -Z camera, the axes that map to screen horizontal/vertical are X and Z, not X and Y. Auto-detection (picking the two largest `localBounds.size` components) is fragile across edge cases (cube, sphere). An explicit Inspector selector is predictable and self-documenting. Default is `XY` (covers Quad, custom facing meshes, SpriteRenderer).
  Author: Initial ExecPlan draft.

- **Decision: Extract math into a Unity-free static class `FrustumFitMath`.**
  Rationale: `FrustumFit` is a `MonoBehaviour` whose lifecycle is difficult to drive in EditMode unit tests. Extracting the pure calculation (given frustum params, mesh size, fill settings → output scale Vector2) into a static helper class allows full test coverage without needing a camera or renderer. The MonoBehaviour becomes a thin host that reads Unity state and delegates to `FrustumFitMath`.
  Author: Initial ExecPlan draft.

- **Decision: Place all types in `Game.GearEngine` assembly under `Assets/Scripts/Game/GearEngine/Presentation/World/`.**
  Rationale: This is a presentation-layer concern. The `Game.GearEngine` assembly already has `UnityEngine` access and is the correct home for world-space presentation logic in this project. A future refactor could promote it to a shared scaffold package if needed.
  Author: Initial ExecPlan draft.

- **Decision: `fillX` and `fillY` are clamped to `[0, 2]` in the Inspector (not `[0, 1]`).**
  Rationale: Values greater than 1 are valid for intentional screen-bleed effects (e.g. a background plane that extends 10% past every edge). The math handles any positive value.
  Author: Initial ExecPlan draft.

- **Decision: Z localScale is never modified by `FrustumFit`.**
  Rationale: World-space objects that need depth (3D geometry, colliders) should own their own Z scale. `FrustumFit` only manages the two screen-facing axes selected by `FrustumFitAxes`.
  Author: Initial ExecPlan draft.

---

## Outcomes & Retrospective

*(Fill in at completion.)*

---

## Context and Orientation

### The problem

Unity's Canvas `RectTransform` automatically adapts UI element size and position to screen dimensions via anchors and the Canvas Scaler. World-space GameObjects have no equivalent. Their `localScale` is in world units; if the camera field of view changes or the window is resized, the objects no longer fill the intended proportion of the screen.

### Why `Camera.ViewportToWorldPoint` alone is not enough

Viewport-to-world gives you the world-space position of a screen point at a given depth, but it tells you nothing about how large a specific mesh already is or how to scale it. You also need the mesh's natural size (from `Renderer.localBounds`) to compute the correct scale multiplier.

### Unity mesh axis conventions

A Unity **Quad** (the primitive intended for 2D-facing use): localBounds ≈ (1, 1, 0). Screen horizontal = local X, screen vertical = local Y → use `FrustumFitAxes.XY`.

A Unity **Plane** (10×10, flat on XZ): localBounds ≈ (10, 0, 10). After rotating the GameObject 90° on X to face the camera, screen horizontal = local X, screen vertical = local Z → use `FrustumFitAxes.XZ`.

A **custom mesh** designed to face the camera with Y-up, Z-forward: depends on the artist's convention. The `FrustumFitAxes` selector handles all cases explicitly.

### Term: Frustum bounds

The visible width and height of the camera's view frustum **at a given depth** (world units):

    // Perspective
    frustumHeight = 2 * tan(fov / 2) * depth
    frustumWidth  = frustumHeight * aspect

    // Orthographic
    frustumHeight = orthographicSize * 2
    frustumWidth  = frustumHeight * aspect

Note: orthographic frustum dimensions are independent of depth.

### Term: Fill mode

How the object is scaled to match a target box (width = `frustumWidth * fillX`, height = `frustumHeight * fillY`):

| Mode        | Behavior                                                       | Analogy       |
|-------------|----------------------------------------------------------------|---------------|
| `Stretch`   | Each axis scales independently; aspect ratio not preserved     | CSS stretch   |
| `Fit`       | Uniform scale; fits inside the box with potential empty space  | CSS contain   |
| `Fill`      | Uniform scale; covers the box, potentially exceeding one axis  | CSS cover     |
| `FillWidth` | Matches target width exactly; height follows proportionally    | —             |
| `FillHeight`| Matches target height exactly; width follows proportionally    | —             |

### Term: Parent lossyScale correction

Unity's `transform.localScale` is relative to the parent. If a parent has `lossyScale.x = 2`, setting `localScale.x = 5` results in a world scale of 10, not 5. To achieve a desired **world scale** `W` on axis X:

    localScale.x = W / parent.lossyScale.x

`FrustumFitMath` receives the parent lossyScale as a parameter so the MonoBehaviour stays testable (tests pass `Vector2.one` as the parent lossyScale).

### Existing codebase reference

World-space grid positioning: `Assets/Scripts/Game/GearEngine/Config/BoardConfigSO.cs` — uses world units calculated from a `Spacing` value. `FrustumFit` is independent of `BoardConfigSO`; it scales objects against the camera frustum, not the grid.

Presentation layer: `Assets/Scripts/Game/GearEngine/Presentation/UI/` — existing UI views. World-space presentation lives in the sibling folder `Presentation/World/` (to be created).

Assembly: `Assets/Scripts/Game/GearEngine/Game.GearEngine.asmdef` — references `UnityEngine` (no `noEngineReferences` restriction). All new types go in namespace `Game.GearEngine.Presentation.World` inside this assembly.

---

## Plan of Work

### Milestone 1 — Core math types

Create the following pure C# types (no MonoBehaviour, no `UnityEngine.Object` subclass):

**`FrustumFillMode.cs`** — enum with values: `Stretch`, `Fit`, `Fill`, `FillWidth`, `FillHeight`.

**`FrustumFitAxes.cs`** — enum with values: `XY` (default), `XZ`, `YZ`. Determines which two local axes are treated as screen-horizontal and screen-vertical.

**`FrustumBounds.cs`** — readonly struct:

    public readonly struct FrustumBounds
    {
        public readonly float Width;
        public readonly float Height;
        // Constructor and FromCamera factory (see Concrete Steps)
    }

`FromCamera` is a static factory that computes frustum bounds from camera parameters passed as primitive values (not a `Camera` reference) so it is testable without a Unity camera object.

**`FrustumFitMath.cs`** — static class:

    public static class FrustumFitMath
    {
        public static FrustumBounds ComputeBounds(bool isOrthographic, float orthographicSize,
            float fieldOfViewDegrees, float aspect, float depth)

        public static Vector2 ComputeLocalScale(FrustumBounds bounds, float fillX, float fillY,
            FrustumFillMode mode, Vector2 meshSize, Vector2 parentLossyScale)
    }

`ComputeLocalScale` returns the `(localScaleX, localScaleY)` pair the MonoBehaviour should apply to the two selected axes. It does **not** modify Z.

Guards: if `meshSize.x <= 0` or `meshSize.y <= 0`, throw `ArgumentException`. If `parentLossyScale.x ≈ 0` or `parentLossyScale.y ≈ 0`, throw `ArgumentException` (guard clause, not silent swallow).

### Milestone 2 — `FrustumFit` MonoBehaviour

Create `FrustumFit.cs`. It is the thin Unity host for `FrustumFitMath`.

**Key serialized fields:**

    [SerializeField] Camera          _camera;
    [SerializeField] float           _depth       = 10f;
    [SerializeField, Range(0f, 2f)]  float _fillX = 1f;
    [SerializeField, Range(0f, 2f)]  float _fillY = 1f;
    [SerializeField] FrustumFillMode _mode        = FrustumFillMode.Fit;
    [SerializeField] FrustumFitAxes  _axes        = FrustumFitAxes.XY;
    [SerializeField] bool            _applyOnStart    = true;
    [SerializeField] bool            _applyEveryFrame = false;

**Lifecycle:**

- `Awake`: cache `Renderer` component. If `_camera` is null, assign `Camera.main`. Guard: if renderer is null log error and return.
- `Start`: if `_applyOnStart`, call `Apply()`.
- `LateUpdate`: if `_applyEveryFrame`, call `Apply()`.

**`Apply()` method (public)**:
1. Guard: log error and return early if `_camera` or cached renderer is null.
2. Call `FrustumFitMath.ComputeBounds` passing `_camera.orthographic`, `_camera.orthographicSize`, `_camera.fieldOfView`, `_camera.aspect`, `_depth`.
3. Read `renderer.localBounds.size`. Extract the two mesh dimensions that correspond to `_axes` (X→size.x, Y→size.y, Z→size.z), call them `meshPrimary` and `meshSecondary`.
4. Compute `parentLossyScale` from `transform.parent?.lossyScale ?? Vector3.one`. Extract the two components matching `_axes`.
5. Call `FrustumFitMath.ComputeLocalScale` with bounds, `_fillX`, `_fillY`, `_mode`, mesh dimensions, parent lossyScale.
6. Apply the returned `Vector2` to the correct two components of `transform.localScale`, leaving the third component unchanged.
7. Wrap in try/catch; log via `Debug.LogError` on any exception — never swallow silently (per project error-handling standard).

`[RequireComponent(typeof(Renderer))]` is NOT used because the project may attach `FrustumFit` to a parent that does not itself have a `Renderer` (e.g. a root transform containing a child mesh). Instead, validate at runtime in `Awake` and surface a clear error.

### Milestone 3 — EditMode tests

Create `Assets/Scripts/Game/GearEngine/Tests/Editor/FrustumFitMathTests.cs` inside the existing `Game.GearEngine.Tests` EditMode test assembly (`Assets/Scripts/Game/GearEngine/Tests/Editor/Game.GearEngine.Tests.asmdef`).

Test cases to cover (using NUnit `Assert`):

1. **Perspective frustum bounds**: known FOV + aspect + depth → expected width/height (tolerance ±0.001f).
2. **Orthographic frustum bounds**: known `orthographicSize` + aspect → expected width/height; depth value must have no effect.
3. **Stretch mode**: `fillX = 0.5, fillY = 1.0` on a 1×1 mesh with identity parent scale → output X = 0.5 × frustumWidth, output Y = 1.0 × frustumHeight.
4. **Fit mode**: `fillX = 1, fillY = 1` on a 2×1 mesh → output is uniform, equal to the smaller of the two raw scale factors.
5. **Fill mode**: `fillX = 1, fillY = 1` on a 2×1 mesh → output is uniform, equal to the larger of the two raw scale factors.
6. **FillWidth mode**: output X matches raw width scale, output Y equals output X.
7. **FillHeight mode**: output Y matches raw height scale, output X equals output Y.
8. **Parent lossyScale correction**: parent lossyScale (2, 3) → output localScale is halved on X and one-third on Y relative to the non-parent case.
9. **Zero mesh size guard**: `meshSize = (0, 1)` → `ArgumentException` thrown.
10. **Zero parent lossyScale guard**: `parentLossyScale = (0, 1)` → `ArgumentException` thrown.

### Milestone 4 — Documentation

Create `Docs/FrustumFit.md`. Cover:

- Purpose and use case (one paragraph).
- Quick-start: add `FrustumFit` to a world-space Quad, assign the camera, set `fillX/fillY`, choose a mode.
- Fill mode table (same as the one in Context and Orientation).
- `FrustumFitAxes` explanation with the Quad vs Plane example.
- `_depth` guidance: set to the object's Z distance from the camera.
- When to use `_applyEveryFrame = true` vs calling `Apply()` externally.
- Future: note the `ScreenResizeWatcher` + coordinator pattern as the intended upgrade path.

---

## Concrete Steps

For each milestone:

1. Implement the milestone scope.
2. Run the validate gate:

        powershell -NoProfile -ExecutionPolicy Bypass -File ".\.agents\scripts\validate-changes.ps1" -SkipTests

3. If the gate fails, fix all reported failures and re-run.
4. Commit the milestone changes.

For Milestone 3 (tests), also run:

    powershell -NoProfile -ExecutionPolicy Bypass -File ".\.agents\scripts\run-editmode-tests.ps1"

Confirm all `FrustumFitMathTests` pass before committing.

---

## Validation and Acceptance

- **Math correctness**: all 10 `FrustumFitMathTests` pass.
- **Manual Unity check**: place a world-space Quad in the scene, attach `FrustumFit`, set `fillX = 1, fillY = 1, mode = Fit`, enter Play mode. The quad should exactly fill the camera view at the configured depth. Set `fillX = 0.5` — the quad should occupy half the width with empty space on both sides (Fit mode). Set `mode = Stretch` — the quad should be half-width, full-height.
- **Independent objects**: two objects with different `fillX/fillY/mode` settings must each scale to their own configured proportion without interfering.
- **Parent hierarchy**: place the `FrustumFit` object under a parent with `localScale = (2, 2, 1)`. `Apply()` must still produce the correct visual screen fill.
- **Orthographic camera**: repeat the manual check with an orthographic camera; changing depth must have no effect on the computed scale.
- **Quality gate**: `validate-changes.ps1 -SkipTests` exits clean.

---

## Idempotence and Recovery

Calling `Apply()` multiple times with no state changes must produce identical `localScale` output each time (pure function of camera + renderer state). There are no side effects to clear.

If `_camera` is reassigned between calls (e.g. camera swap on scene load), the next `Apply()` call uses the new camera automatically because `_camera` is read fresh each call, not cached beyond `Awake`.

If the attached `Renderer` is destroyed at runtime, `Apply()` will encounter a null renderer — this is treated as a configuration error: `Debug.LogError` and early return (the component should be disabled or destroyed alongside its renderer).

---

## Artifacts and Notes

File paths produced by this plan:

    Assets/Scripts/Game/GearEngine/Presentation/World/FrustumFillMode.cs
    Assets/Scripts/Game/GearEngine/Presentation/World/FrustumFitAxes.cs
    Assets/Scripts/Game/GearEngine/Presentation/World/FrustumBounds.cs
    Assets/Scripts/Game/GearEngine/Presentation/World/FrustumFitMath.cs
    Assets/Scripts/Game/GearEngine/Presentation/World/FrustumFit.cs
    Assets/Scripts/Game/GearEngine/Tests/Editor/FrustumFitMathTests.cs
    Docs/FrustumFit.md

No new `.asmdef` or `package.json` files are needed. All production code compiles under the existing `Game.GearEngine` assembly. Tests compile under the existing `Game.GearEngine.Tests` EditMode assembly.

Pseudocode — typical usage in a scene:

    // On a world-space Quad at Z = 10 from the camera:
    // FrustumFit component in Inspector:
    //   _camera     = Main Camera
    //   _depth      = 10
    //   _fillX      = 0.8
    //   _fillY      = 0.8
    //   _mode       = Fit
    //   _axes       = XY
    //   _applyOnStart    = true
    //   _applyEveryFrame = false
    //
    // Result: Quad fills 80% of the screen's smaller dimension,
    // centered, with empty space on two sides. Identical math
    // whether running at 1920x1080, 2560x1440, or any aspect ratio.

---

## Interfaces and Dependencies

**New types after this plan (all in `Game.GearEngine.Presentation.World`):**

- `FrustumFillMode` — enum.
- `FrustumFitAxes` — enum.
- `FrustumBounds` — readonly struct. Static factory `FromCamera(bool, float, float, float, float)`.
- `FrustumFitMath` — static class. `ComputeBounds(...)` and `ComputeLocalScale(...)`.
- `FrustumFit` — `MonoBehaviour`. Public `Apply()`. Serialized Inspector fields listed above.

**No new external dependencies.** `FrustumFitMath` uses only `UnityEngine.Mathf` and `UnityEngine.Vector2`. `FrustumFit` uses `UnityEngine.Camera`, `UnityEngine.Renderer`, and `UnityEngine.Transform` — all already available in `Game.GearEngine`.

**Future integration point (not in this plan):**

    // ScreenResizeWatcher (future) publishes ScreenResizedEvent.
    // FrustumFitCoordinator (future) subscribes and calls Apply()
    // on all registered FrustumFit components via OnEnable/OnDisable registration.
    // FrustumFit._applyEveryFrame can bridge the gap until then.

---

*Revision note: Initial version authored from stakeholder request — frustum-relative responsive scaling for world-space objects, fill percentage + fill mode per object, pure math layer separated for testability, resize detection deferred.*
