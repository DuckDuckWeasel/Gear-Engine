# Spline-Evaluate Car Movement — Replace Physics with Pure Spline Traversal

This ExecPlan is a living document.

---

## Purpose / Big Picture

The current `SplineCarRunnerService` drives cars by **feeding inputs into a physics-based controller** (`PrometeoCarController` + `Rigidbody`). The AI evaluates waypoints from the spline, then presses virtual throttle/brake/steer buttons to make the physics car follow. This creates unpredictable behavior: physics glitches, drift that feels wrong, collisions that break the spline path, and tuning that is extremely fragile.

This plan replaces the entire approach with **pure spline evaluation**: the car's position and rotation are computed directly from `SplineContainer.Spline.Evaluate*(t)` each frame. No Rigidbody, no PrometeoCarController, no physics forces. The car _is_ the spline position at parameter `t`.

On top of the base racing-line spline, **alternative offset splines** (or lateral offset curves) simulate:
- **Drift routes** — wider exit lines through curves
- **Racing line open-up** — entering curves from the outside before cutting in
- **Chance/variation** — randomized lane offsets per driver personality
- **5 tunable stats (0–10)** — interpolate between conservative and aggressive lines

The visual result must look **physics-like** (body roll, slip angle, suspension bob) but be 100% deterministic and spline-driven.

> **Reference video:** "The Continuity of Splines" by Freya Holmér — foundational spline math (Bézier, Catmull-Rom, evaluation, tangent, curvature). The concepts of `EvaluatePosition`, `EvaluateTangent`, and curvature analysis are the core of this system.

---

## Progress

- [x] M1 — Define the new data model and spline lane system
- [x] M2 — Implement `SplineEvaluateDriver` (core tick: position from t)
- [x] M3 — Implement speed model (accel/brake/drift speed caps from curvature)
- [x] M4 — Implement lateral lane interpolation (5-stat system + alternative splines)
- [x] M5 — Implement physics-like visual layer (body roll, slip angle, suspension)
- [x] M6 — New standalone scene + bootstrap (zero external dependencies)
- [x] M7 — Integrate with existing `RaceManagerService` and telemetry
- [x] M8 — Tests and documentation

---

## Surprises & Discoveries

_(to be filled during implementation)_

---

## Decision Log

- **Decision:** Remove `PrometeoCarController` and `Rigidbody` from the new system entirely.
  **Rationale:** The whole point is deterministic spline-only movement. Mixing physics defeats the purpose. The old system stays untouched on its branch for reference.

- **Decision:** Use lateral offset from the base spline rather than authoring N separate SplineContainers.
  **Rationale:** Authoring 5+ full parallel splines per track is impractical. Instead, compute lateral offset per-`t` using the spline's `right` vector. A `LaneOffsetCurve` (AnimationCurve over normalized t) defines how far left/right each "alternative line" sits. This is the same technique the current `GenerateTrajectory` uses for `preCurveWideOffset` / `postCurveWideOffset`, but formalized.

- **Decision:** The 5 tunable stats (0–10) interpolate between offset curves, not between discrete splines.
  **Rationale:** Continuous interpolation gives smooth blending. Each stat controls one aspect of the offset profile. The final lateral offset at any `t` is the weighted blend of all 5 contributions.

- **Decision:** New scene, new assembly, zero references to PROMETEO.
  **Rationale:** Clean break. The `Game.CarSimulation` assembly keeps its PROMETEO dependency. The new module has no dependency on it.

- **Decision:** Keep `CarEntity`, `CarDefinition`, `TrackDefinition`, `RaceState`, `RaceManagerService` as shared contracts.
  **Rationale:** These are already decoupled from physics. The new driver plugs into the same race lifecycle.

---

## Context and Orientation

### Current system (what we're replacing)

```
SplineCarRunnerService.Tick()
  -> Find nearest t on spline (GetNearestPoint)
  -> Generate waypoints ahead on spline (GenerateTrajectory)
  -> Check curvature for braking (CheckForApproachingCurveVector)
  -> Set virtual button presses on PrometeoTouchInput
  -> PrometeoCarController reads buttons -> applies physics forces
  -> Rigidbody moves the car
  -> Arcade steer assist rotates car toward waypoint
  -> Velocity override lerps Rigidbody velocity toward spline direction
```

**Problems:** Physics fights the spline. Drift is unpredictable. Tuning is fragile. Cars fly off track. Performance costs of Rigidbody + collision.

