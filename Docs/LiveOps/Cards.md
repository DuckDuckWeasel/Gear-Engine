# Cards module (LiveOps)

## TL;DR

- **DTO**: `CardConfig` (`catalog`, `currencyId`, `baseCost`, `costPerPurchaseGrowth`), `CardPersistence` (`unlocked` ids), `CardGameData`, `PurchaseCardRequest` / `PurchaseCardResponse` (`[UsesGameApi]`).
- **Cloud Code**: `CardsModule` (`Initialize`); `PurchaseCardHandler` spends via nested `session.InvokeAsync<SpendCurrencyRequest, SpendCurrencyResponse>` then unlocks a random remaining catalog id.
- **Unity**: `CardsClientModule` in `Game.App.Bootstrap` (`GearEngine.App.Bootstrap.Cards`), registered via `CardsClientInstaller` in `LiveOpsLayer`; nested `SpendCurrencyResponse` reconciles `CurrencyClientModule` via existing handlers.
- **Remote Config**: [Card.rc](../../Assets/LiveOps/RemoteConfig/Card.rc).

## References

- [NewApiAndServices.md](NewApiAndServices.md)
