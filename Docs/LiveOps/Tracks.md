# Tracks module (LiveOps)

## TL;DR

- **DTO**: `TrackConfig` / `TrackConfigEntry` (`baseReward`, `bands` with `maxSec` + `r`), `TrackPersistence` (`currentTrackId`, `bestTimeSec`), `TrackGameData`, `RecordRaceResultRequest` (`raceTimeSec`) / `RecordRaceResultResponse` (`newBestTimeSec`, `matchedBandIndex`, `reward`, `advanced`, `nextTrackId`; `[UsesGameApi]`).
- **Cloud Code**: `TracksModule` (`Initialize` only); `RecordRaceResultHandler` updates best time, resolves band / `baseReward`, may advance `currentTrackId` when a band matches, and grants **`gold`** via nested `AddCurrencyRequest` when `reward > 0`.
- **Unity**: `TracksClientModule` (`Game.Campaign`) implements `ITrackService` using `TrackAssetIndex` (id → `TrackDefinition`; a single `CarDefinition` is injected in `FoundationLayer` as `defaultRaceCar` on `GearAppFlowRoot` / `RegisterInstance` for racing). Track definitions are loaded via the `liveops.tracks` Addressables label from `AssetPublisherDefinition` on the bootstrap. Register via `CampaignTracksInstaller` inside `LiveOpsLayer` (same stack as `ILiveOpsService`). Applies nested `AddCurrencyResponse` to `CurrencyClientModule` after a race.
- **Remote Config**: [Track.rc](../../Assets/LiveOps/RemoteConfig/Track.rc) — `entries[].id` must match `TrackDefinition` **asset name** (same as `TrackEntry.TrackId`).

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [Bootstrap.md](Bootstrap.md)
