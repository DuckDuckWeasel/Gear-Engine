# Car Simulation module

Runtime code lives in `Assets/GearEngine/Scripts/Game/CarSimulation/` (`Game.CarSimulation` assembly).

## Lap race session (spline + speed)

- **`CarEntity`** holds live stats. Travel speed comes from **`CarVariableSet.Speed`** (`VariableSO`) on **`RaceSessionConfig`**, read each tick from the entity bag.
- **`Track`** owns the scene **`SplineContainer`**, copies **`TrackDefinition`** on bind, and calls **`LapRaceSession.BindSpline`** so length and open/closed state match the scene spline.
- **`LapRaceSession`** holds race progress: **`ProgressDistance`**, **`NormalizedProgress`** (derived), **`CurrentSpeed`**, **`RaceTime`**, **`CurrentLap`**, **`LapTimes`**, and **`Phase`** (**`SimulationLifecycleState`**: Created / Running / Paused / Completed). **`RaceSessionConfig.TotalLaps`** controls finish on closed tracks (**`-1`** = unlimited). Open tracks finish when progress reaches spline length.
- **`IRaceSessionRunner`** / **`RaceSessionRunner`** tick the active session from **`RaceBootstrap`** or **`CarTrackBootstrap`**.
- **`TrackSimulationFactory`** builds **`LapRaceSession`** from **`CarDefinition`**, **`TrackDefinition`**, and **`RaceSessionConfig`** (optional **`CarVariableSet`**).
- **`CarView`** on the car configures **`SplineAnimate`**, sets **`MaxSpeed`** from session speed, keeps playback paused, and assigns **`NormalizedTime`** from **`ProgressDistance / trackLength`** so the transform matches simulation progress (EditMode-friendly).
- **`TrackViewModel`** exposes **`State`** (**`SimulationLifecycleState`**) from **`LapRaceSession.Phase`** for the Race UI.

## Removed legacy stack

The baked-profile / curve-band **`TrackSimulationRunner`** path was removed earlier. Curve sampling (**`SplineCurveSampler`**), handling/acceleration pacing, cosmetic drift playback, **`RaceState`**, **`RaceLifecycle`**, and **`CarSplineDriver`** were removed or folded into **`CarView`**.

## Tests

EditMode tests: `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/` (`Game.CarSimulation.Tests`). Includes **`LapRaceSimulationTests`**.
