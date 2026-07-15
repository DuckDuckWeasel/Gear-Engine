using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Extensions
{
    public static class VContainerExtensions
    {
        /// <summary>
        /// Attempts to inject dependencies into the target object using the active LifetimeScope.
        /// Useful for dynamically created objects (e.g. Fungus commands) that bypass normal injection.
        /// </summary>
        public static void TryInject(this object target)
        {
            if (target == null) return;

            var scope = Object.FindObjectOfType<LifetimeScope>();
            if (scope != null && scope.Container != null)
            {
                try
                {
                    scope.Container.Inject(target);
                }
                catch
                {
                    // Ignore injection exceptions
                }
            }
        }
    }
}
