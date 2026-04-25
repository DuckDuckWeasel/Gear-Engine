using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
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
            IReadOnlyList<AssetPublisherDefinition> layerAssetPublishers = null,
            CarDefinition defaultRaceCar = null)
        {
            this.navigationSettings = navigationSettings ?? throw new ArgumentNullException(nameof(navigationSettings));
            this.navigationViewHolder = navigationViewHolder ?? throw new ArgumentNullException(nameof(navigationViewHolder));
            this.layerAssetPublishers = layerAssetPublishers ?? Array.Empty<AssetPublisherDefinition>();
            this.defaultRaceCar = defaultRaceCar;
        }

        private readonly NavigationSettings navigationSettings;
        private readonly Transform navigationViewHolder;
        private readonly IReadOnlyList<AssetPublisherDefinition> layerAssetPublishers;
        private readonly CarDefinition defaultRaceCar;

        public void Install(IContainerBuilder builder)
        {
            if (defaultRaceCar != null)
            {
                builder.RegisterInstance(defaultRaceCar);
            }

            new AddressablesInstaller().Install(builder);
            new NavigationInstaller(navigationViewHolder, navigationSettings).Install(builder);
            new EventsInstaller().Install(builder);

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
}