### New system (what we're building)

```
SplineEvaluateDriver.Tick(dt)
  -> Advance t based on current speed and spline length
  -> Compute target speed from curvature lookahead
  -> Integrate speed toward target (accel/brake rates)
  -> Compute lateral offset from 5-stat blend of LaneOffsetCurves
  -> Final world position = spline.EvaluatePosition(t) + right * lateralOffset
  -> Final rotation = Quaternion.LookRotation(tangent, up) + visual roll/slip
  -> Apply visual-only effects (body lean, suspension bob, tire slip angle)
```

### Terms

| Term | Definition |
|------|-----------|
| **t** | Normalized spline parameter (0-1), the car's progress along the track |
| **Curvature** | Rate of direction change at a point on the spline; higher = sharper turn |
| **Lateral offset** | Perpendicular displacement from the spline centerline (meters) |
| **LaneOffsetCurve** | An `AnimationCurve` mapping `t` (0-1) to lateral offset (meters) |
| **Racing line** | The optimal path through a corner: outside-in-outside |
| **Drift route** | A wider exit line simulating controlled oversteer |
| **Body roll** | Visual-only rotation around the forward axis simulating weight transfer |
| **Slip angle** | Visual-only yaw offset simulating tire grip loss |

### The 5 tunable stats (0-10)

| Stat | Effect on lateral offset |
|------|------------------------|
| **Aggression** | How early and deep the car cuts into corners (inside offset) |
| **Drift Tendency** | How wide the exit line is through corners (outside offset post-apex) |
| **Line Width** | General lane variation amplitude (Perlin-based wandering on straights) |
| **Consistency** | Reduces random variation; high = robotic precision, low = human-like errors |
| **Risk** | Late braking + tighter entry; affects both speed model and lateral entry offset |

Each stat produces a lateral offset contribution. The final offset at any `t` is the sum of all 5 contributions, clamped to track width limits.

---

## Plan of Work

### Milestone 1 — Data model and spline lane system

**Goal:** Define all data structures for the new system.

**New files:**

| File | Purpose |
|------|---------|
| `SplineEvaluate/Definitions/SplineDriverConfig.cs` | ScriptableObject: speed limits, accel/brake rates, curvature thresholds, visual params |
| `SplineEvaluate/Definitions/LaneProfile.cs` | ScriptableObject: 5 AnimationCurves (one per stat axis) defining lateral offset vs t |
| `SplineEvaluate/Definitions/DriverPersonality.cs` | Serializable struct: 5 float stats (0-10) |
| `SplineEvaluate/Simulation/SplineMotionState.cs` | Runtime state: t, speed, lateralOffset, slipAngle, bodyRoll, curvature |

**SplineDriverConfig fields:**
- `float maxSpeed` — absolute speed cap (km/h)
- `float minCurveSpeed` — speed floor on hardest curves
- `float accelerationRate` — speed gain per second
- `float brakeRate` — speed loss per second
- `float curvatureLookaheadMeters` — how far ahead to sample curvature
- `int curvatureSampleCount` — number of lookahead samples
- `float maxLateralOffset` — track half-width clamp (meters)
- `float bodyRollScale` — visual roll multiplier
- `float slipAngleScale` — visual slip multiplier
- `float suspensionBobFrequency` — visual bob Hz
- `float suspensionBobAmplitude` — visual bob meters

**SplineMotionState:**
```csharp
public struct SplineMotionState
{
    public float T;                  // 0-1 spline parameter
    public float Speed;              // current speed (m/s)
    public float TargetSpeed;        // speed cap from curvature
    public float LateralOffset;      // meters from centerline
    public float SlipAngle;          // visual yaw offset (degrees)
    public float BodyRoll;           // visual roll (degrees)
    public float SuspensionOffset;   // visual vertical bob (meters)
    public float Curvature;          // current curvature at t
    public int CurrentLap;
}
```

**Validation:** Unit test creates each struct/SO, verifies defaults and serialization.

---

### Milestone 2 — Core SplineEvaluateDriver (position from t)

**Goal:** Car moves along spline by advancing `t` each frame. No lateral offset yet, no visuals yet.

**New files:**

| File | Purpose |
|------|---------|
| `SplineEvaluate/Simulation/SplineEvaluateDriver.cs` | Pure C# class (no MonoBehaviour). Core tick logic. |
| `SplineEvaluate/Simulation/SplineCurvatureHelper.cs` | Static helper: sample curvature, compute max curvature in range |

