# Car Simulation module

Runtime code lives in `Assets/GearEngine/Scripts/Game/CarSimulation/` (`Game.CarSimulation` assembly).

## Simple waypoint driver

- **`SplineWaypointPath`** — Builds a polyline of world-space points by sampling the track **`Spline`** at a fixed spacing (`SimpleTrackDriverTuning.WaypointSpacingMetres`). Used only for steering targets and path length; no per-frame curvature solver.
- **`TrackSimulation`** is a **`Scaffold.MVVM.Model`**: lifecycle (`State`), **`RaceRuntimeState`** (lap, time, progress, speed, drift flag, distance travelled), and internal **`CarMotionState`** (world **Position**, **YawDegrees**, **WaypointIndex**, **DistanceAlongPath**, speed, slip/drift visuals, pending speed boost). **`TrackRootTransform`** is set from **`CarSplineDriver.Bind`** so the runner uses the same space as the scene spline.
- **`TrackSimulationRunner`** (`ITickable`, VContainer singleton) steps when the simulation is **Running**: applies pending speed boost, then **`SimpleWaypointDriver.Step`** — seeks a lookahead point on the waypoint polyline, caps yaw rate (scaled slightly with speed), brakes when the required turn rate exceeds the cap, updates drift visuals from steering-error bands, integrates position, updates race stats.
- **`SimpleTrackDriverTuning`** — Serializable block on **`TrackSimulationConfig`** (spacing, capture radius, lookahead, yaw rate, accel/brake, corner slowdown scale, drift band degrees, slip lerp). No separate ScriptableObject for track tuning.
- **`CarVariableSet`** — ScriptableObject listing **`VariableSO`** references: **Speed**, **Acceleration**, and **Brake** are read by the driver (with safe defaults if unset); other entries remain for future use or tooling.
- **`CarEntityFactory`** — creates **`CarEntity`** instances; **`CarEntity`** exposes **`Definition`** and **`CarPrefab`** for presentation.
- **`CarSplineDriver`** — Sets the car transform from **`CarMotionState`** (position + yaw + slip). Does not evaluate a baked profile each frame.
- **`TrackViewModel`** mirrors **`TrackSimulation.State`** and **`RaceRuntimeState`** for UI.

## Track profile baking (optional / tooling)

- **`TrackProfileBaker`** bakes a `Spline` into a **`BakedTrackProfile`** (dense samples with curvature). The runtime race driver no longer uses this for motion; tests and tooling may still reference it.

## Track selection

- **`TrackDefinition`** — ScriptableObject holding a `Spline` (knots, closed flag) and a display name. **`TrackSimulationFactory`** builds **`SplineWaypointPath`** from this spline when creating a **`TrackSimulation`**.
- **`Track`** (`Track/Track.cs`) — Scene component with a `SplineContainer`. Binds a **`TrackViewModel`** and copies spline data from the definition into the container for rendering.
- **`CarTrackScope`** — Assign `CarDefinition`, `TrackDefinition`, and the scene `Track` reference.

## Editor setup

Menu **Game / Car Simulation / Setup Scene** refreshes track assets from `SplineTrack_TestScene`, expects a scene root **Track** with **`Path`** (spline mesh) and runtime **Car** under the same root, and wires `CarTrack_LifetimeScope`.

## Tests

EditMode tests: `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/` (`Game.CarSimulation.Tests`). Includes `BakedTrackProfileTests` and `TrackSimulationRunnerTests`.
