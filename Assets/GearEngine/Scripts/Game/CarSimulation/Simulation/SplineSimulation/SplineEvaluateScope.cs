using System;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.SplineSimulation
{
    /// <summary>
    /// LifetimeScope for the spline-evaluate test scene. Registers the config
    /// assets as instances and delegates service registration to
    /// <see cref="SplineEvaluateInstaller"/>.
    /// </summary>
    public sealed class SplineEvaluateScope : SceneFoundationScope
    {
        [Header("Scene Bootstrap")]
        [SerializeField] private SplineEvaluateBootstrap sceneBootstrap;

        [Header("Simulation Config")]
        [SerializeField] private SplineDriverConfig driverConfig;

        [Header("Default Lane Profile (optional)")]
        [SerializeField] private LaneProfile defaultLaneProfile;

        protected override void ValidateSceneAssignments()
        {
            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[SplineEvaluateScope] Assign sceneBootstrap.");
            }

            if (driverConfig == null)
            {
                throw new InvalidOperationException("[SplineEvaluateScope] Assign SplineDriverConfig.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(driverConfig);

            new SplineEvaluateInstaller().Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();

            // Wire optional lane profile after container build
            LaneProfile capturedProfile = defaultLaneProfile;
            if (capturedProfile != null)
            {
                builder.RegisterBuildCallback(resolver =>
                {
                    var service = resolver.Resolve<SplineEvaluateRunnerService>();
                    service.SetDefaultLaneProfile(capturedProfile);
                });
            }
        }
    }
}