**SplineEvaluateDriver core loop:**
```
Tick(float dt):
  1. splineLength = spline.GetLength()
  2. distanceThisFrame = state.Speed * dt
  3. state.T += distanceThisFrame / splineLength
  4. if (state.T >= 1.0) -> state.T -= 1.0, lap++
  5. worldPos = splineContainer.EvaluatePosition(state.T)
  6. worldTangent = splineContainer.EvaluateTangent(state.T)
  7. worldUp = splineContainer.EvaluateUpVector(state.T)
  8. carTransform.position = worldPos
  9. carTransform.rotation = Quaternion.LookRotation(tangent, up)
```

**SplineCurvatureHelper:**
- `float SampleMaxCurvature(Spline spline, float fromT, float lookaheadMeters, int sampleCount)`
- Uses finite differences of tangent vectors to approximate curvature
- `curvature = |tangent(t+dt) - tangent(t)| / arcLength(dt)`

**Validation:** Test that advancing t by known distance produces correct world position. Test lap detection on t wraparound.

---

### Milestone 3 — Speed model (accel/brake from curvature)

**Goal:** Car accelerates on straights, brakes before curves, respects speed caps.

**Logic added to SplineEvaluateDriver.Tick:**
```
  maxCurvature = SplineCurvatureHelper.SampleMaxCurvature(...)
  curvatureSeverity = InverseLerp(0, maxCurvatureReference, maxCurvature)
  state.TargetSpeed = Lerp(config.maxSpeed, config.minCurveSpeed, curvatureSeverity)

  if (state.Speed < state.TargetSpeed)
      state.Speed = MoveTowards(state.Speed, state.TargetSpeed, config.accelerationRate * dt)
  else
      state.Speed = MoveTowards(state.Speed, state.TargetSpeed, config.brakeRate * dt)
```

**Risk stat influence:** Higher risk -> shorter lookahead -> later braking.
`effectiveLookahead = config.lookahead * Lerp(1.2, 0.6, risk/10)`

**Validation:** Test that car speed drops before a sharp curve and recovers on straight.

---

### Milestone 4 — Lateral lane interpolation (5-stat blending)

**Goal:** Car position is offset laterally from centerline based on the 5 stats and LaneProfile curves.

**Logic:**
```csharp
float ComputeLateralOffset(float t, DriverPersonality p, LaneProfile profile)
{
    float aggression = profile.AggressionCurve.Evaluate(t) * (p.aggression / 10f);
    float drift      = profile.DriftCurve.Evaluate(t)      * (p.driftTendency / 10f);
    float width      = profile.WidthCurve.Evaluate(t)      * (p.lineWidth / 10f);
    float noise      = PerlinNoise(t * noiseFreq, seed)     * (1f - p.consistency / 10f);
    float riskEntry  = profile.RiskEntryCurve.Evaluate(t)   * (p.risk / 10f);

    float total = aggression + drift + width + noise + riskEntry;
    return Mathf.Clamp(total, -config.maxLateralOffset, config.maxLateralOffset);
}
```

**Applied in Tick:**
```
  Vector3 right = Vector3.Cross(up, tangent).normalized;
  state.LateralOffset = ComputeLateralOffset(state.T, personality, laneProfile);
  carTransform.position = worldPos + right * state.LateralOffset;
```

**Validation:** Test aggression=10 produces max inside offset at apexes. Test all stats at 0 = centerline.

---

### Milestone 5 — Physics-like visual layer

**Goal:** Car looks like it has physics but everything is spline-derived.

**Body roll:**
```csharp
float lateralAccel = curvature * speed * speed;
state.BodyRoll = -lateralAccel * config.bodyRollScale;
```

**Slip angle (visual drift):**
```csharp
float offsetChangeRate = (currentOffset - previousOffset) / dt;
state.SlipAngle = Lerp(state.SlipAngle, offsetChangeRate * config.slipAngleScale, dt * 8f);
```

**Suspension bob:**
```csharp
float speedNorm = speed / config.maxSpeed;
state.SuspensionOffset = Sin(Time.time * config.bobFreq * speedNorm) * config.bobAmp * speedNorm;
```

**Final transform:**
```csharp
Quaternion baseRot = Quaternion.LookRotation(tangent, up);
Quaternion rollRot = Quaternion.AngleAxis(state.BodyRoll, tangent);
Quaternion slipRot = Quaternion.AngleAxis(state.SlipAngle, up);
carTransform.rotation = slipRot * rollRot * baseRot;
carTransform.position = worldPos + right * lateralOffset + up * suspensionOffset;
```

