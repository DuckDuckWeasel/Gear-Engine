# Spline Evaluate Movement

Module documentation for `Game.SplineEvaluate` — the physics-free, pure-spline car movement system.

## Overview

This module replaces the physics-based car simulation (`SplineCarRunnerService` + `PrometeoCarController` + `Rigidbody`) with deterministic spline evaluation. The car's position is computed directly from `SplineContainer.Spline.EvaluatePosition(t)` each frame. No physics forces, no collision, no Rigidbody.

## Assembly

`Game.SplineEvaluate.asmdef` — deliberately excludes `PROMETEO` from its references.

## Architecture

```
SplineEvaluateRunnerService (ITickable, manages drivers)
  └─ SplineEvaluateDriver (pure C#, one per car)
       ├─ Speed Model: curvature lookahead → target speed → accel/brake
       ├─ Lateral Offset: 5-stat personality × LaneProfile curves
       └─ Visuals: body roll, slip angle, suspension bob
```

## Key Types

| Type | Location | Role |
|------|----------|------|
| `SplineDriverConfig` | Definitions/ | ScriptableObject tuning (speed, curvature, visuals) |
| `LaneProfile` | Definitions/ | Per-track AnimationCurves for lateral offset |
| `DriverPersonality` | Definitions/ | 5-stat struct (0–10): Aggression, DriftTendency, LineWidth, Consistency, Risk |
| `SplineMotionState` | Simulation/ | Runtime snapshot (t, speed, offset, visuals) |
| `SplineEvaluateDriver` | Simulation/ | Core tick logic (pure C#) |
| `SplineEvaluateRunnerService` | Simulation/ | Service managing multiple drivers (ITickable) |
| `SplineCurvatureHelper` | Simulation/ | Static curvature sampling via tangent finite differences |
| `SplineEvaluateBootstrap` | Bootstrap/ | Scene launcher |
| `SplineEvaluateScope` | Bootstrap/ | LifetimeScope |
| `SplineEvaluateHUD` | Presentation/ | Debug HUD with stat sliders |

## The 5 Stats

| Stat | Range | Effect |
|------|-------|--------|
| Aggression | 0–10 | Inside line cutting at corners |
| Drift Tendency | 0–10 | Wider exit lines (visual drift) |
| Line Width | 0–10 | General lane wandering amplitude |
| Consistency | 0–10 | Reduces Perlin noise variation (10 = robotic) |
| Risk | 0–10 | Shorter curvature lookahead → later braking |

## Shared Types (from Game.CarSimulation)

- `CarEntity`, `CarDefinition`, `TrackDefinition`
- `CarTelemetryData`, `SimulationLifecycleState`
- `CarEntityFactory`

## Scene Setup

1. Create a scene with a `SplineContainer` (the track)
2. Add `SplineEvaluateScope` with `SplineDriverConfig` and optional `LaneProfile`
3. Add `SplineEvaluateBootstrap` with `SplineContainer` + `CarDefinition` references
4. Add `SplineEvaluateHUD` for debug controls
5. The car prefab needs only a mesh — no Rigidbody, no PrometeoCarController

## Revision

Update this file when types, stats, or tick pipeline change.
