using System;
using System.Collections.Generic;
using GearEngine.App.Bootstrap.Publishers.DataDriven;
using Scaffold.Addressables;
using Scaffold.Addressables.Contracts;
using Scaffold.AppFlow;
using Scaffold.Events.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Navigation.Contracts;
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
            var assetClient = new AddressablesAssetClient();
            var refHandler = new AddressablesAssetReferenceHandler(assetClient);
            builder.RegisterInstance<IAddressablesAssetClient>(assetClient);
            builder.RegisterInstance<IAssetReferenceHandler>(refHandler);
            builder.Register<AddressablesGateway>(Lifetime.Singleton)
                .WithParameter<IAddressablesAssetClient>(assetClient)
                .WithParameter<IAssetReferenceHandler>(refHandler)
                .As<IAddressablesGateway>()
                .As<IAsyncInitializable>();

            builder.RegisterInstance(navigationSettings);
            new NavigationInstaller(navigationViewHolder).Install(builder);
            builder.Register<NoViewControllerDependencyInjector>(Lifetime.Singleton)
                .As<IViewControllerDependencyInjector>();
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

        private sealed class NoViewControllerDependencyInjector : IViewControllerDependencyInjector
        {
            public void Inject(IViewController controller)
            {
            }
        }
    }
}
