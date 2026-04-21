# Cards (`Game.Cards`)

Runtime module for card catalog, LiveOps-backed random unlock purchases (`CardsClientModule`), and optional powerup resolution.

## Scene sample

- **Scene**: [`Assets/GearEngine/Scenes/Cards Scene.unity`](../../Assets/GearEngine/Scenes/Cards%20Scene.unity)
- **Scope**: `GearEngine.Cards.Bootstrap.CardsScope` (`SceneFoundationScope`)
- **Bootstrap**: `GearEngine.Cards.Bootstrap.CardsBootstrap` warms `CurrencyClientModule` + `CardsClientModule`, then opens `CardSampleViewModel` via navigation
- **Parent scope**: `CardsScope` **must** have `Parent` set to the Meta / application root that runs `LiveOpsLayer` (provides `ILiveOpsService`, `CurrencyClientModule`, `CardsClientModule`).
- **UI**: `GearEngine.Cards.Presentation.CardSampleView` (prefab `Assets/GearEngine/Prefabs/Cards/CardSampleViewRoot.prefab`) — wire `goldLabel`, `nextCostLabel`, `unlockedListContainer`, optional `purchaseButton`, optional `unlockedRowTemplate`.

Assign a `CardCatalogSO` on the scope (e.g. example catalog under `Assets/GearEngine/Data/Cards/Examples/`). Remote config `Card.rc` must list card ids that match catalog entries for server rolls.

## Related

- [`SceneFoundation.md`](SceneFoundation.md) — shared scene DI pattern
- [`Architecture.md`](../Architecture.md) — module boundaries
