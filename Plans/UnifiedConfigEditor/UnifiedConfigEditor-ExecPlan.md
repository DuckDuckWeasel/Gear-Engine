# Unified LiveOps Configs editor window

This ExecPlan is a living document.

## Purpose / Big Picture

Replace separate surfaces (**Catalog SO** in the Project window, **ConfigBuilderSO** in the Project window, **Window → LiveOps → Config Deployment**, **Window → Deployment**) with a single **Window → LiveOps → Configs** UI Toolkit window that lists all `ConfigBuilderSOBase` assets, embeds builder and referenced **ScriptableObject** inspectors, syncs to `Assets/LiveOps/RemoteConfig/*.rc`, compares local payloads to the live dashboard, and deploys via `com.unity.services.deployment` with **`ugs deploy` CLI** fallback.

## Progress

- [x] Extract disk sync into `RcSyncService` (`RowStatus`, `Sync`, `Pull`, `GetRcPath`, `RenderEnvelopeJson`).
- [x] Add `IRemoteDeployer` / `RemoteDeployer` (Deployment API + CLI).
- [x] Add `CloudRemoteConfigSnapshot` using `Unity.RemoteConfig.Editor.RemoteConfigWebApiClient` + `Deployments.Instance.EnvironmentProvider`.
- [x] Add `LiveOpsConfigDiscovery` with duplicate `ConfigKey` detection.
- [x] Add `LiveOpsConfigsWindow`, `ConfigDetailView`, `ConfigReferencedScriptableObjects`.
- [x] Remove legacy `ConfigDeploymentWindow` / `DeploymentRow`.
- [x] Reference `Unity.Services.DeploymentApi` and `Unity.RemoteConfig.Editor` from `Scaffold.LiveOps.Authoring.Editor.asmdef`.
- [x] EditMode tests: drift status, duplicate flags, cloud JSON extraction, `IRemoteDeployer` recording stub.
- [x] Docs: `AuthoringPipeline.md`, `RemoteConfig.md`, `Bootstrap.md`, package READMEs, note in `RemoteConfigDeployment-ExecPlan.md`.

## Surprises & Discoveries

- `RemoteConfigWebApiClient` uses callbacks and `fetchConfigsFinished`; completion must be guarded against double-finish (parse error + empty fetch).
- `ListView.selectionChanged` follows `BaseVerticalCollectionView`’s `(IEnumerable<object>, IEnumerable<object>)` signature in this Unity version.

## Decision Log

- **No new base API on `ConfigBuilderSOBase`** — referenced catalog SOs are found by walking `SerializedObject` for `ScriptableObject` references.
- **Duplicate `ConfigKey`** — all rows in a duplicate group are marked; batch Sync / Pull / Deploy are disabled until fixed.
- **Deploy** — prefer `IDeploymentWindow.Deploy(paths)`; on failure or missing window, run `ugs deploy "<fullpath>"`.
- **Cloud diff** — compare dashboard JSON value for the setting key to the local `entries[ConfigKey]` fragment.

## Outcomes & Retrospective

- Pending after team smoke-test: linked UGS project, Deploy, and **Compare with cloud** on each module key.

## Context and Orientation

- Runtime and server contracts unchanged: `ConfigBuilderSO<TConfig>`, `RcEnvelope`, committed `.rc` files, `LiveOpsConfigBuilderAndRcTests`.
- Implementation package: [Assets/Packages/com.scaffold.liveops.authoring](../../Assets/Packages/com.scaffold.liveops.authoring/README.md).

## Plan of Work

1. Services: `RcSyncService`, `RemoteDeployer`, `CloudRemoteConfigSnapshot`.
2. Discovery: `LiveOpsConfigDiscovery`, `ConfigReferencedScriptableObjects`.
3. UI: `LiveOpsConfigsWindow` + `ConfigDetailView` (toolbar, list, IMGUI embedded inspectors, cloud diff).
4. Remove legacy deployment IMGUI window.
5. Tests + docs.

## Concrete Steps

See implementation under `Assets/Packages/com.scaffold.liveops.authoring/Editor/` and tests in `Game.Campaign.Tests`.

## Validation and Acceptance

- [ ] **Window → LiveOps → Configs** lists all builders; statuses match disk after checkout.
- [ ] Sync / Pull / Deploy / Compare with cloud succeed against a dev environment (manual).
- [ ] `LiveOpsConfigBuilderAndRcTests` and new EditMode tests pass in Unity.
- [ ] `.agents/scripts/validate-changes.cmd` passes.

## Idempotence and Recovery

- Re-deploying the same `.rc` overwrites remote keys; use Dashboard or `ugs` backup before risky edits.
- If the Deployment API is unavailable, install **UGS CLI** and ensure `ugs` is on `PATH`.

## Artifacts and Notes

- [Assets/Packages/com.scaffold.liveops.authoring/Editor/Window/LiveOpsConfigsWindow.cs](../../Assets/Packages/com.scaffold.liveops.authoring/Editor/Window/LiveOpsConfigsWindow.cs)
- [Docs/LiveOps/RemoteConfig.md](../../Docs/LiveOps/RemoteConfig.md)

## Interfaces and Dependencies

- `com.scaffold.liveops.authoring` — `ConfigBuilderSOBase`, `RcEnvelope`
- `com.unity.services.deployment` / `Unity.Services.DeploymentApi` — `IDeploymentWindow`, `Deployments`
- `com.unity.remote-config` — `Unity.RemoteConfig.Editor.RemoteConfigWebApiClient`
- Newtonsoft.Json (existing)
