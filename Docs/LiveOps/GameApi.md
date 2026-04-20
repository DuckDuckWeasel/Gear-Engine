# GameApi (single Cloud Code entry)

All LiveOps player mutations and `GameDataRequest` are routed through one Cloud Code function: **`GameApi`**.

## Client

Requests decorated with `[UsesGameApi]` are sent as a `GameApiEnvelopeRequest` (`RequestKey` + `Payload`) to module **`LiveOps`**, endpoint **`GameApi`**. See `Scaffold.LiveOps` / `LiveOpsService`.

### Optional optimistic responses (client)

When a call can return a **deterministic** `TResponse` before the server responds, register **`IRequestHandler<TRequest, TResponse>`** on **`CloudCodeOptimisticHandlerRegistry`** (same `(TRequest, TResponse)` key as the server’s **`IGameApiHandler<TRequest, TResponse>`**). **`LiveOpsService`** matches on the **typed request** and **`TryMatch(module, "GameApi", request)`**, returns the optimistic **`TResponse`** immediately, and reconciles against the real **`GameApiEnvelopeResponse`** in the background (**`Validate`**, **`CloudCodeErrorHandler`** on failure). **Nested `ModuleResponse` items** from the envelope are merged and dispatched **only after** the real response arrives (not from the optimistic value alone). See [CloudCode-Optimistic-Returns.md](../../Plans/CloudCode-Optimistic-Returns.md). For **how to register, run automated tests, and run the repo validate script**, see [NewApiAndServices.md — §5.1](NewApiAndServices.md#51-optional-optimistic-gameapi-responses-register-test-validate).

## Server

- **`GameApiDispatcher.Invoke`** dispatches to `IGameApiHandler<TRequest, TResponse>` implementations registered in `ModuleConfig`.
- **`GameApiSession`** exposes context, `IPlayerData`, `IGameState`, `IRemoteConfig`, and `InvokeAsync` for nested calls (e.g. level completion awarding gold).
- After a successful handler, **`UnityDataCache.FlushDirtyAsync`** persists keys whose in-memory JSON differs from the loaded snapshot (replaces the old `AddToCache` / `SaveCache` pattern).

## Adding a handler

1. Add `[UsesGameApi]` on the request DTO in `Scaffold.LiveOps.DTO`.
2. Implement `IGameApiHandler<TRequest, TResponse>` in `LiveOps/Project` (handlers are picked up by assembly scan in `ModuleConfig`).
