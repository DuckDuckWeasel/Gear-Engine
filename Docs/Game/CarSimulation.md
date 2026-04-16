# Car Simulation module

Runtime code lives in `Assets/GearEngine/Scripts/Game/CarSimulation/` (`Game.CarSimulation` assembly).

## Lap race session (unified spline model)

- **`CarEntity`** remains the live stat authority. Pace and cosmetic playback read **`VariableSO`** values through **`CarVariableSet`** on **`RaceSessionConfig`** (same pattern as the previous **`CarVariableSet`** wiring).
- **`Track`** remains the scene spline authority: it owns the **`SplineContainer`**, copies authoring data from **`TrackDefinition`** on bind, and calls **`LapRaceSession.BindSpline`** so runtime sampling uses the scene spline.
- **`SplineCurveSampler`** evaluates the **closed or open** scene **`Spline`** once per sample: shared **`CurveSample`** carries **`CurveAmount`**, **`CurveDirection`**, and pose **`Position` / `Tangent` / `Up`** for simulation, visual playback, and the car driver.
- **`LapSimulation`** owns only race outcome state in **`RaceState`** (progress, speed, clock, lap count, **`LapTimes`**, **`RaceLifecycle`**). Target pace uses **`CarVariableSet.MaxStraightSpeed`** and **`CarVariableSet.Acceleration`** on **`CarEntity`** (no separate sim **`MaxSpeed`** / **`AccelerationRate`**). **`LapSimulationConfig`** keeps only race-wide tuning: **`CurveSlowdown`**, **`TotalLaps`** (**-1** = unlimited laps on closed tracks), **`HandlingNormalizationScale`** (set this near the top of your handling stat range so `handling / scale` is a meaningful 0–1 value; e.g. **100** when bag handling uses 0–100).
- **`CarVisualPlayback`** updates **`CarVisualState`** (corner effect, lateral offset, slip angle, drift flag). Cosmetic playback does not feed back into lap timing in this pass.
- **`LapRaceSession`** composes sampler, lap simulation, visual playback, and session clock. **`IRaceSessionRunner`** / **`RaceSessionRunner`** tick the active session from **`RaceBootstrap`** or **`CarTrackBootstrap`**.
- **`TrackSimulationFactory`** builds a **`LapRaceSession`** from **`CarDefinition`**, **`TrackDefinition`**, and **`RaceSessionConfig`** ( **`LapSimulationConfig`**, **`SplineSamplerConfig`**, **`CarVisualConfig`**, optional **`CarVariableSet`** ).
- **`CarSplineDriver`** is presentation-only: places the car from **`CurveSample`** + **`CarVisualState`** in **`LateUpdate`** after the runner advances the session.
- **`TrackViewModel`** mirrors **`SimulationLifecycleState`** for the Race UI ( **`Created` / `Running` / `Paused` / `Completed`** ) from **`RaceLifecycle`** plus the session clock.

## Removed legacy stack

The baked-profile / curve-band / heading-error **`TrackSimulationRunner`** path (**`BakedTrackProfile`**, **`TrackSample`**, **`TrackSimulationTuning`**, **`SimulationFrame`**, **`CarMotionState`**, **`RaceRuntimeState`**, **`TrackSimulation`**) was removed in favor of live **`SplineContainer`** sampling and the simulation vs. visual split above.

## Tests

EditMode tests: `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/` (`Game.CarSimulation.Tests`). Includes **`SplineCurveSamplerTests`** and **`LapRaceSimulationTests`**.
