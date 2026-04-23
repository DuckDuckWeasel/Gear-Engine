using System;
using GearEngine.Currency.Bootstrap;
using Scaffold.AppFlow;
using Scaffold.CloudCode.Container;
using Scaffold.LiveOps.Container;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap.Layers
{
    /// <summary>
    /// Cloud Code + <see cref="Scaffold.LiveOps.ILiveOpsService"/> registration and initial game-data fetch.
    /// Pushed before <see cref="LiveOpsClientModulesLayer"/> so <c>GetModuleData&lt;T&gt;()</c> is populated when client modules initialize.
    /// </summary>
    public sealed class LiveOpsServiceLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            new CloudCodeInstaller().Install(builder);
            new LiveOpsInstaller().Install(builder);
        }
    }

    /// <summary>
    /// LiveOps-backed client modules (currency, tracks, gear, etc.) that read hydrated data from <see cref="Scaffold.LiveOps.ILiveOpsService"/>.
    /// </summary>
    public sealed class LiveOpsClientModulesLayer : IScopeLayer
    {
        public LiveOpsClientModulesLayer(params IInstaller[] gameClientInstallers)
        {
            this.gameClientInstallers = gameClientInstallers ?? Array.Empty<IInstaller>();
        }

        private readonly IInstaller[] gameClientInstallers;

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            new CurrencyClientInstaller().Install(builder);

            foreach (IInstaller installer in gameClientInstallers)
            {
                installer?.Install(builder);
            }
        }
    }
}
