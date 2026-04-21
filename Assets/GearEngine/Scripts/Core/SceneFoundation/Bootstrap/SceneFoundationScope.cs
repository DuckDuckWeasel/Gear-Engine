using System;
using Scaffold.Addressables.Container;
using Scaffold.Events.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.SceneFoundation.Bootstrap
{
    public abstract class SceneFoundationScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;

        [SerializeField]
        private Transform navigationViewHolder;

        protected sealed override void Configure(IContainerBuilder builder)
        {
            ValidateFoundationAssignments();
            ValidateSceneAssignments();
            InstallFoundation(builder);
            InstallFeatureServices(builder);
        }

        protected virtual void ValidateSceneAssignments()
        {
        }

        protected abstract void InstallFeatureServices(IContainerBuilder builder);

        private void ValidateFoundationAssignments()
        {
            RequireNavigationSettings();
            RequireNavigationViewHolder();
        }

        private void RequireNavigationSettings()
        {
            if (navigationSettings == null)
            {
                throw new InvalidOperationException(
                    $"[{GetType().Name}] Assign navigationSettings (e.g. Assets/Navigation/Navigation Settings.asset).");
            }
        }

        private void RequireNavigationViewHolder()
        {
            if (navigationViewHolder == null)
            {
                throw new InvalidOperationException(
                    $"[{GetType().Name}] Assign navigationViewHolder to the transform that parents the scene context view.");
            }
        }

        private void InstallFoundation(IContainerBuilder builder)
        {
            new AddressablesInstaller().Install(builder);
            builder.RegisterInstance(navigationSettings);
            new NavigationInstaller(navigationViewHolder).Install(builder);
            new EventsInstaller().Install(builder);
        }
    }
}
