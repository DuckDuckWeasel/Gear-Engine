using Game.CarSimulation;
using Game.GearEngine;
using Game.GearEngine.Presentation;
using Scaffold.Events;
using Scaffold.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Game.Race.Navigation;

namespace Game.Race
{
    public class RaceScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;
        [SerializeField]
        private Transform navigationViewHolder;
        [SerializeField]
        private ViewConfig trackPreviewViewConfig;
        [SerializeField]
        private ViewConfig raceViewConfig;

        [Header("Car Simulation")]
        [SerializeField]
        private CarDefinition carDefinition;
        [SerializeField]
        private TrackDefinition trackDefinition;
        [SerializeField]
        private Track track;

        [Header("Gear Engine")]
        [SerializeField]
        private BoardConfigSO boardConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            if (navigationViewHolder == null)
            {
                throw new System.InvalidOperationException("[RaceScope] navigationViewHolder must be assigned.");
            }

            RaceNavigationInstaller.Install(builder, navigationSettings, navigationViewHolder);

            builder.RegisterInstance(new TrackPreviewViewConfigRef(trackPreviewViewConfig));
            builder.RegisterInstance(new RaceViewConfigRef(raceViewConfig));

            builder.Register<CarFactory>(Lifetime.Singleton);
            builder.RegisterInstance(carDefinition);
            builder.RegisterInstance(trackDefinition);
            builder.RegisterComponent(track);
            builder.RegisterEntryPoint<CarTrackBootstrap>()
                .AsImplementedInterfaces()
                .AsSelf();

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
            builder.RegisterComponentInHierarchy<GearBootstrap>();
            builder.RegisterComponentInHierarchy<GearInventoryView>();
            builder.RegisterComponentInHierarchy<SimulationControlView>();
            builder.RegisterComponentInHierarchy<BoardView>();

            builder.Register<TrackPreviewViewModel>(Lifetime.Singleton);
            builder.Register<RaceViewModel>(Lifetime.Singleton);

            builder.RegisterEntryPoint<RaceNavigationStartup>();
        }
    }
}
