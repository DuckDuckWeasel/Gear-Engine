# Campaign (Game.Campaign)

The **Game.Campaign** assembly implements a five-screen flow in a single scene: **Main → Setup → Active race → Result popup → Roguelike card pick → Main**. Root view models resolve shared services from VContainer (`ITrackService`, `IWalletService`, gear engine, car simulation) instead of passing a hand-built data bag between screens.

## Responsibilities

- **`ITrackService` / `LocalTrackService`** — Current track and car, the active `LapRaceSession`, roguelike card pool, result recording (stub log), and campaign progression (stub index). Exposes **`TrackProgressModel`** via `GetTrackProgress()` and ordered `TrackEntry` list via `GetOrderedTracks()`.
- **`IWalletService` / `LocalWalletService`** — In-memory gold; credited after each race. Exposes **`WalletModel`** via `GetWallet()`; spending uses `TrySpendGold(int)` (returns `false` when insufficient).
- **`CampaignScope`** — Extends `SceneFoundationScope`, installs `GearMechanicsInstaller`, `CarTrackInstaller`, registers track and wallet services, and registers `CampaignBootstrap`.
- **`CampaignBootstrap`** — On startup, creates the initial `LapRaceSession` via `TrackSimulationFactory`, assigns it to `ITrackService` and `IRaceSessionRunner`, then opens `MainViewModel`.

## Simulation wiring

`IRaceSessionRunner` must receive the same `LapRaceSession` instance as `ITrackService.CurrentSession` so `Update()` ticks the session (see `CampaignBootstrap` and `ActiveRaceViewModel`).

## Tests

Edit Mode tests live under `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/` and cover the five root view models.

## Scene setup (editor)

1. Duplicate **Race Scene** (or create a new scene with a root object that hosts the scope).
2. Replace **RaceScope** with **CampaignScope** on the scene root (or add `CampaignScope` and remove the old scope).
3. Assign **Navigation Settings**, **navigation view holder**, **BoardConfigSO**, optional **GearEngineFeatureToggleSO**, **CampaignBootstrap**, **TrackEntry** array (each entry: `TrackDefinition` + `CarDefinition`), and optional **roguelike card pool** (`GearConfig[]`).
4. Register **ViewConfig** assets for `MainView`, `SetupView`, `ActiveRaceView`, `ResultPopupView`, and `RoguelikeView` in **Navigation Settings** (same pattern as `RaceViewConfig`).
5. Point each ViewConfig at a prefab that has the matching `View` component and wire serialized references (track, buttons, HUD, board, inventory, etc.).

Stub prefabs and ViewConfigs are under `Assets/GearEngine/Prefabs/Campaign/` and `Assets/GearEngine/Data/Campaign/` for reference; replace or extend them for production UI.

## Stubs vs. future LiveOps

`LocalTrackService` and `LocalWalletService` are intentionally in-memory and logging-only where persistence would go, so they can be swapped for LiveOps-backed implementations without changing the view models.
