using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Layers;
using LiveOps.Modules.DTO.Cards;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.Roguelike;
using LiveOps.Modules.DTO.Tracks;
using Scaffold.AppFlow;
using Scaffold.LiveOps;
using UnityEngine;

namespace GearEngine.App.Bootstrap
{
    public sealed class MetaApplicationBootstrap : GearAppFlowRoot
    {
        protected override IEnumerable<IScopeLayer> GetGameLayers()
        {
            yield return new UgsLayer();
            yield return new LiveOpsLayer();
        }

        protected override Task OnReadyAsync(CancellationToken ct)
        {
            LogMetaLiveOpsSmokeTest();
            _ = ct;
            return Task.CompletedTask;
        }

        protected override Task OnStartupFailedAsync(Exception ex, CancellationToken ct)
        {
            Debug.LogError($"[Meta] Startup failed: {ex.Message}\n{ex.StackTrace}");
            _ = ct;
            return Task.CompletedTask;
        }

        private void LogMetaLiveOpsSmokeTest()
        {
            try
            {
                ILiveOpsService liveOps = Host.Resolve<ILiveOpsService>();
                LogRawPayloadsCore(liveOps);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Meta] OnReadyAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void LogRawPayloadsCore(ILiveOpsService liveOps)
        {
            CurrencyGameData currencyData = liveOps.GetModuleData<CurrencyGameData>();
            TrackGameData trackData = liveOps.GetModuleData<TrackGameData>();
            LoadoutGameData loadoutData = liveOps.GetModuleData<LoadoutGameData>();
            InventoryGameData inventoryData = liveOps.GetModuleData<InventoryGameData>();
            CardGameData cardData = liveOps.GetModuleData<CardGameData>();
            RoguelikeGameData roguelikeData = liveOps.GetModuleData<RoguelikeGameData>();
            Debug.Log($"[Meta] LiveOps raw payloads: CurrencyGameData wallets={(currencyData != null ? currencyData.Wallets.Count : 0)}, TrackGameData={(trackData != null)}, LoadoutGameData={(loadoutData != null)}, InventoryGameData={(inventoryData != null)}, CardGameData={(cardData != null)}, RoguelikeGameData={(roguelikeData != null)}.");
        }
    }
}
