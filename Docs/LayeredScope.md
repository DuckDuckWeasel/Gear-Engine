# LayeredScope

`Core.LayeredScope` is a small VContainer helper for **stacked `LifetimeScope` layers**: install scopes in order, run async initialization per layer, optionally `PrepareAsync` against the parent resolver, and pop layers with `IAsyncDisposable` teardown. Cross-layer consumers use `ILayerResolver` (bound to the current top scope); vanilla `IObjectResolver` injection stays scoped per layer as usual.

## Three roles

| Role | Type | Role |
|------|------|------|
| Bootstrap | `ApplicationBootstrap` (extends `LifetimeScope`) | Root composition root: registers `LayerResolverProxy` / `ILayerResolver` automatically in `Configure`, creates `ApplicationHost` on `Start`, yields initial layers, drives startup. `ReadyTask` completes after `InstallAllAsync(GetInitialLayers())` and `OnReadyAsync` succeed; it faults on errors or cancels if `destroyCancellationToken` fires. |
| Runtime stack | `ApplicationHost` | `PushAsync` / `PopAsync` / `InstallAllAsync`, implements `ILayerResolver`. Exposes `LayerFailed` event (`Action<LayerOperation, IScopeLayer, Exception>`) for runtime push/pop failures. |
| Layer descriptors | `IScopeLayer`, `IAsyncScopeLayer` | Plain C# objects; `Install` registers into a child scope; optional `PrepareAsync` runs before the child is built. `IScopeLayer.Name` defaults to the concrete type name for logs; override when you want a shorter or stable telemetry key. |

## Setup (root scope)

Subclass `ApplicationBootstrap` (or use it as the component type on your root GameObject). Override `ConfigureApplication(IContainerBuilder builder)` to register app services; do not override `Configure` — the base registers the layer resolver proxy before calling `ConfigureApplication`.

No separate installer call is required.

## Error handling

There are two observation points for failures, used together:

1. `ApplicationBootstrap.OnStartupFailedAsync(Exception, CancellationToken)` — protected virtual fired exactly once when initial-stack installation or `OnReadyAsync` throws. Runs **before** `ReadyTask` faults, so subscribers see the failure ahead of any external `await ReadyTask`. Exceptions thrown from the override are caught and logged so they cannot mask the original cause.
2. `ApplicationHost.LayerFailed` — event raised on `Prepare`, `Init`, `Dispose`, and `Unwind` failures, for both initial and runtime `PushAsync` / `PopAsync` calls. Use this when a screen pushes its own feature layer and needs a single observable channel for failures.

```csharp
protected override Task OnStartupFailedAsync(Exception ex, CancellationToken ct)
{
    crashReporter.Report(ex);
    boot.ShowFatalErrorScreen(ex.Message);
    return Task.CompletedTask;
}

// Runtime:
Host.LayerFailed += (op, layer, ex) =>
{
    telemetry.Track("layer_failed", layer?.Name ?? "<root>", op.ToString(), ex.Message);
};
```

`Debug.LogError` is still emitted at the original failure site so Bugsnag/Crashlytics get a trail.

## Cross-layer registration patterns

Three patterns coexist and the sample exercises all of them:

1. **Service in parent, ctor-injected in child** — register an interface in the upper layer; child layers consume it via constructor injection (VContainer's child-scope inheritance). Example: `ISampleAssetGateway` registered in `SampleAssetsLayer` is injected into `SampleConfigService` in `SampleConfigsLayer`.
2. **Pre-warmed asset in parent, ctor-injected in child** — register a dedicated `IAsyncInitializable` warmer plus a factory registration that returns its built asset. The warmer's init wave runs while the parent layer is being pushed, so any descendant can ctor-inject the asset directly without its own `PrepareAsync`. Example: `SharedSampleAssetWarmer` exposes `SharedSampleAsset`, registered as `Register(resolver => resolver.Resolve<SharedSampleAssetWarmer>().Asset, Lifetime.Singleton)` in `SampleAssetsLayer` and consumed by `SampleFeatureService`.
3. **Per-push asset prepared from parent, instance-registered in own scope** — `IAsyncScopeLayer.PrepareAsync` resolves from the parent and `RegisterInstance` bakes the result into its own container. Example: `SampleFeatureLayer.PrepareAsync` builds a per-push `SampleAsset`.

Pattern 2 is the right tool when many descendants need the asset and the parent layer's lifetime owns the loading.

## Sample scene

The sample is checked in at [Assets/GearEngine/Scripts/Core/LayeredScope/Sample/Scenes/LayeredScopeSample.unity](Assets/GearEngine/Scripts/Core/LayeredScope/Sample/Scenes/LayeredScopeSample.unity). Hierarchy:

- **LayeredScope_Root** — `SampleApplicationBootstrap` (root `LifetimeScope` + bootstrap in one component).

Open that scene in the editor and press Play; no extra root scope GameObject or serialized root reference is required.

### Expected console output (order)

```
[SampleAssetGateway] warming…
[SharedSampleAssetWarmer] preloading shared asset…
[SharedSampleAssetWarmer] ready (payload='asset:shared.payload').
[SampleAssetGateway] ready.
[LayeredScope] Pushed 'SampleAssetsLayer' (init: 2, dispose: 0).
[SampleConfigService] loading via gateway…
[SampleConfigService] ready (value=12).
[LayeredScope] Pushed 'SampleConfigsLayer' (init: 1, dispose: 0).
[SampleFeatureService] init asset='asset:feature.payload', shared='asset:shared.payload', config=12, top resolves gateway? True
[LayeredScope] Pushed 'SampleFeatureLayer' (init: 1, dispose: 1).
[SampleFeatureService] async dispose.
[Sample] feature popped; assets/configs still alive.
```

This demonstrates ordered layers, async in-layer init, deduplication of initializables across the stack, `PrepareAsync`, parent resolution, parent-warmed asset injection into a child layer, `ILayerResolver`, async dispose on pop, and survival of parent layers after the feature layer is popped. The `[SampleAssetGateway] warming…` and `[SharedSampleAssetWarmer] preloading shared asset…` lines may interleave because they run inside the same parallel init wave.

## When to add a new layer

Add a new scope when you need a **feature or infrastructure boundary** (e.g. assets, configs, a race or lobby context) — not per UI widget. Prefer registering shared singletons in the lowest layer that truly owns them; use `IAsyncScopeLayer.PrepareAsync` to resolve from the parent and `RegisterInstance` on the child when you need a concrete instance baked into that layer's container, or use the warmer-plus-factory pattern when many descendants will share it.

## Real-world example: Meta scene

The Meta smoke-test scene ([`Assets/GearEngine/Scenes/Meta.unity`](../Assets/GearEngine/Scenes/Meta.unity)) uses [`MetaApplicationBootstrap`](../Assets/GearEngine/Scripts/App/Bootstrap/MetaApplicationBootstrap.cs): foundation (Addressables, navigation, events) → UGS → Cloud Code + LiveOps. [`Scaffold.Ugs`](../Assets/Packages/com.scaffold.ugs/Runtime/Ugs.cs) and [`LiveOpsService`](../Assets/Packages/com.scaffold.liveops/Runtime/LiveOpsService.cs) implement `IAsyncInitializable` so each layer’s init wave completes before the next scope is pushed. See [`Docs/Meta/Bootstrap.md`](Meta/Bootstrap.md) for the layer list and extension points.
