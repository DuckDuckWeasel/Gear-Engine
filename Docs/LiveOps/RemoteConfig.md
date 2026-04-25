# LiveOps Remote Config (`.rc` authoring)

Server modules read Remote Config keys via `IRemoteConfig` (see [UnityRemoteConfig.cs](../../LiveOps/Project/Core/ModuleFetchData/Implementation/Unity/UnityRemoteConfig.cs)). This document is the **source-of-truth** for how those keys are deployed from the Unity Editor.

Authoring workflow (builders, deploy, runtime matching) is summarized in [AuthoringPipeline.md](AuthoringPipeline.md).

## Location

Per-module Remote Config files live under:

- [Assets/LiveOps/RemoteConfig/](../../Assets/LiveOps/RemoteConfig/)

Each file uses the `.rc` extension and follows the Unity Remote Config authoring shape: a single top-level JSON object with an `entries` map (see the `com.unity.remote-config` / Deployment samples). **Do not hand-edit** these files for content changes — use **Window → LiveOps → Configs** → **Deploy** (which regenerates `.rc` from builders before upload) so tests and deploys stay aligned. For file-only refresh without calling UGS, use `RcSyncService.Sync` from editor code. The historical `$schema` URL is omitted in generated files; Deployment does not require it.

## Key naming

Each `.rc` file should expose **one** top-level Remote Config key under `entries` that matches the DTO / module name:

| File | Remote Config key | Cloud Code consumer |
|------|---------------------|------------------------|
| `Currency.rc` | `CurrencyConfig` | `CurrencyModule` |
| `Track.rc` | `TrackConfig` | `TracksModule` |
| `Card.rc` | `CardConfig` | `CardsModule` |
| `Loadout.rc` | `LoadoutConfig` | `LoadoutModule` |
| `Inventory.rc` | `InventoryConfig` | `InventoryModule` |
| `Roguelike.rc` | `RoguelikeConfig` | `RoguelikeModule` |

Adding a new module: add a DTO config type, add `ConfigKey` in the module, add a `ConfigBuilderSO<TConfig>` asset, then **Deploy** (or `RcSyncService.Sync`) to regenerate `<Module>.rc`.

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

## Format note

Author `entries` as in the official sample (object values for JSON blobs). Do **not** use a nested `{ "type": "json", "value": … }` wrapper unless your pipeline explicitly expects that schema; the built-in importer derives `JSON` type from object values.