**Validation:** Test body roll sign matches curve direction. Test slip angle increases with drift tendency.

---

### Milestone 6 — Standalone scene and bootstrap

**Goal:** Self-contained scene with zero PROMETEO dependencies.

**New assembly:** `Game.SplineEvaluate.asmdef`
- References: `Scaffold.Entities`, `Scaffold.MVVM*`, `VContainer`, `Unity.Splines`, `Unity.Mathematics`, `Game.CarSimulation` (shared types only)
- Does NOT reference: `PROMETEO`

**Scene contents:**
- SplineContainer with test track
- Camera (Cinemachine follow)
- Car visual prefab (mesh only, no Rigidbody)
- `SplineEvaluateScope`, `SplineEvaluateBootstrap`
- UI: speed HUD + 5 stat sliders for live tuning

**New car prefab:** Pure mesh. `SplineEvaluateCarView` MonoBehaviour applies transform from driver.

**Validation:** Scene plays, car completes laps, sliders change behavior.

---

### Milestone 7 — Integration with RaceManagerService and telemetry

**Goal:** New driver plugs into existing race infrastructure.

1. **Lap detection:** Driver fires `OnLapCompleted` when `t` wraps
2. **Telemetry:** `GetTelemetry()` returns `CarTelemetryData`
3. **Pause/Resume:** `SetPaused(bool)` — speed lerps to 0, t stops
4. **RaceState:** Wired through `RaceManagerService`

**Validation:** Full race with laps, telemetry HUD, pause/resume.

---

### Milestone 8 — Tests and documentation

**EditMode tests:**
- SplineMotionState defaults
- Driver advances t correctly
- Lap detection on t wraparound
- Curvature helper accuracy
- Speed model brakes/accelerates correctly
- Lateral offset respects stats
- Body roll sign correctness
- Pause stops advancement

**Documentation:** `Docs/SplineEvaluateMovement.md`

---

## Validation and Acceptance

1. New scene runs standalone — car completes 3 laps on spline
2. Zero references to PROMETEO or Rigidbody in new assembly
3. Adjusting each of the 5 stats (0-10) visibly changes car behavior
4. Car looks physics-like (body roll, drift visual, suspension bob)
5. Lap counting and race lifecycle work through RaceManagerService
6. All EditMode tests pass
7. `.agents/scripts/validate-changes.cmd` clean

---

## Idempotence and Recovery

- Each milestone is independently compilable
- The old `SplineCarRunnerService` is untouched — lives on main branch
- New code lives in a separate assembly (`Game.SplineEvaluate`)
- If any milestone fails, revert only that milestone's files

---

## Interfaces and Dependencies

### Shared types consumed (from `Game.CarSimulation`):
- `CarEntity`, `CarDefinition`, `TrackDefinition`
- `RaceState`, `RaceManagerService`, `RaceSessionConfig`
- `CarTelemetryData`, `SimulationLifecycleState`
- `CarVariableSet`, `RoguelikeCarStats`
- `TrackSimulationFactory`, `CarEntityFactory`

### New types introduced:
- `SplineEvaluateDriver` — core simulation (pure C#, ITickable)
- `SplineDriverConfig` — ScriptableObject tuning
- `LaneProfile` — ScriptableObject per-track offset curves
- `DriverPersonality` — per-car stat struct (5 floats 0-10)
- `SplineMotionState` — runtime state struct
- `SplineCurvatureHelper` — static curvature math
- `SplineEvaluateCarView` — MonoBehaviour on car prefab
- `SplineEvaluateScope` — LifetimeScope
- `SplineEvaluateBootstrap` — scene launcher
- `SplineEvaluateInstaller` — DI registration

### Assembly dependency graph:
```
Game.SplineEvaluate
  +-- Game.CarSimulation  (shared types only)
  +-- Scaffold.Entities
  +-- Scaffold.MVVM / View / ViewModel
  +-- VContainer
  +-- Unity.Splines
  +-- Unity.Mathematics
```

---

## Artifacts and Notes

- Reference video: https://www.youtube.com/watch?v=jvPPXbo87ds (spline math foundations)
- The current `SplineCarRunnerService` (633 lines) is the primary reference for existing behavior patterns
- The `LaneProfile` concept replaces the current Perlin-noise-based offset system with authored curves per track
