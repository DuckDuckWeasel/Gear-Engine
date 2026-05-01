using System;
using System.Linq;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;

namespace LiveOps.Modules.Perks
{
    public sealed class PurchasePerkHandler : IGameApiHandler<PurchasePerkRequest, PurchasePerkResponse>
    {
        public async Task<PurchasePerkResponse> HandleAsync(GameApiSession session, PurchasePerkRequest request)
        {
            PerkConfig config = await session.RemoteConfig.Get(session.Context, PerksModule.ConfigKey, new PerkConfig());
            PerkPersistence persistence = await session.Player.Get(session.Context, PerksModule.PersistenceKey, new PerkPersistence());

            int uniquePerksCount = persistence.Unlocked.Distinct().Count();

            if (config.Catalog == null || config.Catalog.Count == 0)
            {
                return new PurchasePerkResponse { Success = false, NextCost = config.CostFor(uniquePerksCount), Cost = 0 };
            }

            long cost = config.CostFor(uniquePerksCount);

            SpendCurrencyResponse spend = await session.InvokeAsync<SpendCurrencyRequest, SpendCurrencyResponse>(
                new SpendCurrencyRequest("gold", cost));

            if (spend == null || !spend.Succeeded)
            {
                return new PurchasePerkResponse { Success = false, NextCost = cost, Cost = cost };
            }

            // Pick any random Perk from the catalog — duplicates are intentionally allowed.
            string newId = config.Catalog[new Random().Next(config.Catalog.Count)];
            persistence.Unlocked.Add(newId);

            await session.Player.Set(session.Context, PerksModule.PersistenceKey, persistence);

            return new PurchasePerkResponse
            {
                Success = true,
                UnlockedPerkId = newId,
                Cost = cost,
                NextCost = config.CostFor(persistence.Unlocked.Distinct().Count()),
                NewGoldBalance = spend.NewAmount
            };
        }
    }
}

