using System;
using System.Collections.Generic;
using Scaffold.AppFlow;
using Scaffold.AppFlow.Publishers.DataDriven;
using Scaffold.Navigation;
using UnityEngine;
using GearEngine.App.Bootstrap.Layers;

namespace GearEngine.App.Bootstrap
{
    public abstract class GearAppFlowRoot : AppFlowRoot
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;

        [SerializeField]
        private Transform navigationViewHolder;

        [Header("Addressable catalog publishers")]
        [Tooltip("Rebaked AddressableScriptableObjectPublisherSO assets (Track / Gear / Roguelike). Registered in FoundationLayer before UGS/LiveOps. Campaign: assign all catalogs. Meta: use the same set as campaign when probing client data.")]
        [SerializeField]
        private List<AddressableScriptableObjectPublisherSO> addressableCatalogPublishers = new List<AddressableScriptableObjectPublisherSO>();

        protected sealed override IInLayerScheduler CreateScheduler()
        {
            return new SequentialInLayerScheduler();
        }

        protected sealed override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            yield return new FoundationLayer(navigationSettings, navigationViewHolder, addressableCatalogPublishers);
            foreach (IScopeLayer layer in GetGameLayers())
            {
                yield return layer;
            }
        }

        protected abstract IEnumerable<IScopeLayer> GetGameLayers();
    }
}
