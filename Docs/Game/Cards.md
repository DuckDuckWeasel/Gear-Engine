# Cards (`Game.Cards`)

Runtime module for card catalog, slot inventory, local purchase stub, and optional powerup resolution.

## Scene sample

- **Scene**: [`Assets/GearEngine/Scenes/Cards Scene.unity`](../../Assets/GearEngine/Scenes/Cards%20Scene.unity)
- **Scope**: `GearEngine.Cards.Bootstrap.CardsScope` (`SceneFoundationScope`)
- **Bootstrap**: `GearEngine.Cards.Bootstrap.CardsBootstrap` opens `CardSampleViewModel` via navigation
- **UI**: `GearEngine.Cards.Presentation.CardSampleView` (prefab `Assets/GearEngine/Prefabs/Cards/CardSampleViewRoot.prefab`)

Assign a `CardCatalogSO` on the scope (e.g. example catalog under `Assets/GearEngine/Data/Cards/Examples/`).

## Related

- [`SceneFoundation.md`](SceneFoundation.md) — shared scene DI pattern
- [`Architecture.md`](../Architecture.md) — module boundaries
