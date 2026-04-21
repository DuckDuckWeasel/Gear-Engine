# Cloud Code optimistic returns — design notes

**Status:** Implemented (registry + optimistic path + `CloudCodeErrorHandler` + DI; **GameApi** optimistic gate in `LiveOpsService`)  
**Related code:** [`com.scaffold.cloudcode`](https://github.com/MgCohen/Scaffold/tree/main/Assets/Packages/com.scaffold.cloudcode) (`ICloudCodeService`, `ICloudCodeCallHandler`, `CloudCodeService`, `CloudCodeOptimisticHandlerRegistry`, `IRequestHandler*.cs`, `CloudCodeErrorHandler`); [`com.scaffold.liveops`](https://github.com/MgCohen/Scaffold/tree/main/Assets/Packages/com.scaffold.liveops) (`LiveOpsService` GameApi path). Consumer projects resolve these under `Library/PackageCache/`.

## Purpose

Optional layer that can **return a client-side inferred response immediately** for some Cloud Code calls while the **real network call continues in the background**. This improves perceived latency when the response is knowable from the request (or from app rules) before the server responds.

This document captures **decided behavior**, **reconciliation + error flow**, and **references**.

---

## Current architecture (2026-04 snapshot)

### Direct Cloud Code calls (`CallEndpointAsync` payload is the typed request)

- `ICloudCodeService.CallEndpointAsync<T>(module, endpoint, payload, ct)` validates input, wraps the request as **`{ "request", payload }`** (or empty dict when `payload` is null), starts **`Task<string> serverTask = callHandler.InvokeAsync(...)`**, then:
  - If **`CloudCodeOptimisticHandlerRegistry.TryResolve<T>`** (used from `CloudCodeService`) finds a handler and **`TryMatch(module, endpoint, payload)`** succeeds: returns the **optimistic `T`** immediately and schedules **background reconciliation** on the **same** `serverTask` (no second network call).
  - Otherwise **awaits** `serverTask` and deserializes to `T` — failures go through **`CloudCodeErrorHandler.Handle`** then **rethrow**.
- Handler order (innermost → outermost): **SDK** → timeout → response logging → **retry** → **single-flight (per module)**. Optimistic logic is **not** an `ICloudCodeCallHandler`; it lives **inside** `CloudCodeService` for this path.

### GameApi envelope calls (`[UsesGameApi]` → `GameApiEnvelopeRequest` / `GameApiEnvelopeResponse`)

- `LiveOpsService.CallGameApiAsync` starts **`Task<GameApiEnvelopeResponse> serverTask = cloudCodeService.CallEndpointAsync<GameApiEnvelopeResponse>(module, "GameApi", envelope, ct)`**.
- The registry key for Cloud Code is always **`(GameApiEnvelopeRequest, GameApiEnvelopeResponse)`**, so the **optimistic gate inside `CloudCodeService` does not match** per-operation request types.
- **`LiveOpsService`** resolves optimism with **`CloudCodeOptimisticHandlerRegistry.TryResolve<TResponse>(module, "GameApi", typedRequest, ...)`** using the **concrete `ModuleRequest<TResponse>`** and **`TResponse`**. On success it returns the optimistic **`TResponse`** immediately and runs **background reconciliation**: await the same `serverTask`, **unwrap** the envelope (**`UnwrapAndDispatch`**: merge nested responses, **`DispatchNestedResponses`**), then **`Validate(server, optimistic)`**. Failures call **`CloudCodeErrorHandler.Handle`** with the **typed request** as `requestPayload` and the **optimistic `TResponse`** when applicable (no rethrow on the background path).
- **Nested `ModuleResponse` dispatch** runs **only after** the real envelope is unwrapped (same as the non-optimistic GameApi path), not on the optimistic value alone.

### DI

- `CloudCodeInstaller` registers **`CloudCodeOptimisticHandlerRegistry`** (singleton; ctor receives **`IObjectResolver`** for container discovery), **`CloudCodeErrorHandler`** (singleton), and **`ICloudCodeService` → `CloudCodeService`**. Subclass **`CloudCodeErrorHandler`** and register your subclass as **`CloudCodeErrorHandler`** if you need custom behavior.
- **`CloudCodeOptimisticHandlerRegistry`**: internal dictionary keyed by **`(request CLR type, response CLR type)`**. **`TryResolve`** checks the dictionary first; on miss, resolves **`IEnumerable<IOptimisticCloudCodeHandler>`** from the container, finds a matching **`RequestClrType` / `ResponseClrType`** and **`TryMatch`**, then **`as`** to **`IRequestHandler<TResponse>`**. Successful container hits are **cached** in the same dictionary. **`Register<TRequest, TResponse>(...)`** remains the override (checked before container; also used in tests).
- Handlers registered for discovery must implement **`IOptimisticCloudCodeHandler`** (marker with type identity) and **`IRequestHandler<TRequest, TResponse>`**. Prefer **`singleton`** registrations; caching a non-singleton instance on the registry would be incorrect.
- `LiveOpsService` takes **`CloudCodeOptimisticHandlerRegistry`** and **`CloudCodeErrorHandler`** in addition to **`ICloudCodeService`** and **`IObjectResolver`**.
- **`LiveOpsOptimisticRegistrationExtensions.RegisterOptimisticCloudCodeHandler<TImplementation>`** in `Scaffold.LiveOps.Container` registers **`As<IOptimisticCloudCodeHandler>().AsImplementedInterfaces()`**.

---

## Decided: handler API (final)

| Piece | Decision |
|--------|-----------|
| **Registry** | **`(TRequest, TResponse)`** — explicit `Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse>)` **or** DI discovery via **`IOptimisticCloudCodeHandler`** + **`IEnumerable<IOptimisticCloudCodeHandler>`** (no `MakeGenericType`; type pair from marker properties). Dictionary checked first; container hits cached. |
| **`IRequestHandler`** | **`TryMatch` only** — non-generic registry slot. |
| **`IRequestHandler<TResponse>`** | **`GetOptimisticResponse`**, **`Validate(server, optimistic)`** only. |
| **`IRequestHandler<TRequest, TResponse>`** | App implements typed **`TryMatch`**, **`GetOptimisticResponse(TRequest)`**; inherits **`Validate`**. |
| **Errors** | **`CloudCodeErrorHandler.Handle`** — one concrete type; not optimistic-specific. |

---

## Cloud Code errors (all calls)

**`CloudCodeErrorHandler`** is invoked for:

- **Direct await path** — any exception from **`CallAsync`** (network/SDK chain, deserialization).
- **Optimistic trailing path** — any exception from **`CallAsync`** or **`Validate`** in background reconciliation.

Signature:

```csharp
public virtual void Handle(
    Exception exception,
    string module,
    string endpoint,
    object requestPayload,
    object optimisticResponseOrNull)
```

Use **`optimisticResponseOrNull`** only when the failure came from **after** an optimistic return; otherwise it is **`null`**. Add logging, invalidation, or other app logic in **`Handle`** or in a subclass.

Direct path: **`Handle`** is called, then the exception is **rethrown**. Background path: **`Handle`** only (caller already completed).

---

## Reconciliation (optimistic only)

After an optimistic return, the **same** `serverTask` is awaited off the caller’s continuation.

1. **Success** — Deserialize JSON to `T`, then **`Validate(TResponse serverResponse, TResponse optimisticResponse)`**.

2. **Failure** — **`CloudCodeErrorHandler.Handle`** with the **optimistic** value passed as **`optimisticResponseOrNull`**.

```csharp
private async void RunReconciliationInTheBackground<T>(...)
{
    try
    {
        T response = await CallAsync<T>(serverTask);
        handler.Validate(response, optimisticResponse);
    }
    catch (Exception ex)
    {
        cloudCodeErrorHandler.Handle(ex, module, endpoint, requestPayload, optimisticResponse);
    }
}
```

---

## Interface summary (repo)

```csharp
public interface IRequestHandler
{
    bool TryMatch(string module, string endpoint, object request);
}

public interface IRequestHandler<TResponse> : IRequestHandler
{
    TResponse GetOptimisticResponse(object request);
    void Validate(TResponse serverResponse, TResponse optimisticResponse);
}

public interface IRequestHandler<TRequest, TResponse> : IRequestHandler<TResponse>
    where TRequest : class
{
    bool TryMatch(string module, string endpoint, TRequest request);
    TResponse GetOptimisticResponse(TRequest request);
}
```

---

## Resolver safety (implemented)

- **`request == null`** → optimistic path skipped (`TryResolve` returns false).
- **Registry entry must implement `IRequestHandler<TResponse>`** — if not, skip; **`TryMatch`** runs only after a successful typed reference.

---

## Terminology

| Term | Meaning here |
|------|----------------|
| **Optimistic return** | The async method completes with a value **before** the server response is available, using a locally computed guess. |
| **Background / trailing call** | The real `InvokeAsync` continues after the caller already received `T`. |
| **Deterministic inference** | The guessed `T` is **guaranteed** to match what the server would return for that payload (pure function of payload + known rules). **Intended use for now.** |
| **Single-flight** | At most one in-flight **real** Cloud Code call per **module name**. |

---

## Cross-cutting policies

| # | Topic | Decision |
|---|--------|-----------|
| 1 | Handler mismatch / ineligible request | **Await server** (no optimistic shortcut). |
| 2 | Deterministic vs speculative | Plumbing is neutral; **behavior** aimed at **deterministic** first. |
| 3 | Single-flight key | **Per module name**. |
| 4 | Registry key | **Request type + response type**. |
| 5 | Return type | **Keep `Task<T>`.** |
| 6 | Retry vs optimistic | **Independent** in v1. |
| 7 | Cancellation after optimistic return | Treat as **error** (exact UX with product). |

---

## Decision log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-03-31 | Orchestrator in `CloudCodeService`; deterministic use first | Start server `InvokeAsync` first; optimistic is not an `ICloudCodeCallHandler`. |
| 2026-03-31 | Mismatch → error + restart | Hard consistency (app/LiveOps). |
| 2026-03-31 | Single-flight per **module** | Avoid races. |
| 2026-03-31 | Keep `Task<T>` | No public surface change for optimism. |
| 2026-03-31 | Payload = `object`; internal `{ "request", payload }` | Type-based routing; keeps wire format. |
| 2026-04-01 | **`CloudCodeErrorHandler`** concrete, all Cloud Code failures | No optimistic-only interface; same hook for direct and trailing paths. |
| 2026-04-01 | Null payload skips optimistic | Avoid `GetType()` on null. |
| 2026-04-20 | **GameApi optimistic gate in `LiveOpsService` (Option A)** | Envelope breaks `(payload.GetType(), T)` lookup in `CloudCodeService`; keep **`(TRequest, TResponse)`** registry aligned with **`IGameApiHandler<TRequest, TResponse>`**; nested dispatch only after real envelope. |
| 2026-04-21 | **Container discovery for optimistic handlers** | **`IOptimisticCloudCodeHandler`** marker; registry injects **`IObjectResolver`**; dict-first + cache; explicit **`Register`** overrides. |

---

## Next steps

1. Implement or subclass **`CloudCodeErrorHandler`** for global Cloud Code error behavior.
2. Concrete **`IRequestHandler<TRequest, TResponse>`** implementations for **direct** and/or **GameApi** requests (**`Validate`** for success-path reconciliation).
3. Optional: guard or document against registering **`(GameApiEnvelopeRequest, GameApiEnvelopeResponse)`** optimistic handlers (would stack with `LiveOpsService`).

---

## References (Scaffold repo)

- [`CloudCodeService.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.cloudcode/Runtime/CloudCodeService.cs) — `CallEndpointAsync`, `TryGetOptimisticResponse` → `CloudCodeOptimisticHandlerRegistry.TryResolve`, `RunReconciliationInTheBackground`.
- [`CloudCodeErrorHandler.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.cloudcode/Runtime/CloudCodeErrorHandler.cs) — error hook.
- [`CloudCodeOptimisticHandlerRegistry.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.cloudcode/Runtime/Optimistic/CloudCodeOptimisticHandlerRegistry.cs) — `Register<TRequest, TResponse>`, **`TryResolve<TResponse>`**, container scan + cache.
- [`IOptimisticCloudCodeHandler.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.cloudcode/Runtime/Optimistic/IOptimisticCloudCodeHandler.cs) — DI discovery marker.
- [`LiveOpsOptimisticRegistrationExtensions.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.liveops/Container/LiveOpsOptimisticRegistrationExtensions.cs) — optional **`RegisterOptimisticCloudCodeHandler<TImplementation>`**.
- [`LiveOpsService.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.liveops/Runtime/LiveOpsService.cs) — **`CallGameApiAsync`**, **`UnwrapAndDispatch`**, **`RunGameApiReconciliationInTheBackground`**.
- [`IRequestHandler.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.cloudcode/Runtime/Optimistic/IRequestHandler.cs) — handler contracts (`IRequestHandler`, `IRequestHandler<TResponse>`, `IRequestHandler<TRequest,TResponse>`).
- [`Runtime/Handlers/`](https://github.com/MgCohen/Scaffold/tree/main/Assets/Packages/com.scaffold.cloudcode/Runtime/Handlers) — `ICloudCodeCallHandler` and decorator implementations (including single-flight).
- [`CloudCodeInstaller.cs`](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.cloudcode/Container/CloudCodeInstaller.cs) — DI registration.
