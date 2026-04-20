using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Cards;
using GameModuleDTO.Modules.Currency;

namespace GameModule.Modules.Cards
{
    public sealed class PurchaseCardHandler : IGameApiHandler<PurchaseCardRequest, PurchaseCardResponse>
    {
        public async Task<PurchaseCardResponse> HandleAsync(GameApiSession session, PurchaseCardRequest request)
        {
            CardConfig config = await session.RemoteConfig.Get(session.Context, CardsModule.ConfigKey, new CardConfig()).ConfigureAwait(false);
            CardPersistence persistence = await session.Player.Get(session.Context, CardsModule.PersistenceKey, new CardPersistence()).ConfigureAwait(false);

            long cost = config.CostFor(persistence.Unlocked.Count);
            List<string> available = config.Catalog
                .Where(id => !string.IsNullOrEmpty(id) && !persistence.Unlocked.Contains(id))
                .ToList();
            if (available.Count == 0)
            {
                return new PurchaseCardResponse { Success = false, NextCost = cost, Cost = cost };
            }

            SpendCurrencyResponse spend = await session.InvokeAsync<SpendCurrencyRequest, SpendCurrencyResponse>(
                new SpendCurrencyRequest(config.CurrencyId, cost)).ConfigureAwait(false);
            if (spend == null || !spend.Succeeded)
            {
                return new PurchaseCardResponse { Success = false, NextCost = cost, Cost = cost };
            }

            string newId = available[new Random().Next(available.Count)];
            persistence.Unlocked.Add(newId);

            return new PurchaseCardResponse
            {
                Success = true,
                UnlockedCardId = newId,
                Cost = cost,
                NextCost = config.CostFor(persistence.Unlocked.Count),
            };
        }
    }
}
