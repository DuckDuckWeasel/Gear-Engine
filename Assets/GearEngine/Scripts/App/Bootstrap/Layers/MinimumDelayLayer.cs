using Scaffold.AppFlow;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class MinimumDelayLayer : IScopeLayer
    {
        public string Name => "Finishing up";

        private readonly float _startupTime;
        private readonly float _minimumLoadingTimeSeconds;

        public MinimumDelayLayer(float startupTime, float minimumLoadingTimeSeconds)
        {
            _startupTime = startupTime;
            _minimumLoadingTimeSeconds = minimumLoadingTimeSeconds;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(new MinimumDelayTask(_startupTime, _minimumLoadingTimeSeconds)).As<IAsyncStartable>();
        }
    }
}
