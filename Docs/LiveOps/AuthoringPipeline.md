# LiveOps config authoring pipeline

This document describes the end-to-end flow from Unity assets to Remote Config and back at runtime. Server DTOs live under `LiveOps/LiveOps.DTO/`; Cloud Code modules read them via `IRemoteConfig` ([UnityRemoteConfig.cs](../../LiveOps/Project/Core/ModuleFetchData/Implementation/Unity/UnityRemoteConfig.cs)).

## Flow

```mermaid
flowchart LR
  subgraph author [Author in Unity]
    catalogs[Catalogs and builders]
    catalogs --> builderSO[ConfigBuilderSO TConfig]
    builderSO --> dto[(DTO payload)]
  end
  subgraph publish [Publish]
    window[Window / LiveOps / Config Deployment]
    rc[Assets/LiveOps/RemoteConfig/*.rc]
    ugs[(UGS Remote Config)]
    dto --> window
    window --> rc
    rc --> ugs
  end
  subgraph runtime [Runtime]
    cc[Cloud Code modules]
    client[Game client modules]
    assets[Scene catalog ScriptableObjects]
    ugs --> cc
    cc --> client
    assets --> client
  end
```

1. **Author** — Designers maintain ScriptableObject catalogs (e.g. `TrackCatalogSO`, `CardCatalogSO`) and one `ConfigBuilderSO<TConfig>` per Remote Config key. The builder reads asset-backed entries plus any **asset-independent** fields (e.g. card base cost) and builds the DTO.
2. **Publish** — **Window → LiveOps → Config Deployment** runs `Build()` and writes `Assets/LiveOps/RemoteConfig/<Name>.rc` (no `$schema` line; `entries` wrapper only). Then **Window → Deployment** pushes to UGS ([RemoteConfig.md](RemoteConfig.md)).
3. **Download** — Unchanged: Cloud Code `GameModule` types call `remoteConfig.Get(context, ConfigKey, new TConfig())`.
4. **Match config to assets** — Client modules keep an **injected** catalog (`TrackCatalogSO`, etc.) and resolve config ids with the catalog’s existing APIs (`GetTrack`, `TryGet`, …). There is no shared engine `IAssetResolver` type; the pattern is convention-only (see `TracksClientModule`).

## Add a new module

1. Add DTO type under `LiveOps/LiveOps.DTO/Modules/<Area>/` with Newtonsoft `JsonProperty` attributes.
2. Add `ConfigKey` (usually `nameof(MyConfig)`) on the Cloud Code `GameModule` implementation.
3. Add `public const string ConfigKey` in the client if you expose it.
4. Create `MyConfigBuilderSO : ConfigBuilderSO<MyConfig>` in the appropriate game assembly (`Game.Campaign`, `Game.Cards`, …) and a **Create Asset Menu** entry.
5. Open **Window → LiveOps → Config Deployment** → **Sync** for that row, then commit the updated `.rc`.
6. Add **Window → Deployment** step for your environment when promoting.

## Package reference

Implementation lives in [com.scaffold.liveops.authoring](../../Assets/Packages/com.scaffold.liveops.authoring/README.md) (`ConfigBuilderSO`, `RcEnvelope`, deployment window).

## Default builder assets (repo)

Checked-in defaults live under [Assets/GearEngine/Data/LiveOps/Authoring/](../../Assets/GearEngine/Data/LiveOps/Authoring/) (see `README.md` there). **Window → LiveOps → Config Deployment** discovers all `ConfigBuilderSOBase` assets project-wide, including these.

## Tests

- **dotnet**: `LiveOps.Tests` — `ConfigModuleKeyContractTests` (server `ConfigKey` vs DTO name).
- **Unity EditMode**: `Game.Campaign.Tests` — `LiveOpsConfigBuilderAndRcTests` (default builder output matches committed `.rc` JSON via `JToken.DeepEquals`).

Regenerate `.rc` files after changing defaults in a builder (`Sync All Out-of-Sync`).
