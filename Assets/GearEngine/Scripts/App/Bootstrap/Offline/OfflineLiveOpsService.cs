using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using Scaffold.LiveOps;

namespace GearEngine.App.Bootstrap.Offline
{
    /// <summary>
    /// Dev-only <see cref="ILiveOpsService"/> that returns hand-authored stub data from
    /// <see cref="OfflineStubs"/>. Unknown modules or requests throw with a clear message so
    /// the missing stub is easy to spot and fill in.
    /// </summary>
    public sealed class OfflineLiveOpsService : ILiveOpsService
    {
        private readonly Dictionary<Type, IGameModuleData> modules;
        private readonly Dictionary<Type, Func<ModuleRequest, OfflineLiveOpsService, ModuleResponse>> handlers;

        public OfflineLiveOpsService()
            : this(OfflineStubs.CreateModules(), OfflineStubs.CreateHandlers())
        {
        }

        public OfflineLiveOpsService(
            Dictionary<Type, IGameModuleData> modules,
            Dictionary<Type, Func<ModuleRequest, OfflineLiveOpsService, ModuleResponse>> handlers)
        {
            this.modules = modules ?? throw new ArgumentNullException(nameof(modules));
            this.handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        public T GetModuleData<T>() where T : class, IGameModuleData
        {
            if (modules.TryGetValue(typeof(T), out IGameModuleData data) && data is T typed)
            {
                return typed;
            }

            throw new InvalidOperationException(
                $"[OfflineLiveOps] No stub registered for module '{typeof(T).Name}'. " +
                $"Add an entry to OfflineStubs.CreateModules().");
        }

        public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
            where TResponse : ModuleResponse
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Type requestType = request.GetType();
            if (!handlers.TryGetValue(requestType, out Func<ModuleRequest, OfflineLiveOpsService, ModuleResponse> handler))
            {
                throw new InvalidOperationException(
                    $"[OfflineLiveOps] No stub handler for request '{requestType.Name}'. " +
                    $"Add an entry to OfflineStubs.CreateHandlers().");
            }

            ModuleResponse response = handler(request, this);
            if (response is TResponse typed)
            {
                return Task.FromResult(typed);
            }

            throw new InvalidOperationException(
                $"[OfflineLiveOps] Handler for '{requestType.Name}' returned '{response?.GetType().Name ?? "null"}', " +
                $"expected '{typeof(TResponse).Name}'.");
        }
    }
}
