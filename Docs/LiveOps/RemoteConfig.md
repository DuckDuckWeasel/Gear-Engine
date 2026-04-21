# LiveOps Remote Config (`.rc` authoring)

Server modules read Remote Config keys via `IRemoteConfig` (see [UnityRemoteConfig.cs](../../LiveOps/Project/Core/ModuleFetchData/Implementation/Unity/UnityRemoteConfig.cs)). This document is the **source-of-truth** for how those keys are deployed from the Unity Editor.

Authoring workflow (builders, Sync, runtime matching) is summarized in [AuthoringPipeline.md](AuthoringPipeline.md).

## Location

Per-module Remote Config files live under:

- [Assets/LiveOps/RemoteConfig/](../../Assets/LiveOps/RemoteConfig/)

Each file uses the `.rc` extension and follows the Unity Remote Config authoring shape: a single top-level JSON object with an `entries` map (see the `com.unity.remote-config` / Deployment samples). **Do not hand-edit** these files for content changes — use **Window → LiveOps → Config Deployment** → **Sync** (or **Sync All Out-of-Sync**) from `ConfigBuilderSO` assets so tests and deploys stay aligned. The historical `$schema` URL is omitted in generated files; Deployment does not require it.

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

Adding a new module: add a DTO config type, add `ConfigKey` in the module, add a `ConfigBuilderSO<TConfig>` asset, **Sync** to regenerate `<Module>.rc`, deploy.

## Payload shapes (minimal server-authoritative fields)

| Key | JSON shape (under the config key) |
|-----|-------------------------------------|
| `TrackConfig` | `entries[]`: `{ "id", "baseReward", "bands": [{ "maxSec", "r" }] }`. Race time is **seconds**; first band with `raceTime <= maxSec` (after sorting by `maxSec`) sets reward; `baseReward` when no band matches. |
| `CardConfig` | `catalog` (string ids), `baseCost`, `costPerPurchaseGrowth`. Purchases always spend **`gold`** (no `currencyId`). |
| `InventoryConfig` | `baseSlots` (int default for client + docs). |
| `LoadoutConfig` | `baseSlots` (int). |
| `RoguelikeConfig` | `gearPool` (string gear ids), `optionsPerRoll` (int). |

## Packages

- `com.scaffold.liveops.authoring` (in-repo) — **Window → LiveOps → Config Deployment** writes `.rc` from `ConfigBuilderSO` assets ([README](../../Assets/Packages/com.scaffold.liveops.authoring/README.md)).
- `com.unity.remote-config` — `.rc` ScriptedImporter and Deployment integration.
- `com.unity.services.deployment` — **Window → Deployment** uploads `.rc` assets to the linked UGS environment.

Declared in [Packages/manifest.json](../../Packages/manifest.json).

## Deploy from the Editor

1. Link the project: **Edit → Project Settings → Services** (Unity Gaming Services project and environment).
2. Open **Window → Deployment**.
3. Select the `.rc` files (or the `RemoteConfig` folder) to deploy.
4. Confirm the target environment matches the linked Services settings.
5. **Deploy** and wait for success per asset.

Environment selection follows the linked UGS project/environment in the Editor, not separate `*.dev.rc` / `*.prod.rc` files.

## Troubleshooting: `TrackConfig` has 0 entries

Editing **`CampaignTrackCatalog`** (or any `TrackCatalogSO`) does **not** update UGS by itself. The pipeline is: catalog → **`TrackConfigBuilder`** asset → **Sync** writes `Track.rc` → **Window → Deployment** pushes to Remote Config.

1. Select **`Assets/GearEngine/Data/LiveOps/Authoring/TrackConfigBuilder.asset`** and confirm **Catalog** references your catalog (e.g. `Assets/GearEngine/Data/Campaign/Catalogs/CampaignTrackCatalog`).
2. Each catalog row must have a **Track** reference. The exported id is the **track asset name** (`TrackDefinition.name`).
3. Run **Window → LiveOps → Config Deployment** → **Sync** on `TrackConfig` (or Sync All). Open `Assets/LiveOps/RemoteConfig/Track.rc` and confirm `entries.TrackConfig.entries` is non-empty.
4. Run **Window → Deployment** so the cloud environment matches the repo. The Dashboard shows the deployed JSON; stale environments still look like 0 tracks until you deploy.

If builds still emit 0 rows, check the Unity **Console** for `[TrackConfigBuilder]` warnings (missing catalog, missing track ref, or empty id).

## Verify

After deploy:

1. Optional: confirm keys in the Unity Dashboard (Remote Config) for that environment.
2. Open [Meta.unity](../../Assets/GearEngine/Scenes/Meta.unity) and press Play. A successful init logs `[Meta] LiveOps raw payloads: …` with module payloads driven by Remote Config.

## CI / CLI (optional)

The Unity Gaming Services CLI (`ugs deploy`) can deploy the same `.rc` files; this repo uses Editor-first workflow. Add a scripted step later if you need non-Editor promotion (e.g. staging → production).

## Format note

Author `entries` as in the official sample (object values for JSON blobs). Do **not** use a nested `{ "type": "json", "value": … }` wrapper unless your pipeline explicitly expects that schema; the built-in importer derives `JSON` type from object values.
