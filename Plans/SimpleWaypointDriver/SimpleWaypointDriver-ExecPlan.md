# Simple waypoint driver (replace curvature simulation)

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

Repository planning rules live at `PLANS.md` (repository root).

---

## Purpose / Big Picture

Replace the current **1D distance + curvature lookahead + grip drift** model (`TrackSimulationRunner`) with a **minimal steering loop**:

- **Waypoints** come only from the **existing track spline** (resampled positions; optional tangents for reference).
- **Car tuning** comes only from **`CarEntity` + `CarVariableSet`** (same variables the race already uses, e.g. speed), not from a separate tuning SO full of probe distances.
- Each tick: **seek next waypoint**, **yaw limited by turn rate**, **slow down if the corner demands more yaw than allowed**, **optional drift / “perfect line”** based on how large the required correction is.
- **Presentation**: car transform follows **integrated position + heading** (with optional visual slip), not `BakedTrackProfile.Evaluate(distance)` as the sole pose source.

**How to see it working:** Open the race scene, start the race: car accelerates, steers toward spline-derived waypoints, visibly eases speed on tight bends, shows drift only in a middle band of steering error, and UI still shows lap / time / progress / speed / drifting from `RaceRuntimeState`.

---

## Progress

- [x] **M1** — Add `SplineWaypointPath` (build waypoints from `TrackDefinition.Spline` / `Spline`).
- [x] **M2** — Add `SimpleTrackDriverTuning` (serializable block on `TrackSimulationConfig`).
- [x] **M3** — Replace `TrackSimulationRunner` step with `SimpleWaypointDriver`; keep `TrackSimulation` lifecycle + `RaceRuntimeState` for UI.
- [x] **M4** — Rework `CarSplineDriver` to apply pose from driver state (position, yaw, optional slip).
- [x] **M5** — Collapse `TrackSimulationConfig` / remove `TrackSimulationTuning` usage; update scene serialized config.
- [x] **M6** — Rewrite `TrackSimulationRunnerTests`; add waypoint advance test.
- [x] **M7** — Update `Docs/Game/CarSimulation.md`.
- [ ] **Validate** — Run `validate-changes.ps1` / Unity EditMode tests in an environment with Unity + pwsh.

---

## Surprises & Discoveries

- (Fill during implementation.)

---

## Decision Log

- **Decision:** Keep **`TrackSimulation`** as the MVVM **aggregate** (lifecycle, `Race`, `Motion`, `Car`) but treat **`Motion`** as **kinematic state** for the simple driver, not “distance along baked samples” as the primary pose.
  **Rationale:** `RaceViewModel` / `TrackViewModel` already bind here; smaller surface change than new top-level types.

- **Decision:** **Spline-only** waypoint source; do not author separate waypoint assets in v1.
  **Rationale:** Matches user preference; one source of truth for the track line.

- **Decision:** **Remove** `TrackSimulationTuning` and lookahead/probe parameters once the new driver ships.
  **Rationale:** User asked for minimal config; old knobs are unused.

- **Decision:** `CarSplineDriver` stops being a pure “sample spline at distance” follower; it becomes **view sync** from `CarMotionState` extended with **world position + heading** (or a small nested `KinematicPose` struct on motion).
  **Rationale:** Pose must match the simple integrator, not the old rail.

---

## Outcomes & Retrospective

(Summarize at completion.)

---

## Context and Orientation

### Term definitions

- **Waypoint:** A world-space point on the track polyline derived by sampling the spline at fixed arc-length steps (or per-span count). Index `i` is the current target; advancing happens when the car passes within radius or crosses a half-plane in front of the point.
- **Required correction:** Signed yaw error (degrees) from current forward to the direction toward the lookahead target, optionally blended with tangent alignment for “perfect line” mode.
- **Turn rate cap:** Maximum `|d(yaw)/dt|` derived from config, optionally scaled by speed read from `CarEntity` variables.
- **Drift band:** If `|error|` is between `driftMin` and `driftMax`, increase visual slip / mild speed penalty; if below `perfectMax`, tighten line and clear drift.

