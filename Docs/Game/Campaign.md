# Campaign (Game.Campaign)

The **Game.Campaign** assembly implements a five-screen flow in a single scene: **Main → Setup → Active race → Result popup → Roguelike card pick → Main**. Root view models resolve shared services from VContainer (`ITrackService`, `CurrencyClientModule` / LiveOps gold, **`IInventoryService`** (via `InventoryClientModule`), gear engine, car simulation) instead of passing a hand-built data bag between screens.

## Responsibilities

- **`ITrackService` / `TracksClientModule`** — Current track and car, the active `LapRaceSession`, and server-backed race results via `RecordResultAsync`. Exposes **`TrackProgressModel`** via `GetTrackProgress()` and ordered `TrackEntry` list via `GetOrderedTracks()`. Roguelike gear pool is registered by **`CampaignRoguelikeInstaller`** (`RoguelikeGearPoolSO`), not by the track module.
- **`CurrencyClientModule`** (LiveOps layer) — Server-backed gold via `GetWallet("gold")` and GameApi flows that return nested `AddCurrencyResponse` / direct currency calls; see [Currency.md](../LiveOps/Currency.md). Race completion does **not** call client `AddAsync` for rewards (server grants gold in `RecordRaceResultHandler`).
- **`CampaignApplicationBootstrap`** ([`Assets/GearEngine/Scripts/App/Bootstrap/CampaignApplicationBootstrap.cs`](../../Assets/GearEngine/Scripts/App/Bootstrap/CampaignApplicationBootstrap.cs)) — Root `AppFlowRoot` for the Main scene: **Foundation → Ugs → LiveOps → Campaign** layers. Serialized fields it owns are the bootstrap-only ones whose runtime consumer lives in the same layer: `NavigationSettings` + `Transform` (consumed by `FoundationLayer`), and the four Campaign configs (`BoardRulesSO`, **`GearEngineFeatureToggleSO` (required)**, `RaceSessionDefaultsSO`, `SplineCarRunnerConfigSO`) which are passed through the `CampaignLayer` constructor. Board layout and slot capacity come from LiveOps loadout (`LoadoutClientModule` / `IGearLoadoutService`), not from a client `GearEngineStartDataSO`. After startup, opens **`MainViewModel`** from `OnReadyAsync`.
- **`CampaignLayer`** ([`CampaignLayer.cs`](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/CampaignLayer.cs)) — Registers the four gameplay configs (taken via constructor), with no construction-time board seed (real layout is hydrated from `IGearLoadoutService` in `SetupViewModel`), installs the LiveOps client modules (`CurrencyClientInstaller`, `CampaignTracksInstaller`, `CampaignInventoryInstaller`, `CampaignLoadoutInstaller`, `CardsClientInstaller`, `CampaignRoguelikeInstaller`), then installs `GearMechanicsInstaller`, `CarTrackInstaller`, `CampaignRaceSessionInstaller`, plus `CampaignGearPersistenceHookup` and `RoguelikeRollService`. Track/gear/roguelike data (`TrackDefinition` list via `liveops.tracks`, `GearCatalogSO`, `RoguelikeGearPoolSO`, etc.) are **not** loaded here — they are registered from **`FoundationLayer`** via rebaked `AssetPublisherDefinition` entries on the bootstrap. `RaceSessionConfig` (the template) is the only `RaceSessionDefaultsSO` value registered for DI. Roguelike flow: [Roguelike.md](Roguelike.md), backend: [Roguelike module](../LiveOps/Roguelike.md).

## Simulation wiring

`IRaceSessionRunner` must receive the same `LapRaceSession` instance as `ITrackService.CurrentSession` so `Update()` ticks the session (see `ActiveRaceViewModel`).

## Tests

Edit Mode tests live under `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/` and cover the five root view models.

## Scene setup (editor)

1. Use **Main Scene** with a root **`CampaignApplicationBootstrap`** (see [`Main Scene.unity`](../../Assets/GearEngine/Scenes/Main%20Scene.unity)).
2. Assign on the bootstrap component: **Navigation Settings**, **navigation view holder**, **`defaultRaceCar`**, **`layerAssetPublishers`** (rebaked `AssetPublisherDefinition` rows: label for tracks, singles for gear/roguelike pool), **Race Session Defaults** (`RaceSessionDefaultsSO`, base roguelike car stats template), **BoardRulesSO** (grid size + motor cell authoring), **GearEngineFeatureToggleSO** (required), and **SplineCarRunnerConfigSO**. See [`AddressableCatalogAddresses`](../../Assets/GearEngine/Scripts/App/Bootstrap/AddressableCatalogAddresses.cs) for legacy string keys; track assets use the `liveops.tracks` label. **`FoundationLayer`** registers these, then `CampaignLayer` ctor-injects into consumers.
3. Register **ViewConfig** assets for `MainView`, `SetupView`, `ActiveRaceView`, `ResultPopupView`, and `RoguelikeView` in **Navigation Settings** (same pattern as `RaceViewConfig`).
4. Point each ViewConfig at a prefab that has the matching `View` component and wire serialized references (track, buttons, HUD, board, inventory, etc.).

Stub prefabs are under `Assets/GearEngine/Prefabs/Campaign/`. View-only configs live in `Assets/GearEngine/Data/Campaign/ViewConfigs/`; catalogs and session/start data live in `Assets/GearEngine/Data/Campaign/Catalogs/`.

Sample catalogs: `CampaignGearCatalog.asset`, `CampaignRaceSessionDefaults.asset`, `CampaignRoguelikeGearPool.asset` (see Addressables and `TrackDefinition` assets under `Data/Track/Tracks/` for tracks).

## LiveOps coupling

Campaign progression, gold, gear inventory, board loadout, and card unlocks are backed by LiveOps modules inside the layered bootstrap (`ILiveOpsService` is registered before the Campaign layer). **`ITrackService`** is **`TracksClientModule` only** (cloud). `LocalGearLoadoutService` may remain for isolated gear tests where noted.
