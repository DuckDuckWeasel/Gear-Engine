# LiveOps Remote Config (`.rc` authoring)

Server modules read Remote Config keys via `IRemoteConfig` (see [UnityRemoteConfig.cs](../../LiveOps/Project/Core/ModuleFetchData/Implementation/Unity/UnityRemoteConfig.cs)). This document is the **source-of-truth** for how those keys are authored and deployed from the Unity Editor.

## Location

Per-module Remote Config files live under:

- [Assets/LiveOps/RemoteConfig/](../../Assets/LiveOps/RemoteConfig/)

Each file uses the `.rc` extension and follows the Unity Remote Config authoring format (see the `com.unity.remote-config` package sample: flat `entries` object, optional `types` for keys that must be `LONG` or `FLOAT` at the root).

## Key naming

Each `.rc` file should expose **one** top-level Remote Config key that matches the DTO / module name:

| File | Remote Config key | Cloud Code consumer |
|------|---------------------|------------------------|
| `Ads.rc` | `AdsConfig` | `AdsService` |
| `Currency.rc` | `CurrencyConfig` | `CurrencyModule` |
| `Global.rc` | `GlobalConfigData` | `GlobalConfigModule` |
| `Gold.rc` | `GoldConfig` | `GoldModule` |
| `Level.rc` | `LevelConfig` | `LevelService` |

Adding a new module: add a DTO config type, add `ConfigKey` in the module, create `<Module>.rc` with that key, deploy.

## Packages

- `com.unity.remote-config` — authoring and Deployment integration.
- `com.unity.services.deployment` — **Window → Deployment** uploads `.rc` assets to the linked UGS environment.

Declared in [Packages/manifest.json](../../Packages/manifest.json).

## Deploy from the Editor

1. Link the project: **Edit → Project Settings → Services** (Unity Gaming Services project and environment).
2. Open **Window → Deployment**.
3. Select the `.rc` files (or the `RemoteConfig` folder) to deploy.
4. Confirm the target environment matches the linked Services settings.
5. **Deploy** and wait for success per asset.

Environment selection follows the linked UGS project/environment in the Editor, not separate `*.dev.rc` / `*.prod.rc` files.

## Verify

After deploy:

1. Optional: confirm keys in the Unity Dashboard (Remote Config) for that environment.
2. Open [Meta.unity](../../Assets/GearEngine/Scenes/Meta.unity) and press Play. A successful init logs `[Meta] LiveOps ready…` with module payloads driven by Remote Config.

## CI / CLI (optional)

The Unity Gaming Services CLI (`ugs deploy`) can deploy the same `.rc` files; this repo uses Editor-first workflow. Add a scripted step later if you need non-Editor promotion (e.g. staging → production).

## Format note

Author `entries` as in the official sample (object values for JSON blobs). Do **not** use a nested `{ "type": "json", "value": … }` wrapper unless your pipeline explicitly expects that schema; the built-in importer derives `JSON` type from object values.
