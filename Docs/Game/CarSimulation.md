# Car Simulation module

Runtime code lives in `Assets/Scripts/Game/CarSimulation/` (`Game.CarSimulation` assembly).

## Track selection

- **`TrackDefinition`** (`Definitions/TrackDefinition.cs`) — ScriptableObject holding a `Spline` (knots, closed flag) and a display name.
- **`Track`** (`Track/Track.cs`) — Scene component with a `SplineContainer`. Call `Initialize(TrackDefinition)` at startup to copy spline data from the asset into the container (used by `CarTrackBootstrap`).
- **`CarTrackScope`** — Assign `CarDefinition`, `TrackDefinition`, and the scene `Track` reference. Swap the `TrackDefinition` asset to change the course (e.g. `CircleTrack` vs `SquareTrack` under `Assets/Game/CarSimulation/Data/Tracks/`).

## Editor setup

Menu **Game / Car Simulation / Setup Scene** refreshes track assets from `SplineTrack_TestScene`, ensures a `Track` child under `CircleRaceTrack`, and wires `CarTrack_LifetimeScope`.

## Tests

EditMode tests: `Assets/Scripts/Game/CarSimulation/Tests/Editor/` (`Game.CarSimulation.Tests`).
