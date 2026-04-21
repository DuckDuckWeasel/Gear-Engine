# Scaffold States

Authoritative module documentation: [`com.scaffold.states` README](https://github.com/MgCohen/Scaffold/blob/main/Assets/Packages/com.scaffold.states/README.md).

**Deferred `Notify` batching:** The states package can wrap `IStateEventHandler` with `DeferredStateEventHandler` so notifications flush on demand; see the README section *Deferred event dispatch*. UI-only deferral of MVVM binding updates is a separate feature (`BindingDeferredUpdate` ExecPlan under `Plans/`).
