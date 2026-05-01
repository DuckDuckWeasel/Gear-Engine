using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.ModuleRequests;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;
using PerkGameData = LiveOps.Modules.DTO.Perks.PerkGameData;
using PurchasePerkResponse = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkResponse;
using PurchasePerkRequest = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkRequest;
using BurnPerkResponse = LiveOps.Modules.DTO.ModuleRequests.BurnPerkResponse;
using BurnPerkRequest = LiveOps.Modules.DTO.ModuleRequests.BurnPerkRequest;

namespace GearEngine.Campaign.Bootstrap.Perks
{
    public class PerksClientModule : GameClientModuleBase<PerkGameData>, IPerksClientModule
    {
        public PerksClientModule(ILiveOpsService liveOps)
            : base(liveOps)
        {
        }

        /// <summary>All perk IDs owned by the player (duplicates = multiple copies).</summary>
        public IReadOnlyList<string> Unlocked => data?.Unlocked;

        public long NextCost => data?.NextCost ?? 0;

        /// <summary>Gold refunded when burning one copy of a perk.</summary>
        public long BurnReward => data?.BurnReward ?? 0;

        public string CurrencyId => "gold";

        /// <summary>Purchases a random perk from the catalog (duplicates allowed).</summary>
        public async Task<PurchasePerkResponse> PurchaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                PurchasePerkResponse resp = await liveOps.CallAsync(new PurchasePerkRequest(), cancellationToken);
                if (resp == null || data == null)
                {
                    return resp;
                }

                if (resp.Success && !string.IsNullOrEmpty(resp.UnlockedPerkId))
                {
                    data.Unlocked.Add(resp.UnlockedPerkId);
                }

                data.NextCost = resp.NextCost;
                return resp;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerksClientModule] PurchaseAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>Burns one copy of the specified perk and awards gold.</summary>
        public async Task<BurnPerkResponse> BurnAsync(string perkId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(perkId))
                {
                    throw new ArgumentException("perkId cannot be null or empty.", nameof(perkId));
                }

                BurnPerkResponse resp = await liveOps.CallAsync(new BurnPerkRequest { PerkId = perkId }, cancellationToken);
                if (resp == null || data == null)
                {
                    return resp;
                }

                if (resp.Success)
                {
                    int idx = data.Unlocked.IndexOf(perkId);
                    if (idx >= 0)
                    {
                        data.Unlocked.RemoveAt(idx);
                    }
                }

                return resp;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerksClientModule] BurnAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}

