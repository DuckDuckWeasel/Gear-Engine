# Remote Config `.rc` deployment (LiveOps)

This ExecPlan is a living document.

## Purpose / Big Picture

Move LiveOps Remote Config authoring from ad-hoc CSV/JSON under `LiveOps/LiveOps.DTO/config/` to per-module `.rc` files under `Assets/LiveOps/RemoteConfig/`, deployed with `com.unity.services.deployment` via **Window → Deployment**, keeping the same Remote Config keys consumed by Cloud Code (see [Docs/LiveOps/RemoteConfig.md](../../Docs/LiveOps/RemoteConfig.md) for the current key list).

**Update:** Hand-editing JSON under `Assets/LiveOps/RemoteConfig/` for content is superseded by **`com.scaffold.liveops.authoring`** — **Window → LiveOps → Config Deployment** + `ConfigBuilderSO` assets (see [Docs/LiveOps/AuthoringPipeline.md](../../Docs/LiveOps/AuthoringPipeline.md)). This ExecPlan remains valid for **Deployment** (link project → Window → Deployment → smoke-test Meta).

## Progress

- [x] Add `com.unity.services.deployment` to `Packages/manifest.json` and align `packages-lock.json`.
- [x] Create `Assets/LiveOps/RemoteConfig/` with per-module `.rc` files and Unity-compatible `.meta` (ScriptedImporter).
- [x] Remove legacy `LiveOps/LiveOps.DTO/config/` sources.
- [x] Document workflow in `Docs/LiveOps/RemoteConfig.md` and update `Docs/LiveOps/Bootstrap.md`.
- [ ] **Manual:** Deploy `.rc` files from **Window → Deployment** to the target UGS environment and smoke-test Meta scene (requires Editor + linked project).

## Surprises & Discoveries

- Unity’s package sample `.rc` format uses top-level `entries` with JSON object values, not `{ "type": "json", "value": … }` per entry; the built-in importer matches the sample in `com.unity.remote-config` samples.

## Decision Log

- **Per-module files** — smaller PR diffs, aligns with `LiveOps/Project/Modules/`.
- **Under `Assets/`** — required for the Deployment window to list assets.
- **Level values** — seeded from former `LevelConfig.json` (`_levels` `[1,2]`, `_rewardPerLevel` `10`); `configs.csv` differed and was superseded by `.rc` as source of truth.

## Outcomes & Retrospective

- Pending after manual Dashboard/Meta verification in a linked environment.

## Context and Orientation

- Cloud Code resolves config through `UnityRemoteConfig` → UGS Remote Config API.
- No C# changes were required as long as key names stayed the same.

## Plan of Work

1. Packages: explicit `com.unity.services.deployment` dependency.
2. Assets: five `.rc` files + folder structure + metas.
3. Remove legacy config folder.
4. Docs: `RemoteConfig.md`, Bootstrap prerequisites + changelog.
5. Validate: repo validation script; Editor deploy left to the developer.

## Concrete Steps

See git history and [Docs/LiveOps/RemoteConfig.md](../../Docs/LiveOps/RemoteConfig.md).

## Validation and Acceptance

- [ ] `Window → Deployment` lists all five `.rc` files without import errors.
- [ ] Deploy succeeds for the linked environment.
- [ ] Meta scene Play Mode shows `[Meta] LiveOps ready…` with expected payloads.
- [ ] `.agents/scripts/validate-changes.cmd` passes (when run locally).

## Idempotence and Recovery

- Re-deploying the same `.rc` content overwrites remote keys for that environment; use Dashboard or `ugs` fetch/backup before risky edits.

## Artifacts and Notes

- [Assets/LiveOps/RemoteConfig/](../../Assets/LiveOps/RemoteConfig/)
- [Docs/LiveOps/RemoteConfig.md](../../Docs/LiveOps/RemoteConfig.md)

## Interfaces and Dependencies

- `com.unity.remote-config` @ 4.2.5
- `com.unity.services.deployment` @ 1.7.1
