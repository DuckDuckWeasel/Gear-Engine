# Guide: New LiveOps API and Services

This describes how to add a **player-facing Cloud Code operation** (request → response) and optional **client-side consumption**, using the **single `GameApi` entry** and shared `**Scaffold.LiveOps.DTO`** assembly.

For background: [GameApi.md](GameApi.md), [Bootstrap.md](Bootstrap.md), [LiveOps.md](LiveOps.md).

---

## 1. What you are adding


| Layer                   | Purpose                                                                                         |
| ----------------------- | ----------------------------------------------------------------------------------------------- |
| **DTO (shared)**        | Request/response types and game data shapes both sides serialize.                               |
| **Cloud Code (server)** | `IGameApiHandler<TRequest,TResponse>` + optional `GameModule<T>` for `GameData` aggregation.    |
| **Unity (client)**      | `ILiveOpsService.CallAsync` and/or a `GameClientModuleBase<T>` that reads `GetModuleData<T>()`. |


There is **one** Cloud Code script name for mutations and `GameDataRequest`: `**GameApi`**. You do **not** add new `[CloudCodeFunction("MyRequest")]` endpoints for each feature.

---

## 2. Minimal required types

Not every feature needs every row. Use this as a checklist.

### 2.1 Always (for a new remote **operation**)


| Artifact         | Location                                                     | Role                                                                                                |
| ---------------- | ------------------------------------------------------------ | --------------------------------------------------------------------------------------------------- |
| **Request DTO**  | `LiveOps/LiveOps.DTO/.../YourFeature/Request/YourRequest.cs` | Inherits `ModuleRequest<TResponse>`. Mark with `[UsesGameApi]` (namespace `GameModuleDTO.GameApi`). |
| **Response DTO** | `LiveOps/LiveOps.DTO/.../YourFeature/YourResponse.cs`        | Inherits `ModuleResponse`.                                                                          |
| **Handler**      | `LiveOps/Project/Modules/.../YourHandler.cs`                 | Implements `IGameApiHandler<YourRequest, YourResponse>`.                                            |


`ModuleRequest` / `ModuleResponse` live in `GameModuleDTO.ModuleRequests`. Default `ModuleName` is `**"LiveOps"`** (must match the deployed module id the client uses).

### 2.2 If the feature has **per-player saved state**


| Artifact            | Location                                      | Role                                                                                                                                                           |
| ------------------- | --------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Persistence DTO** | `LiveOps/LiveOps.DTO/.../YourPersistence.cs`  | Loaded/stored via `IPlayerData` (Cloud Save). Often mutated in the handler; **dirty flush** runs automatically after `GameApi` (see [GameApi.md](GameApi.md)). |
| **Storage key**     | Usually `typeof(T).Name` or a string constant | `Get` / `GetOrSet` / `Set` on `IPlayerData` (see existing **Currency** / **Tracks** patterns).                                                                  |


### 2.3 If the feature uses **Remote Config**


| Artifact       | Location                                | Role                                                                                                |
| -------------- | --------------------------------------- | --------------------------------------------------------------------------------------------------- |
| **Config DTO** | `LiveOps/LiveOps.DTO/.../YourConfig.cs` | Fetched with `IRemoteConfig.Get(...)`. Deploy `.rc` entries per [RemoteConfig.md](RemoteConfig.md). |


### 2.4 If the feature should appear in the initial `**GameDataRequest`** snapshot


| Artifact            | Location                                                                 | Role                                                                                                                    |
| ------------------- | ------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------- |
| **Game data DTO**   | `LiveOps/LiveOps.DTO/.../YourGameData.cs`                                | Implements `IGameModuleData` (`Key` must match how you store/merge data).                                               |
| **Server module**   | `LiveOps/Project/Modules/.../YourModule.cs`                              | `class YourModule : GameModule<YourGameData>` with `Initialize(...)` building `YourGameData` from persistence + config. |
| **Register module** | [ModuleConfig.cs](../../LiveOps/Project/Core/Initialize/ModuleConfig.cs) | `RegisterModuleScoped<YourModule>(config);`                                                                             |


