# Meta scene bootstrap (LayeredScope)

The [Meta scene](../../Assets/GearEngine/Scenes/Meta.unity) uses **[`MetaApplicationBootstrap`](../../Assets/GearEngine/Scripts/App/Bootstrap/MetaApplicationBootstrap.cs)** (`Game.App.Bootstrap`), which subclasses [`ApplicationBootstrap`](../../Assets/GearEngine/Scripts/Core/LayeredScope/Runtime/ApplicationBootstrap.cs) from `Core.LayeredScope`. It installs three stacked VContainer scopes in order:

1. **[`FoundationLayer`](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs)** — Addressables, navigation, events, and `CrossLayerObjectResolver` (same responsibilities as the former `SceneFoundationScope` foundation block).
2. **[`UgsLayer`](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/UgsLayer.cs)** — `UgsInstaller`; [`Ugs`](../../Assets/Packages/com.scaffold.ugs/Runtime/Ugs.cs) implements [`IAsyncInitializable`](../../Assets/GearEngine/Scripts/Core/LayeredScope/Runtime/Contracts/IAsyncInitializable.cs) and initializes Unity Gaming Services plus anonymous sign-in.
3. **[`LiveOpsLayer`](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/LiveOpsLayer.cs)** — `CloudCodeInstaller` + `LiveOpsInstaller`; [`LiveOpsService`](../../Assets/Packages/com.scaffold.liveops/Runtime/LiveOpsService.cs) implements `IAsyncInitializable` and runs the initial `GameDataRequest`.

After `InstallAllAsync` completes, **`OnReadyAsync`** resolves [`ILiveOpsService`](../../Assets/Packages/com.scaffold.liveops/Runtime/ILiveOpsService.cs) from the top scope and logs presence of active module payloads (e.g. `CurrencyGameData`, `TrackGameData`) via the `GameModuleDTO` assemblies (server-dependent).

## Adding a layer

1. Implement [`IScopeLayer`](../../Assets/GearEngine/Scripts/Core/LayeredScope/Runtime/Contracts/IScopeLayer.cs) (and optionally [`IAsyncScopeLayer`](../../Assets/GearEngine/Scripts/Core/LayeredScope/Runtime/Contracts/IAsyncScopeLayer.cs) if you need `PrepareAsync` against the parent resolver).
2. Register services with [`.As<IAsyncInitializable>()`](../../Assets/GearEngine/Scripts/Core/LayeredScope/Runtime/Contracts/IAsyncInitializable.cs) when the layer must `await` work after the child container is built.
3. Yield the layer from [`GetInitialLayers()`](../../Assets/GearEngine/Scripts/App/Bootstrap/MetaApplicationBootstrap.cs) in the order you need; each layer’s init wave finishes before the next layer is pushed.

## Related docs

- [LayeredScope overview](../LayeredScope.md) — API and sample scene.
- [LiveOps bootstrap](../LiveOps/Bootstrap.md) — backend build, DTO copy, `.ccmr`, and Play Mode expectations.
