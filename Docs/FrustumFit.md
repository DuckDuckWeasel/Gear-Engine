# FrustumFit and UI-driven world placement

This feature has two parts:

1. **`FrustumFitAnchor`** (preferred for production UI) — a UI `RectTransform` defines the on-screen box; a **world** target (`Transform` + `Renderer`) is moved and scaled to match that box at a chosen camera depth.
2. **`FrustumFit`** (legacy / simple samples) — scales a mesh on the same GameObject to a **fraction of the full camera frustum** at `depth`. It does **not** read UI rects and does **not** change world position.

All runtime types use namespace **`GearEngine.FrustumFit`** in assembly **`Game.GearEngine.FrustumFit`**:

- Scripts: `Assets/GearEngine/Scripts/Core/FrustumFit/`
- Open transition (DOTween): **`FrustumFitAnchorOpenTransition`**, optional inspector runner **`FrustumFitAnchorOpenTransitionRunner`**
- Sample helpers: `Assets/GearEngine/Scripts/Core/FrustumFit/Samples/` (`Game.GearEngine.FrustumFit.Samples`)
- EditMode tests: `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/` (e.g. `FrustumFitMathTests.cs`, `FrustumFitAnchorOpenTransitionTests.cs`; assembly `Game.GearEngine.Tests`, references `Game.GearEngine.FrustumFit` and `DOTween.dll` where needed)

Sample scene: `Assets/GearEngine/Scenes/FrustumFit Sample.unity` (Canvas panel + `FrustumFitAnchor` driving a world sprite). The Canvas also has **`FrustumFitSampleController`**: set **Mode** to **Continuous** (default), **Apply On Key**, or **Tween On Key** (press **Space** to re-fit in the on-demand modes).

---

## Quick start — `FrustumFitAnchor`

1. Add a **Canvas** (any render mode). Put a **panel** `Image` or empty `RectTransform` where you want the world object to appear on screen.
2. Add **`FrustumFitAnchor`** to that UI element (or assign `sourceRect` explicitly).
3. Assign:
   - **World camera** — the camera that renders the world target (often the same as the gameplay camera).
   - **Target transform** — root of the world object.
   - **Target renderer** — `MeshRenderer`, `SpriteRenderer`, etc. (used for `localBounds`).
4. Set **depth** — distance passed to `Camera.ViewportToWorldPoint` (perspective) and to frustum sizing (must be `> 0` for perspective).
5. Choose **fill mode** (`Fit`, `Fill`, `Stretch`, …) and **axes** (`XY` for quads/sprites facing default orientation).
6. Enable **Apply Every Frame** if the UI layout, camera, or window size changes at runtime; otherwise call **`Apply()`** from your coordinator.

Expected result: the world target’s **center** aligns with the UI rect’s **center** on screen, and its **size** matches the UI rect’s screen footprint (per fill mode).

---

## `RectTransform` as source of truth

Use the **resolved** rectangle after layout:

- Prefer reading corners via `RectTransform.GetWorldCorners` (what `RectTransformScreenBoxUtility` does internally).
- Do **not** drive sizing from raw `anchorMin` / `anchorMax` alone — layout groups, offsets, and `CanvasScaler` change the final rect.

---

## Fill modes and axes

| Mode | Behavior |
|------|----------|
| `Stretch` | Independent X/Y scale to hit the exact target box (may distort). |
| `Fit` | Uniform scale; fits inside the box (letterbox). |
| `Fill` | Uniform scale; covers the box (may extend past). |
| `FillWidth` / `FillHeight` | One axis matches exactly; the other follows proportionally. |

**Axes** (`FrustumFitAxes`): which two **local** mesh axes map to screen horizontal and vertical (`XY` default for Unity Quad / most sprites).

---

## Math API (testable, Unity-free except `Mathf` / `Vector`)

`FrustumFitMath`:

- `ComputeBounds(...)` / `FrustumBounds.FromCamera(...)` — full frustum width × height at `depth`.
- `ComputeTargetWorldSize(bounds, viewportWidthFrac, viewportHeightFrac)` — world size of a viewport sub-rectangle.
- `ComputeLocalScaleForTargetSize(targetWorldSize, mode, meshSize, parentLossyScale)` — local scale for a given world target box.
- `ComputeLocalScale(bounds, fillX, fillY, ...)` — **full-frustum** fractions (used by `FrustumFit`).

Parent `lossyScale` on the relevant axes is corrected so hierarchies with scaled parents still match the intended world size.

---

## Legacy — `FrustumFit` (full frustum only)