`GameModule<T>.Key` is `**typeof(T).Name`** (the **game data** type name, e.g. `CurrencyGameData`). Keep that consistent with `IGameModuleData.Key` on your DTO.

### 2.5 Client-side (optional but typical)


| Artifact                    | Location                             | Role                                                                                          |
| --------------------------- | ------------------------------------ | --------------------------------------------------------------------------------------------- |
| **Call site**               | Any service / `GameClientModuleBase` | `await liveOps.CallAsync(new YourRequest(...), ct);`                                          |
| **Hydrate from `GameData`** | `GameClientModuleBase<YourGameData>` | Reads `liveOps.GetModuleData<YourGameData>()` after LiveOps init.                             |
| **VContainer**              | Your feature installer               | Register `IGameClientModule` / `IAsyncInitializable` if the module must run during bootstrap. |


Reference: [CurrencyClientModule.cs](../../Assets/GearEngine/Scripts/App/Bootstrap/Currency/CurrencyClientModule.cs) (calls `CallAsync` + updates local snapshot).

---

## 3. Handler registration — how the API finds your handler

You **do not** register each handler in a central dictionary by hand. Two pieces work together:

### 3.1 `GameApiRegistry` (request key → handler metadata)

At startup, a singleton `**GameApiRegistry`** scans the **same assembly as `GameApiDispatcher`** (`LiveOps.dll`) for every concrete type that implements `**IGameApiHandler<TRequest, TResponse>**`. For each handler it records:

- **Key** = `typeof(TRequest).Name` (e.g. `"AddCurrencyRequest"`)
- **Value** = **`HandlerEntry`**: request type, response type, and **concrete handler `Type`**

That key must match what the client sends in `**GameApiEnvelopeRequest.RequestKey`** (typically `request.GetType().Name`).

Implementation: `[GameApiRegistry.cs](../../LiveOps/Project/Core/GameApi/GameApiRegistry.cs)`.

### 3.2 Dependency injection (resolve the handler instance)

Still in `**ModuleConfig.Setup`**, a loop registers **every concrete handler class** (types assignable from **`IGameApiHandler`**, which includes all `IGameApiHandler<,>` implementations) with the Cloud Code DI container as **scoped** — **by concrete `Type`**, not by closed generic interface:

```csharp
// LiveOps/Project/Core/Initialize/ModuleConfig.cs — handler DI loop (excerpt)
Assembly gameApiAssembly = typeof(GameApiDispatcher).Assembly;
GameApiRegistry gameApiRegistry = new GameApiRegistry(gameApiAssembly);
config.Dependencies.AddSingleton(gameApiRegistry);
RegisterScoped<GameApiDispatcher>(config);

foreach (Type type in gameApiAssembly.GetTypes())
{
    if (type.IsAbstract || type.IsInterface)
        continue;
    if (!typeof(IGameApiHandler).IsAssignableFrom(type))
        continue;
    config.Dependencies.AddScoped(type);
}
```

When `**GameApiDispatcher.Invoke**` runs, it:

1. Uses `**GameApiRegistry.TryGet(requestKey, …)**` (or `**TryResolve**` for request/response types only) to get the **`HandlerEntry`**.
2. Deserializes the payload into the request type via `**JObject.ToObject(requestType, …)**`.
3. Resolves the handler with `**GetService(entry.HandlerType)**` and calls **`IGameApiHandler.HandleAsync(session, requestObj)`** (reflection-free; the typed handler’s default interface implementation performs the cast).

So your **only** obligation for registration is:

- Implement `**YourHandler : IGameApiHandler<YourRequest, YourResponse>`**.
- Put the class in `**LiveOps/Project`** so it is compiled into `**LiveOps.dll`** (the assembly passed to `GameApiRegistry`).

**Explicit registration:** call **`ModuleConfig.RegisterGameApiHandler<TReq, TRes, THandler>(config, registry)`** to register a concrete handler and merge it into the same registry (idempotent if the handler was already scanned).

**If you add handlers in another assembly**, pass that assembly into **`new GameApiRegistry(...)`** and call **`RegisterGameApiHandler`** (or add a similar scan) so both the registry and DI know the concrete handler type.

---

## 4. Required steps and configuration (server)

