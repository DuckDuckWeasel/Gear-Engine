# Game.Race module

Runtime code: `Assets/Scripts/Game/Race/` (`Game.Race` assembly).

## Purpose

Owns the race flow navigation: Track Preview screen → Race screen.

## Screens

- **TrackPreviewView** / `TrackPreviewViewModel` — shows track name, navigates to Race on button press.
- **RaceView** / `RaceViewModel` — shows idle track + gear board; RACE button starts `IGridManager` and `IRaceDriver` simultaneously, then disables itself.

## Scene setup

See `Assets/Scenes/RaceScene.unity`. The scene root holds a `RaceScope` LifetimeScope with a child `NavigationViewHolder` transform. Addressable prefab addresses: `Race/TrackPreviewView`, `Race/RaceView`. ViewConfig assets: `Assets/Data/Navigation/`.

## Car test scenes

`SplineTrack_TestScene` keeps automatic driving via `CarAutoStartDriver` registered on `CarTrackScope`. `RaceScene` uses `RaceScope` only (no `CarTrackScope`).

## Dependencies

- `Game.CarSimulation` — `IRaceDriver` (via `CarTrackBootstrap`), `TrackDefinition`
- `Game.GearEngine` — `IGridManager`
- `Scaffold.Navigation` — `INavigator`, `ViewConfig`, `NavigationSettings`
- `Scaffold.MVVM` — `ViewModel`, `View<T>`

## Navigation registration

`RaceNavigationInstaller` reflects into `Scaffold.Navigation.Container` so the project can load before UPM restores `com.scaffold.navigation` under `Library/PackageCache`. If registration fails at runtime, open the project in Unity once, then align the installer with the package’s `NavigationInstaller` / `AddNavigation` API if needed.
