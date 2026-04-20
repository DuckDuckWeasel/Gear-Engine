# Roguelike Mode Vertical Slice

This ExecPlan is a living document.

## Purpose / Big Picture

Deliver a server-authoritative roguelike gear-offer flow: draw N options from a configurable pool, persist the current roll so players cannot re-roll by quitting, pick one gear, and persist owned inventory via the existing race-inventory hookup.

## Progress

- [x] LiveOps DTOs (`RoguelikeConfig`, persistence, game data, draw/claim requests).
- [x] Cloud Code module + random selection strategy + draw/claim handlers; `ModuleConfig` registration.
- [x] Remote Config `Roguelike.rc`.
- [x] Unity `RoguelikeClientModule`, `CampaignRoguelikeInstaller`, `IRoguelikeRollService` / `RoguelikeRollService`, bootstrap wiring.
- [x] `RoguelikeViewModel` / `RoguelikeView` refactor (async roll, capacity-gated confirm, slot rebuild).
- [x] EditMode tests (`RoguelikeViewModelTests`, `RoguelikeClientModuleTests`).
- [x] Documentation (`Docs/LiveOps/Roguelike.md`, `Docs/Game/Roguelike.md`, `RemoteConfig.md`, `Bootstrap.md` changelog).

## Context and Orientation

See [Docs/LiveOps/Roguelike.md](../../Docs/LiveOps/Roguelike.md) and [Docs/Game/Roguelike.md](../../Docs/Game/Roguelike.md).

## Validation and Acceptance

- `dotnet build LiveOps/LiveOps.sln -c Release`
- Repository gate: `.agents/scripts/validate-changes.cmd`

## Interfaces and Dependencies

- `GearCatalogSO`, `CampaignGearPersistenceHookup`, `InventoryClientModule`, `ILiveOpsService`, `GameClientModuleBase<T>`.