### Step A — DTO project (`Scaffold.LiveOps.DTO`)

1. Add request/response (+ persistence/config/game data as needed) under `LiveOps/LiveOps.DTO/`.
2. On the **request** type, add:
  ```csharp
   using GameModuleDTO.GameApi;

   [UsesGameApi]
   public sealed class YourRequest : ModuleRequest<YourResponse> { ... }
  ```
3. Build the DTO project so Unity gets the updated plugin:
  ```powershell
   dotnet build "LiveOps\LiveOps.DTO\Scaffold.LiveOps.DTO.csproj" -c Release
  ```
   The `.csproj` copies `**Scaffold.LiveOps.DTO.dll**` to `**Assets/Plugins/Scaffold.LiveOps.DTO/**`.

### Step B — Cloud Code project (`LiveOps/Project`)

1. Add `**YourHandler : IGameApiHandler<YourRequest, YourResponse>**` with:
  `public async Task<YourResponse> HandleAsync(GameApiSession session, YourRequest request)`
2. Use `session.Context`, `session.Player`, `session.GameState`, `session.RemoteConfig` inside the handler.
3. For **nested** operations (e.g. award currency from another handler), use:
  `await session.InvokeAsync<AddCurrencyRequest, AddCurrencyResponse>(new AddCurrencyRequest(currencyId, amount));`
   (Pattern: any handler can invoke another registered `IGameApiHandler` via `GameApiSession.InvokeAsync`.)

See **§3** above for how handlers are wired into `**GameApiRegistry`** and DI.

### Step C — `GameModule` (only if part of `GameData`)

1. Implement `YourModule : GameModule<YourGameData>` and `Initialize(...)`.
2. In `**ModuleConfig.Setup`**, add:
  `RegisterModuleScoped<YourModule>(config);`

### Step D — No `[CloudCodeFunction]` on the feature

Only `**GameApiDispatcher`** exposes `[CloudCodeFunction("GameApi")]`. Do not add per-request Cloud Code function attributes.

---

## 5. Required steps (client)

1. Ensure `**LiveOpsInstaller**` is part of your bootstrap (e.g. [LiveOpsLayer.cs](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/LiveOpsLayer.cs)).
2. Resolve `**ILiveOpsService**` and call:
  ```csharp
   YourResponse response = await liveOpsService.CallAsync(new YourRequest(...), cancellationToken);
  ```
3. If the response can carry **nested** `ModuleResponse` items (from `session.InvokeAsync` on the server), handle them via existing `**IResponseHandler` / `IResponseHandler<T>`** registration (see [com.scaffold.liveops README](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.liveops/README.md)).

**Routing:** `LiveOpsService` routes to `**GameApi`** when the request type has `**[UsesGameApi]`**. Requests **without** the attribute still use `request.ModuleName` + `request.FunctionName` (legacy path); new work should use the attribute.

### 5.1 Optional: optimistic GameApi responses (register, test, validate)

Use this when the client can infer `**TResponse`** deterministically before Cloud Code returns (same intent as server `**IGameApiHandler<YourRequest, YourResponse>**`, but client-side). Background work still runs one real `**GameApi**` call; `**Validate**` and `**CloudCodeErrorHandler**` cover mismatches and failures. See [GameApi.md](GameApi.md) and [CloudCode-Optimistic-Returns.md](../../Plans/CloudCode-Optimistic-Returns.md).

#### Register (production)


