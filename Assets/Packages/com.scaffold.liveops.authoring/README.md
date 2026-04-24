# com.scaffold.liveops.authoring

Unity editor tools for authoring **LiveOps Remote Config** (`.rc`) payloads from ScriptableObject **config builders**.

## Contents

- **Runtime** (`Scaffold.LiveOps.Authoring`): `ConfigBuilderSO<TConfig>` / `ConfigBuilderSOBase` — base types for per-module builder assets.
- **Editor** (`Scaffold.LiveOps.Authoring.Editor`): `RcEnvelope` JSON writer and **Window → LiveOps → Configs** to sync and deploy `Assets/LiveOps/RemoteConfig/*.rc`.

DTO contracts live in `LiveOps/LiveOps.DTO` (`GameModuleDTO.Authoring.IConfigBuilder<T>`). Per-game builder implementations (e.g. `TrackConfigBuilderSO`) live in game assemblies such as `Game.Campaign` / `Game.Cards`.

## Workflow

1. Create or edit a `*ConfigBuilderSO` asset (e.g. **LiveOps → Authoring → Track Config Builder**).
2. Open **Window → LiveOps → Configs** → edit inline, then **Deploy** or **Deploy All** (each run saves builders, regenerates `.rc` files, then uploads). Use **Pull** to reapply values from disk back onto a builder. See `Docs/LiveOps/RemoteConfig.md`.

Do not hand-edit committed `.rc` files; regenerate them from builders so EditMode tests stay green.
