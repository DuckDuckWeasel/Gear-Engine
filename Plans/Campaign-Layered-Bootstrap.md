# Campaign layered bootstrap (completed)

## Summary

Main Scene boots with **`CampaignApplicationBootstrap`** (subclass of [`GearAppFlowRoot`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs), which subclasses `Scaffold.AppFlow.AppFlowRoot`): **Foundation → Ugs → LiveOps → Campaign**. Meta uses **`MetaApplicationBootstrap`**, also a `GearAppFlowRoot` subclass, with the same first three layers only (backend smoke test). [`GearAppFlowRoot`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs) always prepends [`FoundationLayer`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs); subclasses only implement `GetGameLayers()`.

## Layering principles

- **Simple installers**: Each `IScopeLayer.Install` only calls `new SomeInstaller().Install(builder)` and registers `AssetPublisherBase<T>` derivatives as `IAsyncInitializable` where needed. No `PrepareAsync` overrides; layers are pure registration unless you introduce a custom layer for load ordering.
- **Publish-to-descendants**: Assets loaded by an `AssetPublisherBase<T>` are exposed to the **child** layer's container via `ILayerPublisher`. The publishing layer cannot consume the asset itself; whatever needs to ctor-inject the catalog must live in a deeper layer.
- **Bootstrap-owned configs**: ScriptableObjects whose runtime consumer lives in the **same** layer that produces them (e.g. `BoardRulesSO` for grid size consumed by `GearMechanicsInstaller` in `CampaignLayer`) stay as serialized fields on the bootstrap MonoBehaviour and are passed through the layer constructor. Board layout and slot capacity are LiveOps-backed (`LoadoutClientModule`), not client start-data assets. We do not introduce extra layers just to publish-and-consume the same data.

## Layers

- **Foundation** ([`FoundationLayer`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs)) — `IAddressablesAssetClient`, `IAddressablesGateway`, `NavigationSettings`, navigation installer, no-op view-controller injector, events, and **Addressable catalog publishers** (rebaked [`AddressableScriptableObjectPublisherSO`](../Assets/GearEngine/Scripts/App/Bootstrap/Publishers/DataDriven/AddressableScriptableObjectPublisherSO.cs) list from the bootstrap). Takes `NavigationSettings` + `Transform` + optional publisher list via constructor.
- **Ugs** ([`UgsLayer`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/UgsLayer.cs)) — Installs only `UgsInstaller`. UGS init has to finish in its own layer because `CloudCodeInstaller.Install` synchronously dereferences `Unity.Services.CloudCode.CloudCodeService.Instance`, which is `null` until `UnityServices.InitializeAsync()` completes. Layers init in order — `IAsyncInitializable.InitializeAsync` for the previous layer finishes before the next layer's `Install` runs — so isolating UGS guarantees CloudCode sees a non-null instance.
- **LiveOps** ([`LiveOpsLayer`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/LiveOpsLayer.cs)) — `CloudCodeInstaller`, `LiveOpsInstaller` only. [`SequentialInLayerScheduler`](../Assets/GearEngine/Scripts/App/Bootstrap/SequentialInLayerScheduler.cs) keeps in-layer `IAsyncInitializable` order deterministic.
- **Campaign** ([`CampaignLayer`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/CampaignLayer.cs)) — Registers gameplay configs (4 SOs taken via constructor), no construction-time board seed, then installs the LiveOps client modules (which ctor-inject the catalogs published by **Foundation**), then `GearMechanicsInstaller`, `CarTrackInstaller`, `CampaignRaceSessionInstaller`, plus the roguelike roll service.

## Publishers

- **Data-driven only**: [`AddressableScriptableObjectPublisherSO`](../Assets/GearEngine/Scripts/App/Bootstrap/Publishers/DataDriven/AddressableScriptableObjectPublisherSO.cs) bakes a closed-generic [`AddressableScriptableObjectPublisherRegistrar<T>`](../Assets/GearEngine/Scripts/App/Bootstrap/Publishers/DataDriven/AddressableScriptableObjectPublisherRegistrar.cs) at edit time; runtime uses [`DataDrivenAddressableScriptableObjectPublisher<T>`](../Assets/GearEngine/Scripts/App/Bootstrap/Publishers/DataDriven/DataDrivenAddressableScriptableObjectPublisher.cs) extending `Scaffold.AppFlow.AssetPublisherBase<T>`. Assign rebaked assets on [`GearAppFlowRoot`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs) subclasses ([`CampaignApplicationBootstrap`](../Assets/GearEngine/Scripts/App/Bootstrap/CampaignApplicationBootstrap.cs) / [`MetaApplicationBootstrap`](../Assets/GearEngine/Scripts/App/Bootstrap/MetaApplicationBootstrap.cs)). Default repo assets: [`Assets/GearEngine/Data/Bootstrap/`](../Assets/GearEngine/Data/Bootstrap/). Entry addresses: [`AddressableCatalogAddresses`](../Assets/GearEngine/Scripts/App/Bootstrap/AddressableCatalogAddresses.cs) (`Catalogs/Track`, `Catalogs/Gear`, `Catalogs/RoguelikeGearPool`). `IAddressablesAssetClient` and `IAddressablesGateway` are registered in [`FoundationLayer`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs).

## Bootstrap MonoBehaviours

- [`GearAppFlowRoot`](../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs) — Abstract base: serialized `NavigationSettings`, `Transform navigationViewHolder`, `List<AddressableScriptableObjectPublisherSO> addressableCatalogPublishers`; sealed `GetInitialLayers()` yields **Foundation** then `GetGameLayers()`. Campaign sets `RequireNonEmptyAddressableCatalogPublishers` to require a non-empty publisher list.
- [`CampaignApplicationBootstrap`](../Assets/GearEngine/Scripts/App/Bootstrap/CampaignApplicationBootstrap.cs) — Serialized gameplay: `BoardRulesSO`, `GearEngineFeatureToggleSO` (required), `RaceSessionDefaultsSO`, `SplineCarRunnerConfigSO`. `GetGameLayers()` yields **Ugs → LiveOps → Campaign** (foundation prepended by base).
- [`MetaApplicationBootstrap`](../Assets/GearEngine/Scripts/App/Bootstrap/MetaApplicationBootstrap.cs) — `GetGameLayers()` yields **Ugs → LiveOps** only (foundation prepended by base).

## Addressables

Only catalogs are addressed (`Catalogs/Track`, `Catalogs/Gear`, `Catalogs/RoguelikeGearPool`). Navigation settings and gameplay configs ride the bootstrap MonoBehaviour as serialized fields.

## References

- [`Docs/Game/Campaign.md`](../Docs/Game/Campaign.md)
- [`Docs/Meta/Bootstrap.md`](../Docs/Meta/Bootstrap.md)
- [`Docs/LiveOps/Bootstrap.md`](../Docs/LiveOps/Bootstrap.md)
