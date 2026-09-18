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

## Result popup presentation

`Campaign_ResultPopupView.prefab` owns the result-screen celebration. A low-opacity `RawImage` tiles `T_RacingFlagPattern.png` behind the result content, and `ResultPopupView` advances its UV offset with `Time.unscaledDeltaTime` so the pattern continues while gameplay time is paused.

The popup also contains a nested instance of `Confetti_directional_multicolor.prefab`. Its particle systems do not play on awake and use unscaled time. Each view binding stops and clears any previous particle state before playing one burst; unbinding or disabling the view stops and clears the hierarchy again. Keep both serialized celebration references assigned when editing the prefab.

## LiveOps coupling

Campaign progression, gold, gear inventory, board loadout, and card unlocks are backed by LiveOps modules inside the layered bootstrap (`ILiveOpsService` is registered before the Campaign layer). **`ITrackService`** is **`TracksClientModule` only** (cloud). `LocalGearLoadoutService` may remain for isolated gear tests where noted.

## Home track navigation

The first configured track starts unlocked and playable without a completed race.
An empty or outdated saved track ID is repaired to the first configured track that
has a local asset.

The home carousel contains the unlocked tracks in Remote Config order, followed by
one black preview of the next locked track. Previous/next wrap around, the counter
shows the selected position within this carousel (including the locked preview),
and both arrows hide if no tracks are unlocked or only one entry is available.
Locked previews cannot open setup. An empty catalog displays `0 of 0` and disables Play.

`TracksClientModule` keeps the chosen race track separate from the server's current
campaign track. Browsing only updates the home view model; Play selects an unlocked
track for setup, simulation, and result submission. Unlocks are restored from the
current campaign position and saved race history, including the successor of a
previously raced track, matching the server's existing advance-after-race behavior.
This retains earlier tracks after replaying a race or wrapping the campaign.
Returning home refreshes the carousel, preview, and statistics after new unlocks.

The Main View prefab owns `PreviousTrackButton`, `NextTrackButton`, and
`TrackPosition`. The view binds presentation state through MVVM, removes its arrow
listeners on unbind, and restores renderer property blocks before leaving the preview.
Focused EditMode coverage: `GearEngine.Campaign.Tests.Editor.MainTrackNavigationTests`.