### Files likely touched

| Area | File(s) |
|------|---------|
| Runner | `Simulation/TrackSimulationRunner.cs` → thin delegate or replace body |
| New | `Simulation/SimpleWaypointDriver.cs` (static methods or instance), `Tracks/SplineWaypointPath.cs` |
| Motion | `Simulation/CarMotionState.cs` — add `Position`, `Heading` (or yaw degrees), maybe remove obsolete fields |
| Driver MB | `Drivers/CarSplineDriver.cs` |
| Config | `Definitions/TrackSimulationConfig.cs`, remove `TrackSimulationTuning.cs`, asset `TrackSimulationTuning.asset` |
| Factory | `TrackSimulationFactory.cs`, `TrackSimulation.cs` ctor (drop tuning param) |
| Race | `Race/RaceStartData.cs`, tests under `Race/Tests`, `CarSimulation/Tests` |
| Docs | `Docs/Game/CarSimulation.md` |

---

## Plan of Work

### Phase A — Waypoint extraction (no behavior change yet)

Build a readonly list of `Vector3` waypoints in **track local space** (same space as spline knots), plus cumulative distances for “progress along path” if needed for `Progress01`.

### Phase B — New step function

Single entry point called from the existing tick (`TrackSimulationRunner.Step`): inputs `dt`, `SplineWaypointPath`, `CarMotionState`, `CarEntity` (for variables), `SimpleTrackDriverTuning`, `RaceRuntimeState`, closed/open track flag.

### Phase C — Presentation + removal

`CarSplineDriver` reads new motion fields. Delete unused tuning types, tests, and documentation references.

---

## Concrete steps (step-by-step)

### 1) Add waypoint builder

- New type `SplineWaypointPath` constructed from `Spline` + `SplineContainer` transform (or bake in world space once at factory time using the same transform `CarSplineDriver` uses).
- Method sketch:

```csharp
public sealed class SplineWaypointPath
{
    public IReadOnlyList<Vector3> PointsLocal { get; }
    public IReadOnlyList<float> CumulativeLength { get; }
    public float TotalLength { get; }
    public bool IsClosed { get; }

    public static SplineWaypointPath Build(Spline spline, Transform splineTransform, float spacingMetres);
    public Vector3 GetWorldPoint(int index, Transform splineTransform);
    public float GetHeadingToNext(int index, Transform splineTransform); // optional helper
}
```

- **Spacing:** single serialized field on minimal tuning, e.g. `waypointSpacingMetres = 4f` (tune in editor).

### 2) Extend `CarMotionState` for kinematic pose

Minimal additions:

```csharp
internal sealed class CarMotionState
{
    public Vector3 Position;        // world
    public float YawDegrees;      // world, around spline up
    public int WaypointIndex;
    // Retain Speed, PendingSpeedBoost for boosts from gameplay if needed.
    // Remove or stop writing: Distance, SampleIndex, LateralOffset, SlipAngle, DriftIntensity
    //   — OR keep SlipAngle/DriftIntensity only as visuals written by the new driver.
}
```

- **Reset:** place car at waypoint 0, yaw toward waypoint 1 (or tangent from first segment).

### 3) Minimal tuning surface

Prefer **one** small config object referenced by `TrackSimulationConfig` (replacing `TrackSimulationTuning`):

