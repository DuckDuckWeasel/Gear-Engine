using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public sealed class CarTrackScope : LifetimeScope
    {
        [SerializeField] private CarTrackBootstrap sceneBootstrap;

        protected override void Configure(IContainerBuilder builder)
        {
            new CarTrackInstaller().Install(builder);

            if (sceneBootstrap != null)
            {
                builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
            }
        }
    }
}
