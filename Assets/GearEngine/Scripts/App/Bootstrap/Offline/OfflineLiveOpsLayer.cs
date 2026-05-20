using System.Collections.Generic;
using Scaffold.AppFlow;
using Scaffold.LiveOps;
using Scaffold.LiveOps.Authoring;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap.Offline
{
    /// <summary>
    /// Replacement for <c>UgsLayer</c> + <c>LiveOpsLayer</c> when offline mode is enabled. Registers an
    /// <see cref="OfflineLiveOpsService"/> that serves module data from local <see cref="ConfigBuilderSOBase"/>
    /// assets so the game does not contact Unity Gaming Services.
    /// </summary>
    public sealed class OfflineLiveOpsLayer : IScopeLayer
    {
        private readonly IReadOnlyList<ConfigBuilderSOBase> configBuilders;

        public OfflineLiveOpsLayer(IReadOnlyList<ConfigBuilderSOBase> configBuilders)
        {
            this.configBuilders = configBuilders ?? new List<ConfigBuilderSOBase>();
        }

        public void Install(IContainerBuilder builder)
        {
            Debug.Log("[OfflineLiveOps] Offline mode enabled; skipping UGS + LiveOps initialization.");
            OfflineLiveOpsService service = OfflineLiveOpsServiceBuilder.Build(configBuilders);
            builder.RegisterInstance<ILiveOpsService>(service);
        }
    }
}
