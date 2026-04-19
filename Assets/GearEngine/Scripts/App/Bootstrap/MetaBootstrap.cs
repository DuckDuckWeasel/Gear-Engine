using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Ads;
using GameModuleDTO.Modules.Gold;
using GearEngine.App.Bootstrap.Presentation;
using Scaffold.LiveOps;
using Scaffold.Navigation.Contracts;
using Scaffold.Scope.Contracts;
using Scaffold.Ugs;
using Unity.Services.CloudCode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap
{
    public sealed class MetaBootstrap : MonoBehaviour, IInitializable
    {

        [Inject] private IObjectResolver resolver;
        [Inject] private Ugs ugs;
        [Inject] private ILiveOpsService liveOps;
        [Inject] private INavigation navigation;



        public async void Initialize()
        {
            try
            {
                await RunStartupAsync(destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MetaBootstrap] Startup failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task RunStartupAsync(CancellationToken cancellationToken)
        {
            await ugs.InitializeAsync(resolver, cancellationToken).ConfigureAwait(true);
            await InitializeNonUgsLayersAsync(cancellationToken).ConfigureAwait(true);
            LogModuleSnapshot();
            //navigation.Open(new MetaLoadedViewModel());
        }

        private async Task InitializeNonUgsLayersAsync(CancellationToken cancellationToken)
        {
            List<IAsyncLayerInitializable> layerInitializers = resolver.Resolve<IEnumerable<IAsyncLayerInitializable>>().ToList();
            layerInitializers.Sort((a, b) => string.CompareOrdinal(a.GetType().FullName, b.GetType().FullName));
            foreach (IAsyncLayerInitializable initializer in layerInitializers)
            {
                if (initializer is Ugs)
                {
                    continue;
                }

                await initializer.InitializeAsync(resolver, cancellationToken).ConfigureAwait(true);
                Debug.Log($"[MetaBootstrap] Layer initialized: {initializer.GetType().FullName}.");
            }
        }

        private void LogModuleSnapshot()
        {
            GoldGameData goldData = liveOps.GetModuleData<GoldGameData>();
            AdData adData = liveOps.GetModuleData<AdData>();
            Debug.Log($"[MetaBootstrap] LiveOps game data ready. GoldGameData={(goldData != null)}, AdData={(adData != null)}.");
        }

    }
}
