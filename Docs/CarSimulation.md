# Car Simulation module

Runtime and test-scene wiring for spline-based car movement. Code lives under `Assets/Scripts/Game/CarSimulation/`.

## Composition

- **`CarTrackInstaller`** — Registers feature services only (`ITrackSimulationService` / `TrackSimulationService`). No scene objects or definition assets.
- **`CarTrackScope`** — `LifetimeScope` host. Serialized reference: **`CarTrackBootstrap`** only. Calls `CarTrackInstaller` and registers the bootstrap component as `IInitializable` when assigned.
- **`CarTrackBootstrap`** — Scene `MonoBehaviour` implementing `IInitializable`. Holds serialized `Track`, `TrackDefinition`, and `CarDefinition`. Injects `ITrackSimulationService`. Intended for the dedicated test scene auto-start.

## Control flow

1. Caller (bootstrap or production code) invokes **`CreateSimulation(CarDefinition, TrackDefinition)`** on `ITrackSimulationService`.  
   This creates a runtime **`CarEntity`** via **`EntityInstanceFactory.CreateInstance`** (Scaffold.Entities) and a **`TrackViewModel`** with **`IsRunning == false`**. No prefabs are spawned here.
2. Caller binds the scene **`Track`** with **`Track.Bind(service.TrackViewModel)`**.  
   `Track` (`ViewComponent<TrackViewModel>`) applies the track spline from the view model, then spawns exactly one **`CarView`** from **`CarDefinition.CarPrefab`** and calls **`CarView.Initialize`**.
3. Caller starts playback with **`ToggleSimulation(true)`**.  
   `TrackViewModel.IsRunning` becomes `true`; the MVVM binding notifies **`Track`**, which forwards **`OnRunningChanged`** to the local **`CarView`** → **`CarSplineDriver.Play()`**.
4. Pause: **`ToggleSimulation(false)`** (still resumable until completed).  
   Stop for good: **`CompleteSimulation()`** (terminal; further **`ToggleSimulation(true)`** throws).

The service keeps a richer internal lifecycle (`Created` / `Running` / `Paused` / `Completed`). Only **`IsRunning`** is exposed on the view model.

## Public surfaces

- **`TrackViewModel`** — Single shared/public view model for this feature; inherits **`Scaffold.MVVM.ViewModel`**, uses observable properties for **`Car`**, **`IsRunning`**, and optional UI mirrors (**`CurrentSpeed`**, **`TrackProgress01`**). Observable setters are not public; the simulation service calls **`SetRunning`**. The view model may subscribe to **`CarEntity`** for mirrors.
- **`CarEntity`** — Serializable wrapper around **`EntityInstance<CarDefinition>`**; runtime source of truth for attributes. Created only via **`CarEntity.Create`** / the service path, not by instantiating the car prefab.
- **`CarView`** / **`CarSplineDriver`** — Presentation only. **`CarSplineDriver`** reads movement data from **`CarEntity`** and the spline; it does not reference **`TrackViewModel`** for simulation input.

Car visuals always come from **`CarDefinition.CarPrefab`**, instantiated by **`Track`**.

## Test scene vs production

- **Test scene**: Assign **`CarTrackBootstrap`** on the scope, wire definitions and **`Track`**, and rely on VContainer **`IInitializable`** order: create simulation → bind track → toggle run.
- **Production**: Omit or leave the bootstrap unassigned; resolve **`ITrackSimulationService`** and perform the same three steps explicitly when your flow should start.

## Related tests

Editor tests: `Assets/Scripts/Game/CarSimulation/Tests/Editor/`.
