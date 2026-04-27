using GearEngine.SplineEvaluate.Simulation;
using VContainer;
using VContainer.Unity;

namespace GearEngine.SplineEvaluate.Bootstrap
{
    /// <summary>
    /// Pure service registration for the spline-evaluate feature.
    /// No scene references, no PROMETEO dependency.
    /// </summary>
    public sealed class SplineEvaluateInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.RegisterEntryPoint<SplineEvaluateRunnerService>(Lifetime.Singleton).AsSelf();
        }
    }
}
