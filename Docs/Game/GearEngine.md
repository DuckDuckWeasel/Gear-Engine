# Gear Engine (Game.GearEngine)

## Ownership and initialization

- **`IInventoryService`** — Persisted, server-authoritative list of owned gears (`Owned`, `TryAdd`, `TryRemove`, `InventoryChanged`). In Campaign this is **`InventoryClientModule`** (LiveOps). In race/test scopes without LiveOps, register **`EmptyInventoryService`**. **`GearMechanicsInstaller` does not register** `IInventoryService`; the host scope must register it **before** the installer runs.
- **`IBoardService` / `BoardService`** — Owns **`BoardModel`** (board config reference, simulation flag, and a synced node collection) and all grid mutations (`TryPlace`, `TryMoveBoardGear`, `TryRemoveBoardGear`, `TryDeleteBoardGear`, `LoadLayout`). There is no construction-time seed: **`SetupViewModel`** (Campaign) calls **`LoadLayout`** from **`IGearLoadoutService.GetBoardLayout()`**; **`GearTestSceneBootstrap`** (Gear test scene) calls **`LoadLayout`** from its serialized **`boardSeed`**; **`BoardViewModel.LoadLayout`** is driven from **`RaceStartData.GearEngineData.BoardLayout`** in the race flow. **`TryDeleteBoardGear`** also removes the gear from **`IInventoryService`** (destroyed, not returned). Max placed gears (`TryPlace`) is capped by **`IBoardSlotCapacityProvider.BoardSlotCapacity`** when that value is positive (LiveOps **`LoadoutGameData.BaseSlots`** via **`LoadoutClientModule`**), otherwise by the grid size (**`BoardRulesSO.MaxBoardGears`**).
- **`IBoardSlotCapacityProvider`** — Implemented by **`LoadoutClientModule`** (Campaign) and **`UnlimitedBoardSlotCapacityProvider`** (sandbox **`GearMechanicsScope`** / **`RaceScope`**). Registered before **`GearMechanicsInstaller`**.
- **`IGearLoadoutService`** — Persisted board layout `(gearId, x, y)` plus **`BoardSlotCapacity`** (loadout `baseSlots`); **`LoadoutClientModule`** in Campaign. **`CampaignGearPersistenceHookup`** forwards **`BoardLayoutChanged`** to persist layout only (no inventory bridging).
- **`GearEngineStartData`** — Serializable payload with **`BoardLayoutData`** (e.g. **`RaceStartData.GearEngineData`**). Initial tray contents come from **`IInventoryService.Owned`**, not start data.

## Tray (derived UI)

- **`GearInventoryViewModel`** exposes **`TrayItems`**: multiset **`Owned` minus gears currently on the board** (from **`IBoardService.GetAllNodes()`**). It subscribes to **`InventoryChanged`**, **`GearPlaced`**, and **`GearRemoved`**. There is no separate race inventory or `InventoryModel`.

## Scopes

`CampaignLayer` registers inventory + loadout via LiveOps installers, then registers **`IBoardSlotCapacityProvider`** (via **`LoadoutClientModule`**) before `GearMechanicsInstaller`. `RaceScope` and `GearMechanicsScope` register **`EmptyInventoryService`** and **`UnlimitedBoardSlotCapacityProvider`** before the installer.

## Drag presentation

- **`DragGhostController`** — Instantiates the **`GearVisual`** child from `GearConfigData.ViewPrefab` under the board’s space root (via `BoardViewComponent.GetBoardSpaceRoot()`), applies `RelativeScaleMultiplier` as local scale, and moves the ghost in world space. Used for both inventory drags (`GearInventoryViewComponent`) and board drags (`GearBoardDragHandler`) so the ghost matches placed gears without canvas/world scale ratio math.
- **`DragHandler`** — EventSystem forwarder only: `OnDragBegin` / `OnDragMoved` / `OnDragEnd` with `PointerEventData`, plus `BuildPayload` and drop resolution via `DragTargetFinder`. Inventory slots wire these callbacks to `DragGhostController`; slot and ghost visuals use `GearView.BindForDisplay` with the authored `GearConfigData.ViewPrefab`.

## Standard

See [State-and-Services-Standard.md](../Standards/State-and-Services-Standard.md) for Tier 1/2 boundaries; inventory vs loadout vs session board state follows the two-service model above.
