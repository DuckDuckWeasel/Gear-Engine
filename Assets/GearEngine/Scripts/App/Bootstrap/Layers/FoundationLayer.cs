using System;
using System.Collections.Generic;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using GearEngine.Perks.Config;
using Scaffold.Addressables.Container;
using Scaffold.AppFlow;
using Scaffold.AppFlow.Publishers.DataDriven;
using Scaffold.Events.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class FoundationLayer : IScopeLayer
    {
        public FoundationLayer(
            NavigationSettings navigationSettings,
            Transform navigationViewHolder,
            global::GearEngine.SceneFoundation.Presentation.GlobalLoadingOverlay globalLoadingPrefab,
            IReadOnlyList<AssetPublisherDefinition> layerAssetPublishers = null,
            CarDefinition defaultRaceCar = null,
            IReadOnlyList<TrackDefinition> localTracks = null,
            PerkCatalogSO localPerkCatalog = null,
            GearCatalogSO localGearCatalog = null,
            RoguelikeGearPoolSO localRoguelikeGearPool = null)
        {
            this.navigationSettings = navigationSettings ?? throw new ArgumentNullException(nameof(navigationSettings));
            this.navigationViewHolder = navigationViewHolder ?? throw new ArgumentNullException(nameof(navigationViewHolder));
            this.globalLoadingPrefab = globalLoadingPrefab;
            this.layerAssetPublishers = layerAssetPublishers ?? Array.Empty<AssetPublisherDefinition>();
            this.defaultRaceCar = defaultRaceCar;
            this.localTracks = localTracks ?? Array.Empty<TrackDefinition>();
            this.localPerkCatalog = localPerkCatalog;
            this.localGearCatalog = localGearCatalog;
            this.localRoguelikeGearPool = localRoguelikeGearPool;
        }

        private readonly NavigationSettings navigationSettings;
        private readonly Transform navigationViewHolder;
        private readonly global::GearEngine.SceneFoundation.Presentation.GlobalLoadingOverlay globalLoadingPrefab;
        private readonly IReadOnlyList<AssetPublisherDefinition> layerAssetPublishers;
        private readonly CarDefinition defaultRaceCar;
        private readonly IReadOnlyList<TrackDefinition> localTracks;
        private readonly PerkCatalogSO localPerkCatalog;
        private readonly GearCatalogSO localGearCatalog;
        private readonly RoguelikeGearPoolSO localRoguelikeGearPool;

        public void Install(IContainerBuilder builder)
        {
            if (defaultRaceCar != null)
            {
                builder.RegisterInstance(defaultRaceCar);
            }

            new AddressablesInstaller().Install(builder);
            new NavigationInstaller(navigationViewHolder, navigationSettings).Install(builder);
            new EventsInstaller().Install(builder);

            if (globalLoadingPrefab != null)
            {
                new global::GearEngine.SceneFoundation.Bootstrap.GlobalLoadingInstaller(globalLoadingPrefab).Install(builder);
            }

            if (HasCompleteLocalCatalogs())
            {
                new DirectAssetListPublisherRegistrar<TrackDefinition>(localTracks).Register(builder);
                new DirectAssetPublisherRegistrar<PerkCatalogSO>(localPerkCatalog).Register(builder);
                new DirectAssetPublisherRegistrar<GearCatalogSO>(localGearCatalog).Register(builder);
                new DirectAssetPublisherRegistrar<RoguelikeGearPoolSO>(localRoguelikeGearPool).Register(builder);
            }
            else
            {
                for (int i = 0; i < layerAssetPublishers.Count; i++)
                {
                    AssetPublisherDefinition def = layerAssetPublishers[i];
                    if (def == null)
                    {
                        continue;
                    }

                    def.Register(builder);
                }
            }
        }

        private bool HasCompleteLocalCatalogs()
        {
            return localTracks.Count > 0
                && localPerkCatalog != null
                && localGearCatalog != null
                && localRoguelikeGearPool != null;
        }
    }
}
