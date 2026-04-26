# LiveOps Remote Config (`.rc` authoring)

Server modules read Remote Config keys via `IRemoteConfig` (see [UnityRemoteConfig.cs](../../LiveOps/Deploy/Core/LiveOps.Core/DataCache/Implementation/Unity/UnityRemoteConfig.cs)). This document is the **source-of-truth** for how those keys are deployed from the Unity Editor.

Authoring workflow (builders, deploy, runtime matching) is summarized in [AuthoringPipeline.md](AuthoringPipeline.md).

## Location

Authoring files live under:

- [Assets/LiveOps/RemoteConfig/](../../Assets/LiveOps/RemoteConfig/) — default **Settings** per key: `<Stem>.rc`
- `Assets/LiveOps/RemoteConfig/_overrides/` — **Game Overrides** (one `.gor` per non-default profile, schema `game-overrides.schema.json`)

Each `.rc` follows the Unity Remote Config shape: top-level `_contentHash`, `_deployedAt` (authoring metadata), and an `entries` map (see `com.unity.services.deployment`). **Do not hand-edit** content — use **Window → LiveOps → Configs** → **Deploy** (regenerates from builders) so tests and deploys stay aligned. For file-only refresh without UGS, use `RcSyncService.SyncForBuilder` / `SyncAll`. The historical `$schema` URL is omitted for `.rc`; `.gor` includes the official Game Overrides schema URL.

## Key naming

Each default **Settings** `.rc` exposes **one** key under `entries` that matches the DTO / module name. Multiple **Variants** of the same key are allowed if they use different **profiles** (see **Profiles** tab in the window); non-default Variants do not get their own `.rc` — they are merged into that profile’s `.gor`.

| File | Remote Config key | Cloud Code consumer |
|------|---------------------|------------------------|
| `Currency.rc` | `CurrencyConfig` | `CurrencyModule` |
| `Track.rc` | `TrackConfig` | `TracksModule` |
| `Card.rc` | `CardConfig` | `CardsModule` |
| `Loadout.rc` | `LoadoutConfig` | `LoadoutModule` |
| `Inventory.rc` | `InventoryConfig` | `InventoryModule` |
| `Roguelike.rc` | `RoguelikeConfig` | `RoguelikeModule` |

Adding a new module: add a DTO config type, add `ConfigKey` in the module, add a `ConfigBuilderSO<TConfig>` asset, then **Deploy** (or `RcSyncService`) to regenerate `<Module>.rc`.

## Payload shapes (minimal server-authoritative fields)

| Key | JSON shape (under the config key) |
|-----|-------------------------------------|
| `TrackConfig` | `entries[]`: `{ "id", "baseReward", "bands": [{ "maxSec", "r" }] }`. Race time is **seconds**; first band with `raceTime <= maxSec` (after sorting by `maxSec`) sets reward; `baseReward` when no band matches. |
| `CardConfig` | `catalog` (string ids), `baseCost`, `costPerPurchaseGrowth`. Purchases always spend **`gold`** (no `currencyId`). |
| `InventoryConfig` | `baseSlots` (int default for client + docs). |
| `LoadoutConfig` | `baseSlots` (int). |
| `RoguelikeConfig` | `gearPool` (string gear ids), `optionsPerRoll` (int). |

## Packages

- `com.scaffold.liveops.authoring` (in-repo) — **Window → LiveOps → Configs** writes `.rc` from `ConfigBuilderSO` assets and can deploy them ([README](../../Assets/Packages/com.scaffold.liveops.authoring/README.md)).
- `com.unity.remote-config` — `.rc` ScriptedImporter and dashboard / fetch integration.
- `com.unity.services.deployment` — provides the Deployment API used for programmatic deploy from the Configs window (with `ugs deploy` CLI fallback).

Declared in [Packages/manifest.json](../../Packages/manifest.json).

## Deploy from the Editor

1. Link the project: **Edit → Project Settings → Services** (Unity Gaming Services project and environment).
2. Open **Window → LiveOps → Configs**.
3. Select a config (or use **Deploy All**). Click **Deploy** / **Deploy All**.
4. Confirm the toolbar shows the expected environment id (from the linked Services / deployment environment).

The window uses `com.unity.services.deployment` programmatically first; if that fails, ensure the **Unity Gaming Services CLI** (`ugs`) is on your PATH so `ugs deploy <path-to-rc>` can run as a fallback.

Environment selection follows the linked UGS project/environment in the Editor, not separate `*.dev.rc` / `*.prod.rc` files.

## Troubleshooting: `TrackConfig` has 0 entries

Editing track **`TrackDefinition`** assets (or the `trackDefinitions` list on `TrackConfigBuilder`) does **not** update UGS by itself. The pipeline is: those assets → **`TrackConfigBuilder`** asset → **Deploy** in **Window → LiveOps → Configs** regenerates `Track.rc` from the builder, then pushes it to Remote Config.

1. Select **`Assets/GearEngine/Data/LiveOps/Authoring/TrackConfigBuilder.asset`** and confirm **`trackDefinitions`** lists the same `TrackDefinition` assets you publish with the `liveops.tracks` label.
2. Each catalog row must have a **Track** reference. The exported id is the **track asset name** (`TrackDefinition.name`).
3. Run **Window → LiveOps → Configs** → **Deploy** for `TrackConfig` (writes `Track.rc`, then uploads). Open `Assets/LiveOps/RemoteConfig/Track.rc` to confirm `entries.TrackConfig.entries` is non-empty; the Dashboard should show the same payload after a successful deploy (stale environments look like 0 tracks until then). To refresh only the file on disk without UGS, use `RcSyncService.Sync` instead.

If builds still emit 0 rows, check the Unity **Console** for `[TrackConfigBuilder]` warnings (missing catalog, missing track ref, or empty id).

## Verify

After deploy:

1. Optional: confirm keys in the Unity Dashboard (Remote Config) for that environment.
2. Open [Meta.unity](../../Assets/GearEngine/Scenes/Meta.unity) and press Play. A successful init logs `[Meta] LiveOps raw payloads: …` with module payloads driven by Remote Config.

## CI / CLI (optional)

The Unity Gaming Services CLI (`ugs deploy`) can deploy the same `.rc` files; this repo uses Editor-first workflow. Add a scripted step later if you need non-Editor promotion (e.g. staging → production).

## Game Overrides (`.gor`)

- Create **LiveOps → Authoring → Config Profile**, uncheck **Is default**, set **Profile id**, and **Targeting** (platforms, app version, tags, schedule, raw JEXL, optional **Audience targets** for schema).
- On each `ConfigBuilderSO` for that slice, assign the same **Config Profile** (non-default Variants are listed under the config key in **Window → LiveOps → Configs**).
- **Deploy** writes `RemoteConfig.Entries` with JSON **values as strings** (per UGS Game Overrides schema). Server-side JEXL uses `app.version`, `unity.platform`, `user.*` custom keys (`user.tag`, `user.bucket` for rollout), etc. Cloud Code requests settings with `RequestAttributes` so rules can run.

## Format note (`.rc`)

Author `entries` as in the official sample (object values for JSON blobs). Do **not** use a nested `{ "type": "json", "value": … }` wrapper unless your pipeline explicitly expects that schema; the built-in importer derives `JSON` type from object values.
