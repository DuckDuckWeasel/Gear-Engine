using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Cards;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.Modules.Loadout;
using GameModuleDTO.Modules.Roguelike;
using GameModuleDTO.Modules.Tracks;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.Campaign.Bootstrap.Cards;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine.Config;
using Scaffold.LayeredScope;
using Scaffold.LiveOps;
using Scaffold.Navigation;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap
{
    public sealed class MetaApplicationBootstrap : ApplicationBootstrap
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;

        [SerializeField]
        private Transform navigationViewHolder;

        protected override void ConfigureApplication(IContainerBuilder builder)
        {
        }

        protected override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            if (navigationSettings == null)
            {
                throw new InvalidOperationException(
                    $"[{nameof(MetaApplicationBootstrap)}] Assign navigationSettings (e.g. Assets/Navigation/Navigation Settings.asset).");
            }

            if (navigationViewHolder == null)
            {
                throw new InvalidOperationException(
                    $"[{nameof(MetaApplicationBootstrap)}] Assign navigationViewHolder to the transform that parents the scene context view.");
            }

            yield return new FoundationLayer(navigationSettings, navigationViewHolder);
            yield return new UgsLayer();
            yield return new LiveOpsServiceLayer();
            yield return CreateMetaLiveOpsClientModulesLayer();
        }

        private static LiveOpsClientModulesLayer CreateMetaLiveOpsClientModulesLayer()
        {
            TrackCatalogSO emptyTrack = ScriptableObject.CreateInstance<TrackCatalogSO>();
            emptyTrack.SetRuntimeEntries(Array.Empty<TrackEntry>(), Array.Empty<GearConfig>());
            GearCatalogSO emptyGear = ScriptableObject.CreateInstance<GearCatalogSO>();
            emptyGear.SetRuntimeEntries(Array.Empty<GearConfig>());
            return new LiveOpsClientModulesLayer(
                new CampaignTracksInstaller(emptyTrack),
                new CampaignGearCatalogInstaller(emptyGear),
                new CampaignLoadoutInstaller(),
                new CampaignInventoryInstaller(),
                new CardsClientInstaller(),
                new CampaignRoguelikeInstaller());
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
                    $"[Meta] Inventory: hasSaved={inventoryClient.HasSavedInventory}, gearIdCount={(inventoryData != null ? inventoryData.GearIds.Count : 0)}.");
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
