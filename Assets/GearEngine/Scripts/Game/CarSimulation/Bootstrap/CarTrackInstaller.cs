using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.SplineSimulation;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Bootstrap
{
    public sealed class CarTrackInstaller
    {
        public void Install(IContainerBuilder builder, SimulationConfigBase config)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            builder.Register<TrackSimulationFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<RaceManagerService>(Lifetime.Singleton).AsSelf();

            switch (config)
            {
                case PhysicsSimulationConfig physics:
                    builder.RegisterInstance(physics);
                    builder.RegisterEntryPoint<SplineCarRunnerService>(Lifetime.Singleton)
                           .As<ISimulationRunnerService>().AsSelf();
                    break;

                case SplineDriverConfig spline:
                    builder.RegisterInstance(spline);
                    builder.RegisterEntryPoint<SplineEvaluateRunnerService>(Lifetime.Singleton)
                           .As<ISimulationRunnerService>().AsSelf();
                    break;

                default:
                    throw new InvalidOperationException(
                        $"[CarTrackInstaller] Unknown simulation config type: {config.GetType().Name}");
            }
        }
    }
}
