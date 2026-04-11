using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    public class GearMechanicsScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var installer = GetComponent<GearMechanicsInstaller>();
            if (installer == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GearMechanicsScope)} requires a {nameof(GearMechanicsInstaller)} on the same GameObject.");
            }

            installer.Install(builder);
        }
    }
}
