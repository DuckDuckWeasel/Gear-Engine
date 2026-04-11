using Game.GearEngine.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    public class GearMechanicsScope : LifetimeScope
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private GearBootstrap bootstrap;
        [SerializeField] private GearInventoryLoadoutSO loadout;
        [SerializeField] private GearEngineSceneBootstrap presentationBootstrap;

        protected override void Configure(IContainerBuilder builder)
        {
            var installer = new GearMechanicsInstaller(boardConfig, bootstrap, loadout);
            installer.Install(builder);

            if (presentationBootstrap != null)
            {
                builder.RegisterComponent(presentationBootstrap);
            }
        }
    }
}
