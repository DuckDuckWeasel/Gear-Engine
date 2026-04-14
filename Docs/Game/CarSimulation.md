# Car Simulation module

Runtime code lives in `Assets/GearEngine/Scripts/Game/CarSimulation/` (`Game.CarSimulation` assembly).

## Rolling solver

- **`TrackProfileBaker`** bakes a `Spline` into a **`BakedTrackProfile`** (fixed-distance `TrackSample`s: position, frame, curvature). No car data is baked.
- **`TrackSimulation`** is a **`Scaffold.MVVM.Model`**: lifecycle (`State`), **`RaceRuntimeState`** (public observables: lap, time, progress, speed, drift flag, distance travelled), and internal **`CarMotionState`** (distance, speed, drift visuals, pending speed boost).
- **`TrackSimulationRunner`** (`ITickable`, VContainer singleton) steps the solver each frame when the simulation is **Running**: reads **top speed** from **`TrackSimulation.CarVariables.Speed`** via **`CarEntity.TryGetValue<float>`** (fallback default if the set or value is missing), applies lookahead braking, acceleration limits, drift intensity, and lap wrapping on closed splines.
- **`CarVariableSet`** — ScriptableObject facade listing **`VariableSO`** references the simulation watches (e.g. **Speed**); assign on **`CarTrackBootstrap`** (and optionally **`RaceStartData`** for race flow).
- **`CarEntityFactory`** — creates **`CarEntity`** instances with incrementing **`InstanceId`**s; **`CarEntity`** exposes **`Definition`** and **`CarPrefab`** for presentation code (base **`EntityInstance`** definition access is internal to Scaffold.Entities).
- **`CarSplineDriver`** sets the car transform from **`CarMotionState`** + **`BakedTrackProfile`** (no `SplineAnimate` driving path).
- **`TrackViewModel`** mirrors **`TrackSimulation.State`** and **`RaceRuntimeState`** fields for UI; it does not use `BindChildViewModel` on the simulation model.

## Track selection

- **`TrackDefinition`** (`Definitions/TrackDefinition.cs`) — ScriptableObject holding a `Spline` (knots, closed flag) and a display name. The factory bakes this spline when creating a **`TrackSimulation`**.
- **`Track`** (`Track/Track.cs`) — Scene component with a `SplineContainer`. Binds a **`TrackViewModel`** and copies spline data from the definition into the container for rendering.
- **`CarTrackScope`** — Assign `CarDefinition`, `TrackDefinition`, and the scene `Track` reference.

## Editor setup

Menu **Game / Car Simulation / Setup Scene** refreshes track assets from `SplineTrack_TestScene`, expects a scene root **Track** with **`Path`** (spline mesh) and runtime **Car** under the same root, and wires `CarTrack_LifetimeScope`.

## Tests

EditMode tests: `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/` (`Game.CarSimulation.Tests`). Includes `BakedTrackProfileTests` and `TrackSimulationRunnerTests`.
