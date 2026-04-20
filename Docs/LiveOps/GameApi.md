# GameApi (single Cloud Code entry)

All LiveOps player mutations and `GameDataRequest` are routed through one Cloud Code function: **`GameApi`**.

## Client

Requests decorated with `[UsesGameApi]` are sent as a `GameApiEnvelopeRequest` (`RequestKey` + `Payload`) to module **`LiveOps`**, endpoint **`GameApi`**. See `Scaffold.LiveOps` / `LiveOpsService`.

## Server

- **`GameApiDispatcher.Invoke`** dispatches to `IGameApiHandler<TRequest, TResponse>` implementations registered in `ModuleConfig`.
- **`GameApiSession`** exposes context, `IPlayerData`, `IGameState`, `IRemoteConfig`, and `InvokeAsync` for nested calls (e.g. level completion awarding gold).
- After a successful handler, **`UnityDataCache.FlushDirtyAsync`** persists keys whose in-memory JSON differs from the loaded snapshot (replaces the old `AddToCache` / `SaveCache` pattern).

## Adding a handler

1. Add `[UsesGameApi]` on the request DTO in `Scaffold.LiveOps.DTO`.
2. Implement `IGameApiHandler<TRequest, TResponse>` in `LiveOps/Project` (handlers are picked up by assembly scan in `ModuleConfig`).