| Step                | What to do                                                                                                                                                                                                                                                                                                                                                                                                         |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Bootstrap order** | Install `**CloudCodeInstaller`** before `**LiveOpsInstaller**` so `**CloudCodeOptimisticHandlerRegistry**` and `**CloudCodeErrorHandler**` exist when `**LiveOpsService**` is resolved. Example: [LiveOpsLayer.cs](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/LiveOpsLayer.cs).                                                                                                                          |
| **Implement**       | A class implementing `**IRequestHandler<TRequest, TResponse>`** (`Scaffold.CloudCode`) with the **same** `TRequest` / `TResponse` as your GameApi operation. Implement `**TryMatch(module, endpoint, request)`** (typically `endpoint == "GameApi"` and `module` matches `**ModuleRequest.ModuleName**`, usually `**"LiveOps"**`), `**GetOptimisticResponse**`, and `**Validate(server, optimistic)**`.            |
| **Wire (preferred)** | Register the handler as a **singleton** with **`IOptimisticCloudCodeHandler`** so **`CloudCodeOptimisticHandlerRegistry`** can resolve **`IEnumerable<IOptimisticCloudCodeHandler>`** on first **`TryResolve`** and **cache** the handler. Use **[`LiveOpsOptimisticRegistrationExtensions`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.liveops/Container/LiveOpsOptimisticRegistrationExtensions.cs)** or `Register<THandler>(Lifetime.Singleton).As<IOptimisticCloudCodeHandler>().AsImplementedInterfaces()`. |
| **Wire (override)** | Call **`registry.Register<TRequest, TResponse>(handler)`** (e.g. `RegisterBuildCallback` or `IAsyncInitializable`) when you need an explicit entry that **wins over** container discovery for the same type pair. |
| **Errors**          | Optionally subclass `**CloudCodeErrorHandler`** and register that concrete type as `**CloudCodeErrorHandler**` for logging, invalidation, or telemetry when reconciliation fails.                                                                                                                                                                                                                                  |


#### Example: where and how to register (VContainer)

**1. Handler** — same `YourRequest` / `YourResponse` as the server `IGameApiHandler<,>` (DTOs in `Scaffold.LiveOps.DTO`). Implement **`IOptimisticCloudCodeHandler`** so the registry can match types without reflection beyond `typeof`:

```csharp
using System;
using GameModuleDTO.ModuleRequests;
using Scaffold.CloudCode;

public sealed class YourRequestOptimisticHandler : IRequestHandler<YourRequest, YourResponse>, IOptimisticCloudCodeHandler
{
    public Type RequestClrType => typeof(YourRequest);

    public Type ResponseClrType => typeof(YourResponse);

    public bool TryMatch(string module, string endpoint, YourRequest request)
    {
        return module == request.ModuleName && endpoint == "GameApi";
    }

    public YourResponse GetOptimisticResponse(YourRequest request)
    {
        return new YourResponse { /* deterministic fields from request + client rules */ };
    }

    public void Validate(YourResponse serverResponse, YourResponse optimisticResponse)
    {
        /* throw or log if server disagrees with what the client assumed */
    }
}
```

Optional: subclass **`OptimisticHandlerBase<YourRequest, YourResponse>`** to supply **`RequestClrType`** / **`ResponseClrType`** and only implement **`TryMatch`**, **`GetOptimisticResponse`**, **`Validate`**.

**2. Registration (preferred)** — same `IContainerBuilder` as **`CloudCodeInstaller`** / **`LiveOpsInstaller`** ([`LiveOpsLayer.cs`](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/LiveOpsLayer.cs)):

```csharp
using Scaffold.LiveOps.Container;
using VContainer;
using VContainer.Unity;

public sealed class YourFeatureInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.RegisterOptimisticCloudCodeHandler<YourRequestOptimisticHandler>(Lifetime.Singleton);
    }
}
```

Equivalent manual registration:

```csharp
builder.Register<YourRequestOptimisticHandler>(Lifetime.Singleton)
    .As<IOptimisticCloudCodeHandler>()
    .AsImplementedInterfaces();
```

**Override:** call **`CloudCodeOptimisticHandlerRegistry.Register<YourRequest, YourResponse>(handler)`** from `RegisterBuildCallback` or `IAsyncInitializable` when an explicit instance must win over a DI-registered handler for the same type pair.

**Manual check:** Breakpoint in `**Validate`** or your error handler should run **after** the optimistic `**CallAsync`** has already completed to the caller; nested `**ModuleResponse**` side effects (via `**IResponseHandler**`) should run **only** after the real envelope is unwrapped, not on the optimistic value alone.

#### Test (automated)


