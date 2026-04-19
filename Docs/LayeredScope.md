# LayeredScope

`Core.LayeredScope` is a small VContainer helper for **stacked `LifetimeScope` layers**: install scopes in order, run async initialization per layer, optionally `PrepareAsync` against the parent resolver, and pop layers with `IAsyncDisposable` teardown. Cross-layer consumers use `ILayerResolver` (bound to the current top scope); vanilla `IObjectResolver` injection stays scoped per layer as usual.

## Three roles

| Role | Type | Role |
|------|------|------|
| Bootstrap | `ApplicationBootstrap` (`MonoBehaviour`) | Holds the root `LifetimeScope`, creates `ApplicationHost`, yields initial layers, drives startup. `ReadyTask` completes after `InstallAllAsync(GetInitialLayers())` and `OnReadyAsync` succeed; it faults on errors or cancels if `destroyCancellationToken` fires. |
| Runtime stack | `ApplicationHost` | `PushAsync` / `PopAsync` / `InstallAllAsync`, implements `ILayerResolver`. |
| Layer descriptors | `IScopeLayer`, `IAsyncScopeLayer` | Plain C# objects; `Install` registers into a child scope; optional `PrepareAsync` runs before the child is built. |

## Setup (root scope)

In your root `LifetimeScope.Configure`:

```csharp
LayeredScopeInstaller.Install(builder);
```

Then create an `ApplicationHost` (usually from a subclass of `ApplicationBootstrap` assigned in a scene).

## Sample scene

The sample is checked in at [Assets/GearEngine/Scripts/Core/LayeredScope/Sample/Scenes/LayeredScopeSample.unity](Assets/GearEngine/Scripts/Core/LayeredScope/Sample/Scenes/LayeredScopeSample.unity). Hierarchy:

- **LayeredScope_Root** — `SampleRootScope` (root `LifetimeScope`, runs `LayeredScopeInstaller`).
- Child **Bootstrap** — `SampleApplicationBootstrap` with **Root Scope** referencing the root’s `SampleRootScope`.

Open that scene in the editor and press Play; no manual wiring is required unless you duplicate the scene.

### Expected console output (order)

```
[SampleAssetGateway] warming…
[SampleAssetGateway] ready.
[LayeredScope] Pushed 'SampleAssets' (init: 1, dispose: 0).
[SampleConfigService] loading via gateway…
[SampleConfigService] ready (value=12).
[LayeredScope] Pushed 'SampleConfigs' (init: 1, dispose: 0).
[SampleFeatureService] init asset='asset:feature.payload', config=12, top resolves gateway? True
[LayeredScope] Pushed 'SampleFeature' (init: 1, dispose: 1).
[SampleFeatureService] async dispose.
[Sample] feature popped; assets/configs still alive.
```

This demonstrates ordered layers, async in-layer init, deduplication of initializables across the stack, `PrepareAsync`, parent resolution, `ILayerResolver`, async dispose on pop, and survival of parent layers after the feature layer is popped.

## When to add a new layer

Add a new scope when you need a **feature or infrastructure boundary** (e.g. assets, configs, a race or lobby context)—not per UI widget. Prefer registering shared singletons in the lowest layer that truly owns them; use `IAsyncScopeLayer.PrepareAsync` to resolve from the parent and `RegisterInstance` on the child when you need a concrete instance baked into that layer’s container.
