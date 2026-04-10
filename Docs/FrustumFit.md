# FrustumFit

`FrustumFit` is a MonoBehaviour that scales a world-space GameObject so it occupies a configured fraction of the camera's visible frustum area. It is the world-space equivalent of a Canvas Scaler anchor — instead of `RectTransform` anchors that adapt UI elements, `FrustumFit` computes and applies `localScale` so any world-space mesh always fills the intended proportion of the screen, regardless of resolution, aspect ratio, field of view, or projection type.

---

## Quick Start

1. Place a world-space **Quad** in the scene (GameObject > 3D Object > Quad).
2. Position it in front of the camera at a known Z distance, e.g. `Z = 10`.
3. Add the `FrustumFit` component to the Quad.
4. In the Inspector, configure:
   - **Camera** — drag in the scene camera (or leave empty; defaults to `Camera.main`).
   - **Depth** — set to `10` (the Quad's distance from the camera).
   - **Fill X / Fill Y** — `1` fills the full frustum width/height.
   - **Mode** — `Fit` to fill the smaller dimension uniformly (no distortion).
   - **Axes** — `XY` for a Quad (default).
   - **Apply On Start** — enabled (applies the scale as soon as the scene starts).
5. Enter Play mode. The Quad fills the screen.

---

## Fill Modes

| Mode        | Behavior                                                                   | Analogy       |
|-------------|----------------------------------------------------------------------------|---------------|
| `Stretch`   | Each axis scales independently to hit the exact fill percentages. Aspect ratio is not preserved. | CSS `stretch` |
| `Fit`       | Uniform scale. Fits entirely inside the target box; may leave empty space on two sides (letterbox). | CSS `contain` |
| `Fill`      | Uniform scale. Covers the entire target box; may exceed one axis (crop). | CSS `cover`   |
| `FillWidth` | Matches the target width exactly. Height scales proportionally.            | —             |
| `FillHeight`| Matches the target height exactly. Width scales proportionally.            | —             |

**Fill X / Fill Y** define the target box as a fraction of the frustum. `fillX = 0.8, fillY = 0.8` means the target box is 80% of the frustum width and 80% of the frustum height. The fill mode then determines how the object occupies that box.

Values greater than `1` are valid — they intentionally make the object bleed past the screen edge (e.g. a background plane that extends 10% past every edge: `fillX = 1.1, fillY = 1.1`).

---

## Axes

The `FrustumFitAxes` field tells the component which two local-space axes of the mesh map to screen horizontal and screen vertical.

| Value | Screen horizontal | Screen vertical | When to use |
|-------|-------------------|-----------------|-------------|
| `XY`  | Local X           | Local Y         | **Default.** Unity Quad, custom meshes, sprites. |
| `XZ`  | Local X           | Local Z         | Unity **Plane** primitive (10×10 flat mesh). Rotate the Plane 90° on X to face the camera, then use `XZ`. |
| `YZ`  | Local Y           | Local Z         | Custom meshes with an unusual facing convention. |

The component reads `Renderer.localBounds.size` to determine the mesh's natural size on the selected axes. This means it works correctly for any mesh shape — a unit Quad (`1×1`), a Unity Plane (`10×10`), or a custom-sized mesh.

---

## Depth

`_depth` is the world-space distance from the camera at which the frustum is sampled. Set it to match the object's actual Z distance from the camera.

For **orthographic cameras**, depth has no mathematical effect — the frustum width and height are determined solely by `orthographicSize` and the camera's aspect ratio. You can leave `_depth` at any positive value when using an orthographic camera.

---

## Update Timing

| Field | Behavior |
|---|---|
| `_applyOnStart = true` | Applies once on `Start`. Sufficient for static scenes or scenes where the camera settings never change. |
| `_applyEveryFrame = true` | Applies every `LateUpdate`. Use when camera FOV, window size, or fill settings change at runtime. Has a minor per-frame cost (one frustum calculation per component). |
| Both false | Only applies when `Apply()` is called externally. |

### Calling `Apply()` externally

`Apply()` is public and idempotent. For best performance when supporting runtime screen resize, disable `_applyEveryFrame` on all components and instead call `Apply()` from a centralized resize coordinator that detects when `Screen.width` or `Screen.height` changes:

```csharp
// Example: drive all FrustumFit components from a coordinator
void OnScreenResized()
{
    foreach (var fit in _registeredComponents)
        fit.Apply();
}
```

See the Decision Log in `Plans/FrustumFit/FrustumFit-ExecPlan.md` for the planned `ScreenResizeWatcher` + `FrustumFitCoordinator` upgrade path.

---

## Parent Hierarchy

`FrustumFit` correctly handles parent transforms with non-trivial scale. It divides the desired world-space scale by the parent's `lossyScale` on the relevant axes to produce the correct `localScale`. Place `FrustumFit` objects under scaled parents freely — the math accounts for it.

---

## Multiple Objects

Each `FrustumFit` instance is fully independent. Place the component on as many objects as needed; each computes its own scale from its own `Renderer.localBounds` and its own fill settings. There is no shared state between instances.

---

## Assembly

All types live in `Game.GearEngine.Presentation.World` inside the `Game.GearEngine` assembly:

```
Assets/Scripts/Game/GearEngine/Presentation/World/
  FrustumFillMode.cs
  FrustumFitAxes.cs
  FrustumBounds.cs
  FrustumFitMath.cs
  FrustumFit.cs
```

Tests are in `Assets/Scripts/Game/GearEngine/Tests/Editor/FrustumFitMathTests.cs` (EditMode, `Game.GearEngine.Tests` assembly).
