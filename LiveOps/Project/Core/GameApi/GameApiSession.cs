using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Core;

namespace GameModule.GameApi
{
    /// <summary>
    /// Per-request context for GameApi handlers (caches, nested side-effect responses).
    /// </summary>
    public sealed class GameApiSession
    {
        private readonly IServiceProvider _services;
        private readonly List<ModuleResponse> _nested = new List<ModuleResponse>();

        public GameApiSession(IServiceProvider services, IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            Context = context;
            Player = player;
            GameState = gameState;
            RemoteConfig = remoteConfig;
        }

        public IExecutionContext Context { get; }

        public IPlayerData Player { get; }

        public IGameState GameState { get; }

        public IRemoteConfig RemoteConfig { get; }

        public IReadOnlyList<ModuleResponse> Nested => _nested;

        public void EmitSideEffect(ModuleResponse response)
        {
            if (response != null)
            {
                _nested.Add(response);
            }
        }

        public async Task<TRes> InvokeAsync<TReq, TRes>(TReq request)
            where TReq : ModuleRequest<TRes>
            where TRes : ModuleResponse
        {
            IGameApiHandler<TReq, TRes> handler = _services.GetService<IGameApiHandler<TReq, TRes>>();
            if (handler == null)
            {
                throw new InvalidOperationException($"No IGameApiHandler registered for {typeof(TReq).Name}.");
            }

            TRes result = await handler.HandleAsync(this, request).ConfigureAwait(false);
            EmitSideEffect(result);
            return result;
        }
    }
}
