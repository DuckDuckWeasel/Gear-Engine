using System;
using GearEngine.SceneFoundation.Bootstrap;
using Scaffold.CloudCode.Container;
using Scaffold.LiveOps.Container;
using Scaffold.Ugs.Container;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap
{
    public sealed class MetaScope : SceneFoundationScope
    {
        [Header("Bootstrap")]
        [SerializeField]
        private MetaBootstrap sceneBootstrap;

        protected override void ValidateSceneAssignments()
        {
            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[MetaScope] Assign sceneBootstrap.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            new UgsInstaller().Install(builder);
            RegisterUnityCloudCodeSdk(builder);
            new CloudCodeInstaller().Install(builder);
            new LiveOpsInstaller().Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private void RegisterUnityCloudCodeSdk(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Register(_ => global::Unity.Services.CloudCode.CloudCodeService.Instance, Lifetime.Singleton)
                .As<global::Unity.Services.CloudCode.ICloudCodeService>();
        }
    }
}