Use when you want “fill 80% of the **entire** view height/width at this depth” without any UI rect:

- Attach to the GameObject that owns the `Renderer`.
- Configure `fillX`, `fillY`, `depth`, `FrustumFillMode`, `FrustumFitAxes`.
- **Scale only** — third local axis is left unchanged (for `XY`, Z is not modified).

---

## Compute without applying (tweens)

Use **`FrustumFitAnchor.TryComputePlacement`** to get a **`FrustumFitAnchorPlacement`** (world position, full local scale, optional world rotation) without touching the target transform.

- **Instance:** `bool ok = anchor.TryComputePlacement(out FrustumFitAnchorPlacement p);` — uses the anchor’s serialized fields; baseline local scale is the target’s current `localScale`.
- **Static:** overload with explicit `RectTransform`, `Canvas`, `Camera`, `Transform`, `Renderer`, modes, and **`baselineLocalScale`** (the third axis is copied from this vector).

Then either:

- tween toward `placement.WorldPosition`, `placement.LocalScale`, and optionally `placement.WorldRotation` when `placement.HasWorldRotation`, or
- snap with **`placement.ApplyTo(targetTransform)`** on the computed **`FrustumFitAnchorPlacement`**.

**Rotation:** `FrustumFitAnchorRotationMode.MatchCameraRotation` sets `HasWorldRotation` and `WorldRotation` to the world camera’s rotation (typical for screen-facing sprites). `PreserveTarget` leaves rotation to the tween or prior state.

### Open transition (multi-anchor, DOTween)

Use **`FrustumFitAnchorOpenTransition.Play`** to tween one or more **`FrustumFitAnchor`** targets from their **current** pose to a **freshly computed** placement (DOTween **`DOMove`**, **`DOScale`** for local scale, **`DORotateQuaternion`** when `HasWorldRotation`). Default easing is **`Ease.InOutQuad`** (smooth in/out; not identical to a manual smoothstep curve).

- **Return value:** a **`Tween`** / **`Sequence`**, or **`null`** if nothing could be driven (invalid anchor, failed compute, empty list). On completion, **`FrustumFitAnchorPlacement.ApplyTo`** runs so the final pose matches math exactly.
- **Inspector:** add **`FrustumFitAnchorOpenTransitionRunner`**, assign anchors and duration/ease, call **`Play()`** from **`OnOpen`** after content is active. **`OnDisable`** kills the active tween.
- **`PlayAfterCanvasLayout`:** use from a **`MonoBehaviour`** view after **`SetActive(true)`** — runs **`Canvas.ForceUpdateCanvases()`**, waits one frame, then calls **`Play`** with **`Ease.InOutQuad`**. It temporarily sets **`ConfigureAutoApply(false, false)`** on the listed anchors and restores **`applyEveryFrame`** when the tween completes. Optional **`Action onComplete`** runs once after restore (and after the tween snaps, if any); it also runs when **`host`** is null or the anchor list is empty, so callers can defer work until the transition contract completes without referencing DOTween types. The public overloads take **`(host, anchors, duration, onComplete)`** (or **`params` anchors** without callback) so views do **not** need a **`DOTween.dll`** assembly reference; custom easing uses **`Play`** directly from code that references DOTween.
- **UX flow:** typical pattern is **`OnOpen`** → **`SetActive(true)`** on world/UI roots → **`Play(...)`** or **`PlayAfterCanvasLayout`**; **`OnClose`** → **`SetActive(false)`** only (no exit tween).
- **Layout:** **`PlayAfterCanvasLayout`** handles **`Canvas.ForceUpdateCanvases()`** plus **`yield return null`**; if you call **`Play`** directly, do the same first when rects are not ready.
- **Continuous fit:** **`PlayAfterCanvasLayout`** suppresses continuous fit during the tween and restores afterward. If you call **`Play`** directly, turn off **`applyEveryFrame`** / avoid **`Apply()`** fighting the tween, then re-enable **`applyEveryFrame`** when done.

---

## Update timing and performance

- `applyEveryFrame` / `LateUpdate`: simple but costs one alignment pass per component per frame.
- For many instances, prefer **`Apply()`** from a single resize/layout coordinator when `Screen` size or canvas layout changes.

---

## Canvas render modes

`RectTransformScreenBoxUtility` uses:

- **Screen Space Overlay** — `null` event camera for `WorldToScreenPoint` on UI corners.
- **Screen Space Camera** / **World Space** — `Canvas.worldCamera` for UI → screen, then the **world** camera for screen → viewport.

Ensure the assigned **world camera** matches the one that should define depth and viewport for the target object.
