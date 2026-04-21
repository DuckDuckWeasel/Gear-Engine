# com.scaffold.liveops.authoring

Unity editor tools for authoring **LiveOps Remote Config** (`.rc`) payloads from ScriptableObject **config builders**.

## Contents

- **Runtime** (`Scaffold.LiveOps.Authoring`): `ConfigBuilderSO<TConfig>` / `ConfigBuilderSOBase` — base types for per-module builder assets.
- **Editor** (`Scaffold.LiveOps.Authoring.Editor`): `RcEnvelope` JSON writer and **Window → LiveOps → Config Deployment** to sync `Assets/LiveOps/RemoteConfig/*.rc`.

DTO contracts live in `LiveOps/LiveOps.DTO` (`GameModuleDTO.Authoring.IConfigBuilder<T>`). Per-game builder implementations (e.g. `TrackConfigBuilderSO`) live in game assemblies such as `Game.Campaign` / `Game.Cards`.

## Workflow

1. Create or edit a `*ConfigBuilderSO` asset (e.g. **LiveOps → Authoring → Track Config Builder**).
2. Open **Window → LiveOps → Config Deployment** → **Sync** or **Sync All Out-of-Sync**.
3. Deploy with **Window → Deployment** to the linked UGS environment (see `Docs/LiveOps/RemoteConfig.md`).

Do not hand-edit committed `.rc` files; regenerate them from builders so EditMode tests stay green.
