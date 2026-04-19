using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.LayeredScope
{
    public abstract class ApplicationBootstrap : MonoBehaviour
    {
        [SerializeField]
        private LifetimeScope rootScope;

        private readonly TaskCompletionSource<bool> readyTcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected ApplicationHost Host { get; private set; }

        public Task ReadyTask => readyTcs.Task;

        protected virtual async void Start()
        {
            try
            {
                await RunStartupAsync();
            }
            catch (OperationCanceledException oce)
            {
                readyTcs.TrySetCanceled(oce.CancellationToken);
            }
            catch (Exception ex)
            {
                LogStartupFailed(ex);
                readyTcs.TrySetException(ex);
            }
        }

        private async Task RunStartupAsync()
        {
            CancellationToken ct = destroyCancellationToken;
            Host = new ApplicationHost(rootScope, CreateScheduler());
            await Host.InstallAllAsync(GetInitialLayers(), ct);
            await OnReadyAsync(ct);
            readyTcs.TrySetResult(true);
        }

        private static void LogStartupFailed(Exception ex)
        {
            Debug.LogError($"[ApplicationBootstrap] Startup failed: {ex.Message}\n{ex.StackTrace}");
        }

        protected abstract IEnumerable<IScopeLayer> GetInitialLayers();

        protected virtual IInLayerScheduler CreateScheduler() => new ParallelScheduler();

        protected virtual Task OnReadyAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
