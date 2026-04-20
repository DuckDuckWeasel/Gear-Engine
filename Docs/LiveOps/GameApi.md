# GameApi (single Cloud Code entry)

All LiveOps player mutations and `GameDataRequest` are routed through one Cloud Code function: **`GameApi`**.

## Client

Requests decorated with `[UsesGameApi]` are sent as a `GameApiEnvelopeRequest` (`RequestKey` + `Payload`) to module **`LiveOps`**, endpoint **`GameApi`**. See `Scaffold.LiveOps` / `LiveOpsService`.

### Optional optimistic responses (client)

When a call can return a **deterministic** `TResponse` before the server responds, implement **`IRequestHandler<TRequest, TResponse>`** and **`IOptimisticCloudCodeHandler`** (same `(TRequest, TResponse)` as the server’s **`IGameApiHandler<TRequest, TResponse>`**), and register the handler in VContainer with **`As<IOptimisticCloudCodeHandler>().AsImplementedInterfaces()`** (or **`RegisterOptimisticCloudCodeHandler<T>`** from `Scaffold.LiveOps.Container`). **`CloudCodeOptimisticHandlerRegistry`** resolves handlers from the container on first use and caches them; explicit **`Register(...)`** still overrides. **`LiveOpsService`** matches on the **typed request** and **`TryMatch(module, "GameApi", request)`**, returns the optimistic **`TResponse`** immediately, and reconciles against the real **`GameApiEnvelopeResponse`** in the background (**`Validate`**, **`CloudCodeErrorHandler`** on failure). **Nested `ModuleResponse` items** from the envelope are merged and dispatched **only after** the real response arrives (not from the optimistic value alone). See [CloudCode-Optimistic-Returns.md](../../Plans/CloudCode-Optimistic-Returns.md). For **how to register, run automated tests, and run the repo validate script**, see [NewApiAndServices.md — §5.1](NewApiAndServices.md#51-optional-optimistic-gameapi-responses-register-test-validate).

## Server

- **`GameApiDispatcher.Invoke`** looks up a **`HandlerEntry`** (request type, response type, **concrete handler type**) in **`GameApiRegistry`**, deserializes the payload with `JObject.ToObject(requestType, …)`, resolves the handler by **concrete `Type`** from DI, and calls the non-generic **`IGameApiHandler.HandleAsync(session, object)`**. The typed **`IGameApiHandler<TRequest, TResponse>`** implements that via a default interface method that casts once to `TRequest` — no per-request `MakeGenericType` / `MethodInfo.Invoke` / `Task.Result` reflection.
- **`GameApiSession`** exposes context, `IPlayerData`, `IGameState`, `IRemoteConfig`, and `InvokeAsync` for nested calls (e.g. level completion awarding gold). Nested calls resolve the handler via the same registry + concrete type + cast to **`IGameApiHandler<TReq, TRes>`**.
- After a successful handler, **`IPlayerData.FlushDirtyAsync`** persists keys whose in-memory JSON differs from the loaded snapshot (replaces the old `AddToCache` / `SaveCache` pattern).

## Adding a handler

1. Add `[UsesGameApi]` on the request DTO in `Scaffold.LiveOps.DTO`.
2. Implement `IGameApiHandler<TRequest, TResponse>` in `LiveOps/Project` (handlers are picked up by assembly scan in `ModuleConfig`). Optionally register explicitly with **`ModuleConfig.RegisterGameApiHandler<TReq, TRes, THandler>(config, registry)`** (e.g. handlers in another assembly).