| Item           | Location                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Fixture**    | [`LiveOpsServiceOptimisticTests.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.liveops/Tests/LiveOpsServiceOptimisticTests.cs) — fake `**ICloudCodeService`**, registry, `**CloudCodeErrorHandler**`, VContainer + `**LiveOpsInstaller**`, `**ILiveOpsService.CallAsync**` with a `**[UsesGameApi]**` request. Editor bootstrap mirror: [`MetaLayerInitializationTests.cs`](../../Assets/GearEngine/Scripts/App/Bootstrap/Tests/Editor/MetaLayerInitializationTests.cs) (`LiveOpsLayerComposition_GameApiOptimistic_ReturnsBeforeSlowServerCompletes`). |
| **How to run** | Unity **EditMode** tests, assembly `**Scaffold.LiveOps.Tests`**. Close any other Unity instance that has the same project open so batchmode can run.                                                                                                                                                                                                                                                                                                                                                                                             |


#### Validate (repo quality gate)

From the repository root:

```powershell
.\.agents\scripts\validate-changes.cmd
```

That runs analyzers, Unity compilation precheck, and tests when the project is not locked by another editor. Fix failures before merging.

---

## 6. How to call an API (summary)


| From                            | Code                                                                                             |
| ------------------------------- | ------------------------------------------------------------------------------------------------ |
| Anywhere with `ILiveOpsService` | `await liveOps.CallAsync(new YourRequest(...), ct);`                                             |
| Initial snapshot                | Automatic: `LiveOpsService` runs `GameDataRequest` on init; use `GetModuleData<YourGameData>()`. |


Cloud Code wire shape: the Scaffold Cloud Code client wraps the payload as `**{ "request": <body> }`**. For GameApi, the body is `**GameApiEnvelopeRequest`** (`requestKey` + `payload`), built inside `LiveOpsService`.

---

## 7. How to build and upload (deploy)

1. **Build solution** (DTO + server):
  ```powershell
   dotnet build "LiveOps\LiveOps.sln" -c Release
  ```
2. **Confirm Unity has the DTO DLL** at `Assets/Plugins/Scaffold.LiveOps.DTO/Scaffold.LiveOps.DTO.dll` (post-build copy from DTO project).
3. **Publish Cloud Code** using Unity’s **Cloud Code** / **Deployment** workflow. The repo uses `**Assets/CloudCode/LiveOps.ccmr`** pointing at `**LiveOps/LiveOps.sln`** (see [Bootstrap.md](Bootstrap.md)).
4. **Remote Config**: add or update keys your module reads, then deploy `.rc` per [RemoteConfig.md](RemoteConfig.md).
5. **Smoke test**: Play **Meta** scene with UGS linked; on success you should see `[Meta] LiveOps ready...` and your new data keys if included in `GameDataRequest`.

---

## 8. Checklist (copy-paste)

- `YourRequest` : `ModuleRequest<YourResponse>` + `[UsesGameApi]`
- `YourResponse` : `ModuleResponse`
- `YourHandler` : `IGameApiHandler<YourRequest, YourResponse>` in `LiveOps/Project` (auto-registered via `ModuleConfig`; see §3)
- (Optional) Persistence/config/game DTOs in `LiveOps.DTO`
- (Optional) `GameModule<YourGameData>` + `RegisterModuleScoped` in `ModuleConfig`
- Build DTO → Unity plugin updated
- Build + deploy Cloud Code **LiveOps** module
- Client: `CallAsync(new YourRequest(...))` and/or `GetModuleData<YourGameData>()`
- Remote Config deployed if needed
- (Optional) Client optimistic GameApi: `IRequestHandler<TRequest, TResponse>` + `CloudCodeOptimisticHandlerRegistry.Register` (see §5.1)

---

## 9. Further reading

- [GameApi.md](GameApi.md) — dispatcher, session, flush behavior, optimistic client notes
- [Bootstrap.md](Bootstrap.md) — `.ccmr`, Meta flow, build outputs
- [Currency.md](Currency.md) — example of config + persistence + client module
- [CloudCode-Optimistic-Returns.md](../../Plans/CloudCode-Optimistic-Returns.md) — optimistic architecture (direct Cloud Code vs GameApi path)
- [`LiveOpsServiceOptimisticTests.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.liveops/Tests/LiveOpsServiceOptimisticTests.cs) — EditMode tests for GameApi optimism

