using System;
using Game.GearEngine.Presentation;
using Scaffold.Events;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    /// <summary>
    /// Holds explicit scene references for GearEngine VContainer registration.
    /// Compose this with any <see cref="LifetimeScope"/> by calling <see cref="Install"/> from <c>Configure</c>.
    /// </summary>
    public sealed class GearMechanicsInstaller : MonoBehaviour
    {
        [SerializeField]
        private BoardConfigSO boardConfig;

        [SerializeField]
        private GearBootstrap bootstrap;

        [SerializeField]
        private GearInventoryView inventoryView;

        [SerializeField]
        private SimulationControlView simControlView;

        [SerializeField]
        private BoardView boardView;

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (bootstrap == null)
            {
                throw new InvalidOperationException($"{nameof(GearMechanicsInstaller)}: {nameof(bootstrap)} is not assigned.");
            }

            if (inventoryView == null)
            {
                throw new InvalidOperationException($"{nameof(GearMechanicsInstaller)}: {nameof(inventoryView)} is not assigned.");
            }

            if (simControlView == null)
            {
                throw new InvalidOperationException($"{nameof(GearMechanicsInstaller)}: {nameof(simControlView)} is not assigned.");
            }

            if (boardView == null)
            {
                throw new InvalidOperationException($"{nameof(GearMechanicsInstaller)}: {nameof(boardView)} is not assigned.");
            }

            if (boardConfig != null)
            {
                builder.RegisterInstance(boardConfig);
            }

            builder.Register<EventController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GridManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<GearMergeService>(Lifetime.Singleton);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            builder.Register<GearViewFactory>(Lifetime.Singleton);

            builder.Register<GearInventoryViewModel>(Lifetime.Singleton);
            builder.Register<SimulationControlViewModel>(Lifetime.Singleton);

            builder.RegisterComponent(bootstrap);
            builder.RegisterComponent(inventoryView);
            builder.RegisterComponent(simControlView);
            builder.RegisterComponent(boardView);
        }
    }
}
