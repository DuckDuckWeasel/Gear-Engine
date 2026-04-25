# com.scaffold.appflow.publishers

Data-driven **asset publishers** for Scaffold AppFlow. A row (`AssetPublisherDefinition`) holds a `SerializeReference` `IAssetPublisherSource` and an edit-time-baked `IPublisherRegistrar` so the owning layer can call `def.Register(builder)` with minimal reflection in production.

## Runtime (`Scaffold.AppFlow.Publishers`)

- `IAssetPublisherDefinitionHost` / `IAssetPublisherSource` / `IPublisherRegistrar` / `AssetPublisherDefinition`
- **Direct** sources: `DirectAssetSource`, `DirectAssetListSource` and matching publishers + registrars (in-process lists or single assets)

**Editor** (`Scaffold.AppFlow.Publishers.Editor`):

- `AssetPublisherDefinitionDrawer` — type dropdown for `IAssetPublisherSource`, per-row rebake
- `AssetPublisherRebakeMenu` — **Tools → Scaffold → AppFlow → Rebake all publishers** (walks `GearAppFlowRoot` hosts in consuming projects)

## Addressables add-on

Use `com.scaffold.appflow.publishers.addressables` for:

- `AddressableSingleSource` + `AddressableSinglePublisher` / `AddressableSinglePublisherRegistrar<T>`
- `AddressableLabelSource` + `AddressableLabelPublisher<T>` / `AddressableLabelPublisherRegistrar<T>` (typed `PublishMany` by label)
- `Editor/AddressableBakeUtility` — group entry validation and bake

`Scaffold.AppFlow.Publishers` has **no** reference to Unity Addressables; the Addressables package references this assembly.

## Workflow

1. On your `IAssetPublisherDefinitionHost` (e.g. `GearAppFlowRoot`), add `AssetPublisherDefinition` rows, pick a source, **Rebake** (or use **Rebake all**).
2. The baked registrar is embedded; at runtime, `Register` can fall back to `IAssetPublisherSource.TryCreateRuntimeBakedRegistrar()` when a scene was not saved with a bake.
3. `FoundationLayer` (or your layer) iterates `layerAssetPublishers` and calls `def.Register(builder)`.

## Dependencies

- `com.scaffold.appflow` — `ILayerPublisher`, `IAsyncInitializable`, `ILayerPublisher` integration
- `jp.hadashikick.vcontainer` — `IContainerBuilder`

For Addressables support, add `com.scaffold.appflow.publishers.addressables`, `com.scaffold.addressables`, and `com.unity.addressables`.
