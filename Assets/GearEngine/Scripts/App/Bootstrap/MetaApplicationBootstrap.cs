using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Cards;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.Roguelike;
using LiveOps.Modules.DTO.Tracks;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.Campaign.Bootstrap.Cards;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine.Config;
using Scaffold.AppFlow;
using Scaffold.LiveOps;
using Scaffold.Navigation;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap
{
    public sealed class MetaApplicationBootstrap : AppFlowRoot
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;

        [SerializeField]
        private Transform navigationViewHolder;

        [Header("Catalogs")]
        [SerializeField]
        private TrackCatalogSO trackCatalog;

        [SerializeField]
        private RoguelikeGearPoolSO roguelikeGearPool;

        [SerializeField]
        private GearCatalogSO gearCatalog;

        protected override void ConfigureApplication(IContainerBuilder builder)
        {
        }

        protected override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            Require(navigationSettings, nameof(navigationSettings));
            Require(navigationViewHolder, nameof(navigationViewHolder));
            Require(trackCatalog, nameof(trackCatalog));
            Require(roguelikeGearPool, nameof(roguelikeGearPool));
            Require(gearCatalog, nameof(gearCatalog));

            yield return new FoundationLayer(navigationSettings, navigationViewHolder);
            yield return new UgsLayer();
            yield return new LiveOpsServiceLayer();
            yield return new LiveOpsClientModulesLayer(
                new CampaignTracksInstaller(trackCatalog),
                new CampaignGearCatalogInstaller(gearCatalog),
                new CampaignInventoryInstaller(),
                new CampaignLoadoutInstaller(),
                new CardsClientInstaller(),
                new CampaignRoguelikeInstaller(roguelikeGearPool));
        }

        private static void Require(UnityEngine.Object value, string name)
        {
            if (value == null)
            {
                throw new InvalidOperationException($"[{nameof(MetaApplicationBootstrap)}] Assign {name}.");
            }
        }

        protected override Task OnReadyAsync(CancellationToken ct)
        {
            try
            {
                ILiveOpsService liveOps = Host.Resolve<ILiveOpsService>();
                CurrencyGameData currencyData = liveOps.GetModuleData<CurrencyGameData>();
                TrackGameData trackData = liveOps.GetModuleData<TrackGameData>();
                LoadoutGameData loadoutData = liveOps.GetModuleData<LoadoutGameData>();
                InventoryGameData inventoryData = liveOps.GetModuleData<InventoryGameData>();
                CardGameData cardData = liveOps.GetModuleData<CardGameData>();
                RoguelikeGameData roguelikeData = liveOps.GetModuleData<RoguelikeGameData>();
                Debug.Log($"[Meta] LiveOps raw payloads: CurrencyGameData wallets={(currencyData != null ? currencyData.Wallets.Count : 0)}, TrackGameData={(trackData != null)}, LoadoutGameData={(loadoutData != null)}, InventoryGameData={(inventoryData != null)}, CardGameData={(cardData != null)}, RoguelikeGameData={(roguelikeData != null)}.");

                TracksClientModule tracksClient = Host.Resolve<TracksClientModule>();
                LoadoutClientModule loadoutClient = Host.Resolve<LoadoutClientModule>();
                InventoryClientModule inventoryClient = Host.Resolve<InventoryClientModule>();
                CardsClientModule cardsClient = Host.Resolve<CardsClientModule>();

                Debug.Log(
                    $"[Meta] Tracks: client hydrated, progressIndex={tracksClient.GetTrackProgress().CurrentTrackIndex}, " +
                    $"currentTrackId='{trackData?.CurrentTrackId ?? string.Empty}', orderedCount={trackData?.OrderedTrackIds?.Count ?? 0}, bestTimeEntries={trackData?.BestTimeSec?.Count ?? 0}.");
                Debug.Log(
                    $"[Meta] Loadout: hasSaved={loadoutClient.HasSavedLoadout}, boardPlacements={(loadoutData != null ? loadoutData.Board.Count : 0)}.");
                Debug.Log(
                    $"[Meta] Inventory: hasSaved={inventoryClient.HasSavedInventory}, gearCount={(inventoryData != null ? inventoryData.Gears.Count : 0)}.");
                Debug.Log(
                    $"[Meta] Cards: unlockedCount={cardsClient.Unlocked?.Count ?? 0}, nextCost={cardsClient.NextCost} (gold).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Meta] OnReadyAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }

            _ = ct;
            return Task.CompletedTask;
        }

        protected override Task OnStartupFailedAsync(Exception ex, CancellationToken ct)
        {
            Debug.LogError($"[Meta] Startup failed: {ex.Message}\n{ex.StackTrace}");
            _ = ct;
            return Task.CompletedTask;
        }
    }
}
