# Car Simulation module

Runtime code lives in `Assets/Scripts/Game/CarSimulation/` (`Game.CarSimulation` assembly).

## Track selection

- **`TrackDefinition`** (`Definitions/TrackDefinition.cs`) — ScriptableObject holding a `Spline` (knots, closed flag) and a display name.
- **`Track`** (`Track/Track.cs`) — Scene component with a `SplineContainer`. Call `Initialize(TrackDefinition)` at startup to copy spline data from the asset into the container (used by `CarTrackBootstrap`).
- **`CarTrackScope`** — Inherits **`SceneFoundationScope`** (shared Addressables, navigation, events, cross-layer resolver); installs car track services and registers **`CarTrackBootstrap`**. Assign `CarDefinition` and `TrackDefinition` on the bootstrap; swap the `TrackDefinition` asset to change the course. See [`SceneFoundation.md`](SceneFoundation.md).

## Editor setup

Menu **Game / Car Simulation / Setup Scene** refreshes track assets from `SplineTrack_TestScene`, expects a scene root **Track** (legacy name `CircleRaceTrack`) with **`Path`** (spline mesh) and runtime **Car** under the same root, and wires `CarTrack_LifetimeScope`.

## Tests

EditMode tests: `Assets/Scripts/Game/CarSimulation/Tests/Editor/` (`Game.CarSimulation.Tests`).
