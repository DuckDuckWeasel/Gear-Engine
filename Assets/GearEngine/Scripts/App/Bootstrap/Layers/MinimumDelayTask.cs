using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Scaffold.AppFlow;

namespace GearEngine.App.Bootstrap.Layers
{
    internal sealed class MinimumDelayTask : IAsyncInitializable
    {
        private readonly float startupTime;
        private readonly float minimumLoadingTimeSeconds;

        public MinimumDelayTask(float startupTime, float minimumLoadingTimeSeconds)
        {
            this.startupTime = startupTime;
            this.minimumLoadingTimeSeconds = minimumLoadingTimeSeconds;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                float elapsed = Time.realtimeSinceStartup - startupTime;
                float remainingSeconds = minimumLoadingTimeSeconds - elapsed;
                if (remainingSeconds <= 0f)
                {
                    return;
                }

                float delayEndsAt = Time.realtimeSinceStartup + remainingSeconds;
                while (Time.realtimeSinceStartup < delayEndsAt)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MinimumDelayTask] Failed while waiting for the minimum loading duration. {exception}");
                throw;
            }
        }
    }
}
