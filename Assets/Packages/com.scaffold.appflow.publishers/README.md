# com.scaffold.appflow.publishers

Data-driven **Addressables ScriptableObject publishers** for Scaffold AppFlow. Bake an Addressable `ScriptableObject` reference at edit time into a closed-generic registrar so runtime registration uses **no reflection**, then register the publisher on a layer's container.

## Contents

- **Runtime** (`Scaffold.AppFlow.Publishers`):
  - `IPublisherRegistrar` — edit-time baked registration for one Addressable SO publisher.
  - `AddressableScriptableObjectPublisherSO` — authoring asset that holds the `AssetReferenceT<ScriptableObject>` and the baked registrar.
  - `AddressableScriptableObjectPublisherRegistrar<T>` — closed-generic registrar baked into the SO via `[SerializeReference]`.
  - `DataDrivenAddressableScriptableObjectPublisher<T>` — runtime publisher that loads the addressable and pushes it to `ILayerPublisher` via `AssetPublisherBase<T>`.
- **Editor** (`Scaffold.AppFlow.Publishers.Editor`):
  - `AddressableScriptableObjectPublisherSOEditor` — custom inspector with **Rebuild** button.
  - `AddressableScriptableObjectPublisherSORebaker` — bake helper invoked by the inspector, the asset post-processor, and the menu.
  - `PublisherSOAssetPostprocessor` — rebakes stale assets after import.
  - `PublisherSORebakeMenu` — `Tools → Scaffold → AppFlow → Rebake All Publisher SOs`.

## Workflow

1. Create the asset: **Assets → Create → Scaffold → AppFlow → Addressable ScriptableObject Publisher**.
2. Assign an Addressable `ScriptableObject` to **Asset Reference**, then click **Rebuild** (or run **Tools → Scaffold → AppFlow → Rebake All Publisher SOs**).
3. Add the rebaked asset to the bootstrap's publisher list. The owning layer calls `publisherSo.Register(builder)` so descendants resolve the loaded asset via `ILayerPublisher` once the layer's `IAsyncInitializable` wave completes.

Publish-to-descendants rule from AppFlow still applies: the layer that publishes cannot consume in the same wave.

## Dependencies

- `com.scaffold.appflow` — `AssetPublisherBase<T>`, `ILayerPublisher`, `IAsyncInitializable`.
- `com.scaffold.addressables` — `IAddressablesAssetClient` for the runtime load.
- `com.unity.addressables` — `AssetReferenceT<ScriptableObject>` and group lookup at edit time.
- `jp.hadashikick.vcontainer` — `IContainerBuilder` for registration.
