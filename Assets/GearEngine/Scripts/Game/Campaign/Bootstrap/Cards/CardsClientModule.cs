using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Cards;
using GameModuleDTO.ModuleRequests;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.Cards
{
    public sealed class CardsClientModule : GameClientModuleBase<CardGameData>
    {
        private readonly ILiveOpsService liveOpsService;

        public CardsClientModule(IObjectResolver resolver, ILiveOpsService liveOps)
            : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
        }

        public IReadOnlyList<string> Unlocked => data?.Unlocked;

        public long NextCost => data?.NextCost ?? 0;

        public string CurrencyId => data?.CurrencyId ?? string.Empty;

        public async Task<PurchaseCardResponse> PurchaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                PurchaseCardResponse resp = await liveOpsService.CallAsync(new PurchaseCardRequest(), cancellationToken);
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
