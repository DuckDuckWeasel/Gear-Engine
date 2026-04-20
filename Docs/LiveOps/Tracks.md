# Tracks module (LiveOps)

## TL;DR

- **DTO**: `TrackConfig` / `TrackConfigEntry` (remote config), `TrackPersistence` (`currentTrackId`, `bestScores`), `TrackGameData`, `RecordRaceResultRequest` / `RecordRaceResultResponse` (`[UsesGameApi]`).
- **Cloud Code**: `TracksModule` (`Initialize` only); `RecordRaceResultHandler` (`IGameApiHandler`) updates best score and may advance `currentTrackId` when `score >= advanceScore`.
- **Unity**: `TracksClientModule` (`Game.Campaign`) implements `ITrackService` using `TrackCatalogSO` (track id → `TrackDefinition` / `CarDefinition`). Register via `CampaignTracksInstaller` inside `LiveOpsLayer` (same stack as `ILiveOpsService`).
- **Remote Config**: [Track.rc](../../Assets/LiveOps/RemoteConfig/Track.rc) — `entries[].id` must match `TrackDefinition` **asset name** (same as `TrackEntry.TrackId`).

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [Bootstrap.md](Bootstrap.md)
