# Application startup (composition root)

A minimal **two-scope** host for **UGS + Cloud Code + LiveOps** (no gameplay) lives in [`MetaScope`](../../Assets/GearEngine/Scripts/App/Bootstrap/MetaScope.cs) (`Game.App.Bootstrap`), used by [`Assets/GearEngine/Scenes/Meta.unity`](../../Assets/GearEngine/Scenes/Meta.unity). See [LiveOps bootstrap](../LiveOps/Bootstrap.md) for build / `.ccmr` / Play Mode notes.

There is a partial `com.scaffold.bootstrap` tree under `Assets/Packages/com.scaffold.bootstrap/` (not a full UPM package in this repo); prefer **`MetaScope`** or your own `TwoScopeApplicationHost` subclass for app wiring.

Wire your full game by **subclassing** [`TwoScopeApplicationHost`](../../Assets/Packages/com.scaffold.scope/Runtime/Host/TwoScopeApplicationHost.cs) in **your** assembly: implement `InstallBaseScope` (e.g. Addressables), `PrepareMainScopeAsync` (preload), and `InstallMainScope` (infra modules, navigation, UGS, scene flow, LiveOps, etc.). See [`Startup-Two-Scope-Preload.md`](../../Plans/Startup-Two-Scope-Preload.md).

Optional loading UI: subscribe to [`ApplicationStartupProgress.Changed`](../../Assets/Packages/com.scaffold.scope/Runtime/Host/ApplicationStartupProgress.cs) on the host’s [`StartupProgress`](../../Assets/Packages/com.scaffold.scope/Runtime/Host/TwoScopeApplicationHost.cs) property, or override [`GetStartupProgressListener`](../../Assets/Packages/com.scaffold.scope/Runtime/Host/TwoScopeApplicationHost.cs) to supply a custom [`IApplicationStartupProgress`](../../Assets/Packages/com.scaffold.scope/Runtime/Contracts/IApplicationStartupProgress.cs).
