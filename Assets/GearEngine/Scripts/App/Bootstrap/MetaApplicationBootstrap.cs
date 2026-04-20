using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Ads;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.Modules.Gold;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.LayeredScope;
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
            yield return new LiveOpsLayer();
        }

        protected override Task OnReadyAsync(CancellationToken ct)
        {
            try
            {
                ILiveOpsService liveOps = Host.Resolve<ILiveOpsService>();
                GoldGameData goldData = liveOps.GetModuleData<GoldGameData>();
                AdData adData = liveOps.GetModuleData<AdData>();
                CurrencyGameData currencyData = liveOps.GetModuleData<CurrencyGameData>();
                Debug.Log($"[Meta] LiveOps ready. GoldGameData={(goldData != null)}, AdData={(adData != null)}, CurrencyGameData wallets={(currencyData != null ? currencyData.Wallets.Count : 0)}.");
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