```csharp
[Serializable]
public sealed class SimpleTrackDriverTuning
{
    [SerializeField] private float waypointSpacingMetres = 4f;
    [SerializeField] private float waypointCaptureRadius = 2.5f;
    [SerializeField] private float lookaheadMetres = 6f;
    [SerializeField] private float baseMaxYawRateDegreesPerSecond = 90f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float braking = 18f;
    [SerializeField] private float cornerSlowdownYawDemandScale = 1f; // maps excess demand to target speed drop
    [SerializeField] private float perfectLineErrorDegrees = 8f;
    [SerializeField] private float driftErrorMinDegrees = 12f;
    [SerializeField] private float driftErrorMaxDegrees = 35f;
    [SerializeField] private float driftSpeedPenalty = 0.06f;      // multiplier on speed when drifting
    [SerializeField] private float slipVisualLerpSpeed = 4f;
}
```

- If you want **zero** new ScriptableObjects: embed this as a `[SerializeField]` on `TrackSimulationConfig` only (no `CreateAssetMenu`).

### 4) Driver core (replace `RunDynamicsStep` body)

Pseudocode / snippet for the tick:

```csharp
public static void Step(
    float dt,
    SplineWaypointPath path,
    Transform trackTransform,
    CarMotionState motion,
    CarEntity car,
    CarVariableSet variables,
    SimpleTrackDriverTuning t,
    RaceRuntimeState race,
    bool isClosed)
{
    float topSpeed = ResolveTopSpeed(car, variables);

    Vector3 pos = motion.Position;
    float yaw = motion.YawDegrees;

    int targetIndex = NextTargetIndex(path, motion.WaypointIndex, pos, trackTransform, t.WaypointCaptureRadius);
    Vector3 seekPoint = path.EvaluateLookaheadWorld(targetIndex, trackTransform, t.LookaheadMetres);
    Vector3 to = seekPoint - pos;
    to.y = 0f;
    float desiredYaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
    float yawError = Mathf.DeltaAngle(yaw, desiredYaw);

    float demand = Mathf.Abs(yawError) / Mathf.Max(dt, 1e-4f); // deg/s “needed”
    float maxYaw = t.BaseMaxYawRateDegreesPerSecond * YawScaleFromSpeed(motion.Speed, topSpeed);
    float targetSpeed = topSpeed;
    if (demand > maxYaw)
    {
        float excess = demand - maxYaw;
        targetSpeed = Mathf.Max(0f, topSpeed - excess * t.CornerSlowdownYawDemandScale);
    }

    motion.Speed = StepSpeedToward(motion.Speed, targetSpeed, dt, t.Acceleration, t.Braking);

    float yawStep = Mathf.Sign(yawError) * Mathf.Min(Mathf.Abs(yawError), maxYaw * dt);
    yaw = yaw + yawStep;

    Vector3 fwd = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
    pos += fwd * (motion.Speed * dt);

    motion.WaypointIndex = targetIndex;
    motion.Position = pos;
    motion.YawDegrees = yaw;

    ApplyDriftVisuals(motion, yawError, t, dt);

    AdvanceRaceStats(path, motion, race, dt, isClosed, trackTransform);
}
```

- **`NextTargetIndex`:** while distance to current waypoint < capture radius, increment (wrap if closed).
- **`EvaluateLookaheadWorld`:** walk forward along cumulative length for `lookaheadMetres` from current target.
- **`YawScaleFromSpeed`:** optional `Mathf.Lerp(1.2f, 0.75f, motion.Speed / topSpeed)` so fast cars feel less nimble.

**Drift / perfect line** (replace old grip math):

```csharp
private static void ApplyDriftVisuals(CarMotionState motion, float yawErrorDeg, SimpleTrackDriverTuning t, float dt)
{
    float a = Mathf.Abs(yawErrorDeg);
    bool perfect = a <= t.PerfectLineErrorDegrees;
    bool drift = a >= t.DriftErrorMinDegrees && a <= t.DriftErrorMaxDegrees;

    float targetSlip = drift ? Mathf.Sign(yawErrorDeg) * 25f : 0f;
    motion.SlipAngle = Mathf.Lerp(motion.SlipAngle, targetSlip, dt * t.SlipVisualLerpSpeed);
    motion.DriftIntensity = Mathf.Lerp(motion.DriftIntensity, drift ? 1f : 0f, dt * 3f);

    race.IsDrifting = drift && motion.Speed > 1f;
    // optional: scale speed by (1 - drift * t.DriftSpeedPenalty) when computing displacement
}
```

