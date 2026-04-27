using System;
using GearEngine.SceneFoundation.Bootstrap;
using GearEngine.SplineEvaluate.Definitions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.SplineEvaluate.Bootstrap
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

            if (defaultLaneProfile != null)
            {
                builder.RegisterInstance(defaultLaneProfile);
            }

            new SplineEvaluateInstaller().Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }
    }
}
