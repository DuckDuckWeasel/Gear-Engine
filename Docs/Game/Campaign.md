# Campaign (Game.Campaign)

The **Game.Campaign** assembly implements the campaign flow in a single scene: **Main → Setup → Active race → Results → Rewards → Main**. An eligible gear reward offers Upgrade within Rewards and opens the existing Roguelike card pick. Root view models resolve shared services from VContainer (`ITrackService`, `CurrencyClientModule` / LiveOps gold, **`IInventoryService`** (via `InventoryClientModule`), gear engine, car simulation) instead of passing a hand-built data bag between screens.

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
3. Register **ViewConfig** assets for `MainView`, `SetupView`, `ActiveRaceView`, `ResultPopupView`, `RoguelikeView`, `ReceivedRewardsView`, and `RaceProgressView` in **Navigation Settings** (same pattern as `RaceViewConfig`).
4. Point each ViewConfig at a prefab that has the matching `View` component and wire serialized references (track, buttons, HUD, board, inventory, etc.).

Stub prefabs are under `Assets/GearEngine/Prefabs/Campaign/`. View-only configs live in `Assets/GearEngine/Data/Campaign/ViewConfigs/`; catalogs and session/start data live in `Assets/GearEngine/Data/Campaign/Catalogs/`.

Sample catalogs: `CampaignGearCatalog.asset`, `CampaignRaceSessionDefaults.asset`, `CampaignRoguelikeGearPool.asset` (see Addressables and `TrackDefinition` assets under `Data/Track/Tracks/` for tracks).

## LiveOps coupling

Campaign progression, gold, gear inventory, board loadout, and card unlocks are backed by LiveOps modules inside the layered bootstrap (`ILiveOpsService` is registered before the Campaign layer). **`ITrackService`** is **`TracksClientModule` only** (cloud). `LocalGearLoadoutService` may remain for isolated gear tests where noted.

## Post-race presentation

Results, Rewards, and Progress use separate `View<T>`/ViewModel pairs and Addressable ViewConfigs. `ResultPopupViewModel` remains the race-completion entry point; its existing prefab GUID is preserved. Results opens before persistence completes, and Continue waits for `RaceResultModel.PersistenceCompleted`. Results has no Upgrade action.

Rewards presents recorded gold, then one gear selection when the player finishes first OR earns at least one star. Meeting both criteria still offers one selection. Upgrade appears only on this gear reward page. Returning from a gear pick shows the awarded gear without repeating gold; skipping returns Home. It does not grant currency or inventory a second time. The final reward returns Home through `ToolbarController`. The old Progress view remains registered for compatibility but is absent from this flow; Results already displays score and stars.

`PostRaceAnimation` owns Animora playback at view open/close. The migrated clips use direct local targets, manual playback, nonzero Z scale, and a single player per animated object. Views remove button listeners on close, detach property subscriptions on unbind/destruction, and restart animations on reopening.

## Standings and score stars

`RaceStandingsModel` ranks three synthetic rivals and the player by ascending race time. `TrackDefinition.Opponents` optionally supplies rival names/times; fallback rivals are NOVA, AXEL, and BLAZE at 1.00, 1.12, and 1.24 times the track target. Exact ties favor the existing rival. These rows are local synthetic competition, not online player records.

Results shows the top three and an additional player row only when outside the podium. The opening rank comes from the previous persisted best time (fourth for a first run). `ResultStandingsView` swaps adjacent rows toward this run's rank, stops on close, and resets on reopen. An unchanged third place has three rows and no movement. It alone owns row positions; Animora owns the surrounding presentation.

Stars use score thresholds only, ordered from lowest to highest. Time never grants stars, so fourth place with three stars is valid. The reward queue is owned by its ViewModel, and views only bind values/actions and clean up subscriptions.

**Planned follow-up:** unlocking the next track will require first place; stars will affect rewards only. The current backend time-band gold/unlock contract remains authoritative until that server change is implemented. See [Race standings ExecPlan](../../Plans/RaceStandings/ExecPlan.md) for the migration and reward-policy work still required.

The installed Scaffold View package attaches its own property-change handler during binding. `PostRaceViewBindings` provides the corresponding detach operation for Results/Rewards destruction and unbinding; value updates still use the standard Bind APIs. Analyzer-required extraction keeps race setup, result persistence, ad availability, and reward navigation in named methods without changing their sequence.
