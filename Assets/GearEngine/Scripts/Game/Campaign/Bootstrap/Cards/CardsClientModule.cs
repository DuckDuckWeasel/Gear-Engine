using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Cards;
using LiveOps.Modules.DTO.ModuleRequests;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.Cards
{
    public sealed class CardsClientModule : GameClientModuleBase<CardGameData>
    {
        public CardsClientModule(ILiveOpsService liveOps)
            : base(liveOps)
        {
        }

        public IReadOnlyList<string> Unlocked => data?.Unlocked;

        public long NextCost => data?.NextCost ?? 0;

        public string CurrencyId => "gold";

        public async Task<PurchaseCardResponse> PurchaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                PurchaseCardResponse resp = await liveOps.CallAsync(new PurchaseCardRequest(), cancellationToken);
                if (resp == null || data == null)
                {
                    return resp;
                }

                if (resp.Success && !string.IsNullOrEmpty(resp.UnlockedCardId))
                {
                    data.Unlocked.Add(resp.UnlockedCardId);
                }

                data.NextCost = resp.NextCost;
                return resp;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardsClientModule] PurchaseAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
