# Currency module (LiveOps)

## TL;DR

- **DTO** (shared): `CurrencyConfig` / `CurrencyConfigEntry` (remote config, plain POCO), `CurrencyPersistence` (player save: `balances` map, plain POCO), `CurrencyGameData` (**only** `IGameModuleData` slice in `GameData`), `CurrencyWallet` (`id`, `current`, optional `min`/`max`). Requests: `AddCurrencyRequest`, `SpendCurrencyRequest`; responses: `AddCurrencyResponse`, `SpendCurrencyResponse`, nested `CurrencyChangedResponse`.
- **Cloud Code**: `CurrencyModule` — `Initialize` loads config + persistence by string key (`CurrencyConfig`, `CurrencyPersistence`), runs `CurrencyPersistenceSeeder.SeedAndClampInPlace` (seed missing ids with `initial`, clamp to bounds), persists if dirty, returns `CurrencyGameData`. Endpoints: `AddCurrency`, `SpendCurrency`. Helpers: `AddToPlayer`, `TrySpendFromPlayer`, `SetForPlayer` (for future cross-module use).
- **Unity client**: `CurrencyClientModule` (in `Game.App.Bootstrap`, namespace `GearEngine.Currency`) — hydrates from `ILiveOpsService.GetModuleData<CurrencyGameData>()`, `AddAsync` / `SpendAsync` / `TrySpendAsync` via Cloud Code; updates cached wallet `Current` from server responses. Registered in `LiveOpsLayer` via `CurrencyClientInstaller`.

## Wire shapes

- **Remote config JSON**: `{ "entries": [ { "id": "gold", "initial": 0, "min": 0, "max": 100 } ] }` — omit `min`/`max` when unbounded (compact on the wire).
- **Persistence JSON**: `{ "balances": { "gold": 123 } }` — only stores amounts; server seeds missing keys on `Initialize`.
- **GameData**: `CurrencyGameData` with `wallets` array; each wallet has no `initial` (that stays in config only).

## Unity authoring

- **`CurrencySO`**: per-currency ScriptableObject (id, numeric defaults for Remote Config export, optional **display name / icon / color** for client UI). **`CurrencyConfigBuilderSO`** references an array of `CurrencySO` assets; **`Build()`** still emits plain `CurrencyConfig` / `CurrencyConfigEntry` JSON (no sprites on the wire). **`Apply()`** updates each `CurrencySO`’s numeric fields from a pulled config.

## Non-goals (this milestone)

- Local `Campaign` `LocalWalletService` (removed — campaign uses `CurrencyClientModule`).

## References

- Bootstrap: [Bootstrap.md](Bootstrap.md)
- DTO source: `LiveOps/LiveOps.DTO/Modules/Currency/`
- Cloud Code: `LiveOps/Project/Modules/Currency/CurrencyModule.cs`
- Client: `Assets/GearEngine/Scripts/App/Bootstrap/Currency/`
