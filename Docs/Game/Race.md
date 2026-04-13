# Race module

Runtime code lives in `Assets/Scripts/Game/Race/` (`Game.Race` assembly). It composes the gear board (`Game.GearEngine`) and car track simulation (`Game.CarSimulation`) behind a single screen.

## Flow

- **`RaceScope`** — `LifetimeScope` that installs gear mechanics (`GearMechanicsInstaller`), car track (`CarTrackInstaller`), Addressables, navigation, and events (same layering as `GearMechanicsScope` plus car track).
- **`RaceBootstrap`** — On startup, calls `navigation.Open(new RaceViewModel(startData))` with serialized **`RaceStartData`** (track definition, car definition, optional `GearEngineStartData` for board layout).
- **`RaceViewModel`** — Injects domain services, builds `BoardViewModel` and `TrackViewModel`, and exposes **`StartRace()`** (starts the gear engine and toggles the track simulation on).
- **`RaceView`** — Binds **`BoardView`**, **`Track`**, and a **Race** UI button. Does not auto-start the simulation on bind (unlike `CarTrackTestView`).

## Navigation and scene layout

- Register **`RaceViewModel` → `RaceView`** via a **`ViewConfig`** asset and **`Navigation Settings`** (same pattern as `GearEngineViewConfig`).
- A minimal **navigation stub prefab** may back the ViewConfig `asset` field if required; the **playable hierarchy** is expected to live in the scene.
- Place a **`RaceView`** instance under the **navigation view holder** transform assigned on `RaceScope`. Scaffold navigation resolves and binds this pre-placed view when `Open` runs.
- Use a **`World`** (or similar) branch for prefab instances of **`Track.prefab`** and **`GridBoardCollider.prefab`** (gear grid with `BoardView`). Wire **`RaceView`** serialized references to those scene objects and to the Race button.

## Tests

EditMode tests: `Assets/Scripts/Game/Race/Tests/Editor/` (`Game.Race.Tests`).
