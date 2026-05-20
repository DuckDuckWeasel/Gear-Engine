using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using Scaffold.LiveOps;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Offline
{
    /// <summary>
    /// In-memory <see cref="ILiveOpsService"/> used when offline mode is enabled. Returns module data
    /// built from local <c>ConfigBuilderSO</c> assets and synthesizes responses for known requests so
    /// the game can run without Unity Gaming Services.
    /// </summary>
    public sealed class OfflineLiveOpsService : ILiveOpsService
    {
        private readonly Dictionary<Type, IGameModuleData> modulesByType;
        private readonly OfflineRequestRouter router;

        public OfflineLiveOpsService(Dictionary<Type, IGameModuleData> modulesByType)
        {
            this.modulesByType = modulesByType ?? throw new ArgumentNullException(nameof(modulesByType));
            router = new OfflineRequestRouter(this);
        }

        public T GetModuleData<T>() where T : class, IGameModuleData
        {
            if (modulesByType.TryGetValue(typeof(T), out IGameModuleData data))
            {
                return data as T;
            }

            return null;
        }

        public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
            where TResponse : ModuleResponse
        {
            try
            {
                if (request == null)
                {
                    return Task.FromResult<TResponse>(null);
                }

                cancellationToken.ThrowIfCancellationRequested();

                ModuleResponse response = router.Route(request);
                if (response is TResponse typed)
                {
                    return Task.FromResult(typed);
                }

                return Task.FromResult(CreateDefaultResponse<TResponse>());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OfflineLiveOps] CallAsync({request?.GetType().Name}) failed: {ex.Message}\n{ex.StackTrace}");
                return Task.FromResult(CreateDefaultResponse<TResponse>());
            }
        }

        internal bool TryGetModule<T>(out T data) where T : class, IGameModuleData
        {
            data = GetModuleData<T>();
            return data != null;
        }

        private static TResponse CreateDefaultResponse<TResponse>() where TResponse : ModuleResponse
        {
            try
            {
                return Activator.CreateInstance<TResponse>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OfflineLiveOps] Could not construct default response for {typeof(TResponse).Name}: {ex.Message}");
                return null;
            }
        }
    }
}
