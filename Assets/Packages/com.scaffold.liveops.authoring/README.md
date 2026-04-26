# com.scaffold.liveops.authoring

Unity editor tools for authoring **LiveOps Remote Config** payloads from ScriptableObject **config builders** and optional **config profiles** (UGS **Settings** + **Game Overrides**).

## Contents

- **Runtime** (`Scaffold.LiveOps.Authoring`): `ConfigBuilderSO<TConfig>` / `ConfigBuilderSOBase` and `ConfigProfileSO` (with `TargetingRule`).
- **Editor** (`Scaffold.LiveOps.Authoring.Editor`): `RcEnvelope` / `GorEnvelope`, `RcSyncService`, JEXL `TargetingJexlEmitter`, and **Window → LiveOps → Configs** (Configs + Profiles) to sync and deploy `Assets/LiveOps/RemoteConfig/*.rc` and `_overrides/*.gor`.

DTO contracts live in `LiveOps/LiveOps.DTO` (`GameModuleDTO.Authoring.IConfigBuilder<T>`). Per-game builder implementations (e.g. `TrackConfigBuilderSO`) live in game assemblies such as `Game.Campaign` / `Game.Cards`.

## Workflow

1. Create or edit a `*ConfigBuilderSO` (e.g. **LiveOps → Authoring → Track Config Builder**). For a second **Variant** of the same DTO, duplicate the builder, assign a **Config Profile** (non-default) with targeting.
2. Open **Window → LiveOps → Configs** (list is grouped by `ConfigKey`). **Profiles** tab edits `ConfigProfileSO` and shows a JEXL preview. **Deploy** / **Deploy All** saves, runs `RcSyncService`, then `ugs deploy` on every touched `.rc` and `.gor`. **Pull** reapplies from disk. See `Docs/LiveOps/RemoteConfig.md`.

Do not hand-edit committed `.rc` files; regenerate them from builders so EditMode tests stay green.
