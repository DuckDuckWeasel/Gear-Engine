using System;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.ModuleRequests;
using GearEngine.LayeredScope;
using Scaffold.CloudCode;
using VContainer;

namespace Scaffold.LiveOps
{
    internal sealed class LiveOpsService : ILiveOpsService, IAsyncInitializable
    {
        public LiveOpsService(ICloudCodeService cloudCodeService, IObjectResolver objectResolver)
        {
            if (cloudCodeService == null)
            {
                throw new ArgumentNullException(nameof(cloudCodeService));
            }

            if (objectResolver == null)
            {
                throw new ArgumentNullException(nameof(objectResolver));
            }

            this.cloudCodeService = cloudCodeService;
            this.moduleResponseDispatchService = new ModuleResponseDispatchService(objectResolver);
        }

        private readonly ICloudCodeService cloudCodeService;
        private readonly ModuleResponseDispatchService moduleResponseDispatchService;
        private GameData gameData;

        public T GetModuleData<T>() where T : class, IGameModuleData
        {
            return gameData == null ? null : gameData.GetModuleData<T>();
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return LoadInitialGameDataAsync(cancellationToken);
        }

        private async Task LoadInitialGameDataAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GameDataRequest request = new GameDataRequest();
            GameDataResponse response = await CallAsync(request, cancellationToken);
            gameData = response?.GameData;
        }

        public async Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : ModuleResponse
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();
            Task<TResponse> endpointCall = cloudCodeService.CallEndpointAsync<TResponse>(request.ModuleName, request.FunctionName, payload: request, cancellationToken: cancellationToken);
            TResponse response = await endpointCall;
            moduleResponseDispatchService.DispatchNestedResponses(response);
            return response;
        }
    }
}
