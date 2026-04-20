# Campaign layered bootstrap (completed)

## Summary

Main Scene no longer uses `CampaignScope` (`SceneFoundationScope`) or `CampaignBootstrap`. It now boots with **`CampaignApplicationBootstrap`** (`Core.LayeredScope.ApplicationBootstrap`): **Foundation → UGS → LiveOps → Campaign**, matching the Meta pattern.

## Key changes

- **Catalogs and start data** are authored assets (`TrackCatalogSO`, `GearCatalogSO`, `GearEngineStartDataSO`, `RaceSessionDefaultsSO`) assigned on `CampaignApplicationBootstrap`, not inline arrays on a scope.
- **LiveOps game clients** register through **`LiveOpsLayer` constructor installers**: `CampaignTracksInstaller`, `CampaignGearCatalogInstaller`, `CampaignLoadoutInstaller`, `CampaignInventoryInstaller`, `CardsClientInstaller`. Meta reuses the same installer types with empty runtime catalogs.
- **`CampaignLayer`** installs `GearMechanicsInstaller`, `CarTrackInstaller`, and `CampaignRaceSessionInstaller` after LiveOps.
- **Startup navigation**: `CampaignApplicationBootstrap.OnReadyAsync` opens `MainViewModel` (replacing coroutine warmup in `CampaignBootstrap`).
- **Removed**: `CampaignScope.cs`, `CampaignBootstrap.cs`, `MetaLiveOpsClientInstaller.cs`. **`CampaignRaceSessionDefaults`** lives under `Game.Campaign.Services`.

## Build settings

`Main Scene.unity` is added to **Editor Build Settings** for stand-alone play.

## References

- [`Docs/Game/Campaign.md`](../Docs/Game/Campaign.md)
- [`Docs/Meta/Bootstrap.md`](../Docs/Meta/Bootstrap.md)
- [`Docs/LayeredScope.md`](../Docs/LayeredScope.md)
