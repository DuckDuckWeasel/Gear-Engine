# Car Simulation module

Runtime code lives in `Assets/GearEngine/Scripts/Game/CarSimulation/` (`Game.CarSimulation` assembly).

## Race session (spline + AI runner)

- **`RaceState`** holds race progress: **`RaceTime`**, **`CurrentLap`**, **`LapTimes`**, **`Phase`** (**`SimulationLifecycleState`**: Created / Running / Paused / Completed), plus references to **`CarEntity`** and **`TrackDefinition`**. **`RaceSessionConfig`** (including **`TotalLaps`**) is applied when the session is created via **`TrackSimulationFactory`**.
- **`TrackViewComponent`** owns the scene **`SplineContainer`**, copies **`TrackDefinition`** into the container on bind, and rebuilds extruded visuals.
- **`TrackViewModel`** mirrors **`RaceState`** into observable HUD fields and forwards start/stop to **`RaceManagerService`**.
- **`SplineCarRunnerService`** drives **`PrometeoCarController`** along the spline; **`CarView`** may place the car at the spline start without attaching the runner until **`CarView.AttachRunner()`** (used by the spline test scene preview vs race flow).
- **`CarTrackScreenViewModel`** is the root screen for **`SplineTrack_TestScene`**: it builds preview **`RaceState`** instances, registers them with **`RaceManagerService`** on first race start, and requests runner attachment via **`AttachRunnersRequested`** before **`TrackViewModel.Toggle(true)`**.
- **`CarTrackBootstrap`** only validates serialized refs and opens **`CarTrackScreenViewModel`** (orchestration lives in the screen VM, not the bootstrap).

## Tests

EditMode tests: `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/` (`Game.CarSimulation.Tests`).
