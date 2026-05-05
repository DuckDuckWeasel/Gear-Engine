using Scaffold.Ads.Levelplay;
using Scaffold.AppFlow;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class AdsLayer : IScopeLayer
    {
        public AdsLayer(LevelPlayAdConfigurationSO adConfig)
        {
            this.adConfig = adConfig;
        }

        private readonly LevelPlayAdConfigurationSO adConfig;

        public void Install(IContainerBuilder builder)
        {
#if UNITY_EDITOR
            // Use the mock ad system in the Unity Editor
            var mockConfig = UnityEngine.ScriptableObject.CreateInstance<MockAds.MockAdConfigurationSO>();
            new Scaffold.Ads.AdsInstaller(mockConfig).Install(builder);
            
            builder.Register<MockAds.MockAdsClientModule>(Lifetime.Singleton).As<IAsyncInitializable>();
#else
            if (adConfig != null)
            {
                new LevelPlayInstaller(adConfig).Install(builder);
                builder.Register<AdsClientModule>(Lifetime.Singleton).As<IAsyncInitializable>();
            }
#endif
        }
    }
}
