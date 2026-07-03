using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Scaffold.AppFlow;

namespace GearEngine.App.Bootstrap.Layers
{
    internal sealed class MinimumDelayTask : IAsyncInitializable
    {
        private readonly float _startupTime;
        private readonly float _minimumLoadingTimeSeconds;

        public MinimumDelayTask(float startupTime, float minimumLoadingTimeSeconds)
        {
            _startupTime = startupTime;
            _minimumLoadingTimeSeconds = minimumLoadingTimeSeconds;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            float elapsed = Time.realtimeSinceStartup - _startupTime;
            if (elapsed < _minimumLoadingTimeSeconds)
            {
                int delayMs = Mathf.RoundToInt((_minimumLoadingTimeSeconds - elapsed) * 1000f);
                await Task.Delay(delayMs, cancellationToken);
            }
        }
    }
}
