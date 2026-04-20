using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameApi;
using GameModuleDTO.GameModule;
using GameModuleDTO.ModuleRequests;
using GearEngine.LayeredScope;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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

        private static readonly JsonSerializerSettings LiveOpsJsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

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
            if (RequestUsesGameApi(request))
            {
                return await CallGameApiAsync(request, cancellationToken).ConfigureAwait(false);
            }

            Task<TResponse> endpointCall = cloudCodeService.CallEndpointAsync<TResponse>(request.ModuleName, request.FunctionName, payload: request, cancellationToken: cancellationToken);
            TResponse response = await endpointCall.ConfigureAwait(false);
            moduleResponseDispatchService.DispatchNestedResponses(response);
            return response;
        }

        private static bool RequestUsesGameApi<TResponse>(ModuleRequest<TResponse> request) where TResponse : ModuleResponse
        {
            return request.GetType().GetCustomAttribute<UsesGameApiAttribute>(inherit: false) != null;
        }

        private async Task<TResponse> CallGameApiAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken)
            where TResponse : ModuleResponse
        {
            GameApiEnvelopeRequest envelope = new GameApiEnvelopeRequest
            {
                RequestKey = request.GetType().Name,
                Payload = JObject.FromObject(request, JsonSerializer.Create(LiveOpsJsonSettings)),
            };
            GameApiEnvelopeResponse resp = await cloudCodeService.CallEndpointAsync<GameApiEnvelopeResponse>(
                request.ModuleName, "GameApi", envelope, cancellationToken).ConfigureAwait(false);
            if (resp == null)
            {
                throw new InvalidOperationException("GameApi returned null response.");
            }

            if (resp.StatusType == ResponseStatusType.Exception)
            {
                throw new InvalidOperationException(string.IsNullOrEmpty(resp.Message) ? "GameApi failed." : resp.Message);
            }

            TResponse typed = (TResponse)resp.Result;
            if (typed == null)
            {
                throw new InvalidOperationException("GameApi returned null result payload.");
            }

            if (resp.NestedResponses != null && resp.NestedResponses.Count > 0)
            {
                typed.Responses.AddRange(resp.NestedResponses);
            }

            moduleResponseDispatchService.DispatchNestedResponses(typed);
            return typed;
        }
    }
}