### 5) `CarSplineDriver` changes

- On `Bind`, set `motion.Position` from waypoint 0 world position and initial yaw.
- In `Update`, if running: **do not** call `profile.Evaluate`. Instead:

```csharp
transform.SetPositionAndRotation(
    motion.Position,
    Quaternion.Euler(0f, motion.YawDegrees, 0f) * Quaternion.Euler(0f, motion.SlipAngle, 0f));
```

- If the track is banked, later upgrade to `LookRotation` using spline up at nearest point; v1 can assume **flat** race plane (matches current simple yaw approach).

### 6) Wire factory and remove dead types

- `TrackSimulationFactory`: after baking profile (if still needed for length only) or **remove `BakedTrackProfile` from hot path**, build `SplineWaypointPath` once and store on `TrackSimulation` (new internal field `WaypointPath`).
- `TrackSimulation` ctor: accept `SplineWaypointPath` + `SimpleTrackDriverTuning` (or embedded fields on config), remove `TrackSimulationTuning`.
- Delete `TrackSimulationTuning.cs`, remove `.asset` references, update `TrackSimulationConfig` to hold only `CarVariableSet` + `SimpleTrackDriverTuning` (serializable block).

### 7) `RaceRuntimeState` / progress

- `CurrentSegmentIndex` → map to `WaypointIndex`.
- `Progress01` → `motion.WaypointIndex / (float)(path.Count - 1)` or **distance along polyline / TotalLength** for smoother bar.
- `DistanceTravelled` += `motion.Speed * dt` (or projected on path if you prefer consistency with spline length).

### 8) Tests

- **Waypoint advance:** mock a short path; step driver with position moving into capture radius; assert index increments.
- **Yaw cap:** fixed target to the side; assert per-frame yaw change ≤ `maxYaw * dt`.
- **Closed loop:** last waypoint to first wraps; lap counter increments (same expectations as current `StripClosedLaps` behavior, but driven by waypoint index wrap).

---

## Validation and acceptance

- [ ] Race start / stop still toggles `TrackSimulation.State` and gear engine as today.
- [ ] No references to `TrackSimulationTuning` in code or serialized assets.
- [ ] Car visibly corners by steering, not sliding on an invisible rail.
- [ ] Drift flag only true in the configured error band.
- [ ] EditMode tests pass.

---

## Idempotence and recovery

- Re-running `SplineWaypointPath.Build` on the same spline + spacing is deterministic; safe to rebuild when entering Play Mode.
- If `spacingMetres` is too large, fall back to minimum waypoint count (e.g. at least one per spline knot).

---

## Artifacts and notes

- Optional debug draw: gizmos for waypoint spheres and lookahead point (dev-only component or `#if UNITY_EDITOR`).

---

## Interfaces and dependencies

- **No new public interface required** if `ITrackSimulationRunner` remains the tick entry; implementation swaps internals.
- **`CarEntity`:** continue using `TryGetValue<float>` for speed (same as current runner’s top speed resolution — mirror that helper in the new driver).
- **Unity Splines:** `Spline`, `SplineUtility` / `SplineCache` for equidistant sampling — pick one API already used in `TrackProfileBaker` for consistency.

---

## Snippet: collapsed `TrackSimulationConfig`

```csharp
[Serializable]
public sealed class TrackSimulationConfig
{
    public CarVariableSet Variables => variables;
    public SimpleTrackDriverTuning Driver => driver;

    [SerializeField] private CarVariableSet variables;
    [SerializeField] private SimpleTrackDriverTuning driver = new();
}
```

This is the **entire** race-side tuning surface beside per-car variables.
