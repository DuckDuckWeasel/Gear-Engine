using Scaffold.AppFlow;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap.Offline
{
    /// <summary>
    /// Replacement for <c>UgsLayer</c> + <c>LiveOpsLayer</c> when offline mode is enabled. Installs a
    /// hand-authored <see cref="OfflineLiveOpsService"/> stub so the game runs without UGS / LiveOps.
    /// Edit <see cref="OfflineStubs"/> to change what the stub returns.
    /// </summary>
    public sealed class OfflineLiveOpsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            Debug.Log("[OfflineLiveOps] Offline mode enabled; serving hand-authored stubs (UGS + LiveOps not initialized).");
            builder.RegisterInstance<ILiveOpsService>(new OfflineLiveOpsService());
        }
    }
}
