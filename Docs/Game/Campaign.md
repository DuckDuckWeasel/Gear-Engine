# Campaign (Game.Campaign)

The **Game.Campaign** assembly implements a five-screen flow in a single scene: **Main → Setup → Active race → Result popup → Roguelike card pick → Main**. Root view models resolve shared services from VContainer (`ITrackService`, `CurrencyClientModule` / LiveOps gold, `IOwnedGearInventoryService`, gear engine, car simulation) instead of passing a hand-built data bag between screens.

## Responsibilities

- **`ITrackService` / `TracksClientModule`** — Current track and car, the active `LapRaceSession`, roguelike card pool, and server-backed race results via `RecordResultAsync`. Exposes **`TrackProgressModel`** via `GetTrackProgress()` and ordered `TrackEntry` list via `GetOrderedTracks()`.
- **`CurrencyClientModule`** (LiveOps layer) — Server-backed gold via `GetWallet("gold")` and GameApi flows that return nested `AddCurrencyResponse` / direct currency calls; see [Currency.md](../LiveOps/Currency.md). Race completion does **not** call client `AddAsync` for rewards (server grants gold in `RecordRaceResultHandler`).
- **`CampaignApplicationBootstrap`** ([`Assets/GearEngine/Scripts/App/Bootstrap/CampaignApplicationBootstrap.cs`](../../Assets/GearEngine/Scripts/App/Bootstrap/CampaignApplicationBootstrap.cs)) — Root `ApplicationBootstrap` for the Main scene: **Foundation → UGS → LiveOps → Campaign** layers. References catalog and start-data **assets** (`TrackCatalogSO`, `GearCatalogSO`, `GearEngineStartDataSO`, `RaceSessionDefaultsSO`, board rules, feature toggles, spline config). After startup, opens **`MainViewModel`** from `OnReadyAsync`.
- **`CampaignLayer`** — Installs `GearMechanicsInstaller`, `CarTrackInstaller`, and `CampaignRaceSessionInstaller` after LiveOps client modules have registered.
- **LiveOps client installers** — `CampaignTracksInstaller`, `CampaignGearCatalogInstaller`, `CampaignLoadoutInstaller`, `CampaignInventoryInstaller`, plus `CardsClientInstaller`, run inside **`LiveOpsLayer`** so `IAsyncInitializable` hydration shares the same stack as `LiveOpsService`.

## Simulation wiring

`IRaceSessionRunner` must receive the same `LapRaceSession` instance as `ITrackService.CurrentSession` so `Update()` ticks the session (see `ActiveRaceViewModel`).

## Tests

Edit Mode tests live under `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/` and cover the five root view models.

## Scene setup (editor)

1. Use **Main Scene** with a root **`CampaignApplicationBootstrap`** (see [`Main Scene.unity`](../../Assets/GearEngine/Scenes/Main%20Scene.unity)).
2. Assign **Navigation Settings**, **navigation view holder**, **Track Catalog**, **Gear Catalog**, **Gear Engine Start Data** (`GearEngineStartDataSO`), **Race Session Defaults** (`RaceSessionDefaultsSO`), **BoardRulesSO**, optional **GearEngineFeatureToggleSO**, and **SplineCarRunnerConfigSO** (all asset references on the bootstrap component).
3. Register **ViewConfig** assets for `MainView`, `SetupView`, `ActiveRaceView`, `ResultPopupView`, and `RoguelikeView` in **Navigation Settings** (same pattern as `RaceViewConfig`).
4. Point each ViewConfig at a prefab that has the matching `View` component and wire serialized references (track, buttons, HUD, board, inventory, etc.).

Stub prefabs and ViewConfigs are under `Assets/GearEngine/Prefabs/Campaign/` and `Assets/GearEngine/Data/Campaign/` for reference; replace or extend them for production UI.

Campaign catalog/start-data samples: `Assets/GearEngine/Data/Campaign/CampaignTrackCatalog.asset`, `CampaignGearCatalog.asset`, `CampaignGearStartData.asset`, `CampaignRaceSessionDefaults.asset`.

## LiveOps coupling

Campaign progression, gold, gear inventory, board loadout, and card unlocks are backed by LiveOps modules inside the layered bootstrap (`ILiveOpsService` is registered before the Campaign layer). **`ITrackService`** is **`TracksClientModule` only** (cloud). `LocalGearLoadoutService` may remain for isolated gear tests where noted.
