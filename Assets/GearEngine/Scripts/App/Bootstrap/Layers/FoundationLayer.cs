using System;
using System.Collections.Generic;
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
        public FoundationLayer(NavigationSettings navigationSettings, Transform navigationViewHolder, IReadOnlyList<AddressableScriptableObjectPublisherSO> addressableCatalogPublishers = null)
        {
            this.navigationSettings = navigationSettings ?? throw new ArgumentNullException(nameof(navigationSettings));
            this.navigationViewHolder = navigationViewHolder ?? throw new ArgumentNullException(nameof(navigationViewHolder));
            this.addressableCatalogPublishers = addressableCatalogPublishers ?? Array.Empty<AddressableScriptableObjectPublisherSO>();
        }

        private readonly NavigationSettings navigationSettings;
        private readonly Transform navigationViewHolder;
        private readonly IReadOnlyList<AddressableScriptableObjectPublisherSO> addressableCatalogPublishers;

        public void Install(IContainerBuilder builder)
        {
            new AddressablesInstaller().Install(builder);
            new NavigationInstaller(navigationViewHolder, navigationSettings).Install(builder);
            new EventsInstaller().Install(builder);

            for (int i = 0; i < addressableCatalogPublishers.Count; i++)
            {
                AddressableScriptableObjectPublisherSO publisherSo = addressableCatalogPublishers[i];
                if (publisherSo == null)
                {
                    continue;
                }

                publisherSo.Register(builder);
            }
        }
    }
}
