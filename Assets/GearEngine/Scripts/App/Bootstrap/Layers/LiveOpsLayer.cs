using Scaffold.Ads;
using Scaffold.AppFlow;
using Scaffold.CloudCode.Container;
using Scaffold.LiveOps.Container;
using Scaffold.Ads.Levelplay;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    // todo: Cloud Code + LiveOps only; catalog Addressables load in FoundationLayer via layer asset publishers (AssetPublisherDefinition).
    public sealed class LiveOpsLayer : IScopeLayer
    {
        public LiveOpsLayer(AdConfigurationSO adConfig)
        {
            this.adConfig = adConfig;
        }
        
        private readonly AdConfigurationSO adConfig;
        
        public void Install(IContainerBuilder builder)
        {
            new CloudCodeInstaller().Install(builder);
            new LiveOpsInstaller().Install(builder);
            
            if (adConfig != null)
            {
                if (adConfig is LevelPlayAdConfigurationSO levelPlayConfig)
                {
                    new LevelPlayInstaller(levelPlayConfig).Install(builder);
                }
                else
                {
                    Debug.LogWarning("[LiveOpsLayer] Passed AdConfigurationSO is not a LevelPlayAdConfigurationSO! Ads will not be installed.");
                }
            }
            else
            {
                Debug.LogWarning("[LiveOpsLayer] LevelPlayAdConfigurationSO is null! Ads will not be installed.");
            }
        }
    }
}
