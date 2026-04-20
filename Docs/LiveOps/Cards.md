# Cards module (LiveOps)

## TL;DR

- **DTO**: `CardConfig` (`catalog`, `baseCost`, `costPerPurchaseGrowth`), `CardPersistence` (`unlocked` ids), `CardGameData`, `PurchaseCardRequest` / `PurchaseCardResponse` (`[UsesGameApi]`). Card purchases always use **`gold`** (no `currencyId` in config).
- **Cloud Code**: `CardsModule` (`Initialize`); `PurchaseCardHandler` spends via nested `session.InvokeAsync<SpendCurrencyRequest, SpendCurrencyResponse>` (`"gold"`) then unlocks a random remaining catalog id.
- **Unity**: `CardsClientModule` in `Game.App.Bootstrap` (`GearEngine.App.Bootstrap.Cards`), registered via `CardsClientInstaller` in `LiveOpsLayer`; nested `SpendCurrencyResponse` reconciles `CurrencyClientModule` via existing handlers.
- **Remote Config**: [Card.rc](../../Assets/LiveOps/RemoteConfig/Card.rc).

## References

- [NewApiAndServices.md](NewApiAndServices.md)
