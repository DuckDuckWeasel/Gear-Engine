# TalentCards Feature

## Purpose

The TalentCards screen lets players browse the perks they own and purchase new ones randomly from a backend catalog.

## Architecture

| Layer | File |
|---|---|
| **Backend DTO** | `LiveOps/Game/Perks.DTO/` — `PerkPersistence`, `PerkConfig`, `PerkGameData`, `BuyPerkRequest/Response` |
| **Backend Module** | `LiveOps/Game/Perks/` — `PerksModule` (initialize), `BuyPerkHandler` (action) |
| **Client Module** | `Assets/…/Bootstrap/Perks/PerksClientModule.cs` — thin LiveOps client, exposes `Owned` and `BuyAsync` |
| **Installer** | `PerksClientInstaller.cs` — registers `PerksClientModule` as singleton in VContainer |
| **ViewModel** | `TalentCardsViewModel.cs` — loads perks on init, exposes `Items` list and `BuyRandom()` |
| **Item ViewModel** | `PerkItemViewModel.cs` — one entry per distinct perk ID, holds observable `Count` |
| **View** | `TalentCardsView.cs` — attached to the `TalentCards View` prefab; instantiates `PerkCardView` slots |
| **Card Slot View** | `PerkCardView.cs` — `ViewComponent<PerkItemViewModel>`; attach to the `Card_View` prefab |
| **Tests** | `TalentCardsViewModelTests.cs` (EditMode) |

## Data Flow

```
Backend (Cloud Code)
  PerksModule.InitializeAsync → returns PerkGameData { Owned[], BuyCost }
  BuyPerkHandler              → spends gold, picks random perk, persists, returns BuyPerkResponse

Client
  PerksClientModule.InitializeAsync → fetches PerkGameData
  TalentCardsViewModel.Initialize   → calls InitializeAsync, groups Owned by ID into PerkItemViewModels
  TalentCardsView                   → observes ItemsRevision, rebuilds PerkCardView list
  [Buy Button click]
    → TalentCardsViewModel.BuyRandom()
    → PerksClientModule.BuyAsync()
    → AddOrIncrementItem(acquiredPerkId)
    → ItemsRevision++ → view rebuilds
```

## Prefab Setup Checklist

1. Open `Assets/GearEngine/Prefabs/Campaign/TalentCards View.prefab`.
2. Add the `TalentCardsView` component to the root GameObject.
3. Assign in the Inspector:
   - **Card Container** — the `RectTransform` / `ScrollView Content` that will hold card instances.
   - **Card Prefab** — a prefab with `PerkCardView` attached (can be a copy of `Card_View.prefab`).
   - **Buy Button** — the UI `Button` to trigger random perk purchase.
4. On the `Card_View` prefab (or the dedicated perk card prefab):
   - Add `PerkCardView` component.
   - Assign **Perk Id Label** (TMP text showing the perk key).
   - Assign **Count Label** (TMP text showing `x{count}`).
5. Register `PerksClientInstaller` in the appropriate `LifetimeScope` (same parent that registers other LiveOps modules).
6. Register `TalentCardsViewModel` as `Transient` in the scene scope that opens this view.

## Backend Remote Config

Create a `PerkConfig` Remote Config entry with JSON:
```json
{
  "catalog": ["speed_boost", "grip_multiplier", "nitro"],
  "buyCost": 100
}
```
