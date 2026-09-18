using System;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameApi;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.Keys;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.GameData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Scaffold.AppFlow;
using Scaffold.CloudCode;
using Scaffold.LiveOps;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Layers
{
    internal sealed class ResilientLiveOpsService : ILiveOpsService, IAsyncInitializable
    {
        public ResilientLiveOpsService(
            ICloudCodeService cloudCodeService,
            OfflineLiveOpsState offlineState,
            OfflineSessionState offlineSessionState)
        {
            this.cloudCodeService = cloudCodeService ?? throw new ArgumentNullException(nameof(cloudCodeService));
            this.offlineState = offlineState ?? throw new ArgumentNullException(nameof(offlineState));
            this.offlineSessionState = offlineSessionState ?? throw new ArgumentNullException(nameof(offlineSessionState));
        }

        private static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly ICloudCodeService cloudCodeService;
        private readonly OfflineLiveOpsState offlineState;
        private readonly OfflineSessionState offlineSessionState;
        private GameData gameData;

        public T GetModuleData<T>() where T : class, IGameModuleData
        {
            return gameData?.GetModuleData<T>();
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return LoadInitialGameDataAsync(cancellationToken);
        }

        public async Task<TResponse> CallAsync<TResponse>(
            ModuleRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            where TResponse : ModuleResponse
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (request.GetType() == typeof(GameDataRequest))
                {
                    return await LoadGameDataResponseAsync(request, cancellationToken);
                }

                TResponse fallback = offlineState.CreateResponse(request);
                if (CanAttemptNetwork())
                {
                    _ = ReconcileInBackgroundAsync(request, cancellationToken);
                }

                return fallback;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[ResilientLiveOpsService] Local request fallback failed for {request.GetType().Name}. {exception}");
                return offlineState.CreateEmptyResponse<TResponse>();
            }
        }

        private async Task LoadInitialGameDataAsync(CancellationToken cancellationToken)
        {
            GameDataResponse response = await CallAsync(new GameDataRequest(), cancellationToken);
            gameData = response?.GameData ?? offlineState.CreateInitialGameData();
            offlineState.UseGameData(gameData);
        }

        private async Task<TResponse> LoadGameDataResponseAsync<TResponse>(
            ModuleRequest<TResponse> request,
            CancellationToken cancellationToken)
            where TResponse : ModuleResponse
        {
            if (!CanAttemptNetwork())
            {
                return offlineState.CreateResponse(request);
            }

            try
            {
                TResponse response = await InvokeServerAsync(request, cancellationToken);
                if (response is GameDataResponse gameDataResponse && gameDataResponse.GameData != null)
                {
                    offlineSessionState.MarkOnline();
                    offlineState.UseGameData(gameDataResponse.GameData);
                }

                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                offlineSessionState.MarkOffline();
                Debug.LogError($"[ResilientLiveOpsService] Initial online data request failed; using local game data. {exception}");
                return offlineState.CreateResponse(request);
            }
        }

        private async Task ReconcileInBackgroundAsync<TResponse>(
            ModuleRequest<TResponse> request,
            CancellationToken cancellationToken)
            where TResponse : ModuleResponse
        {
            try
            {
                await InvokeServerAsync(request, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // The caller already received a local response, so cancellation only stops reconciliation.
            }
            catch (Exception exception)
            {
                offlineSessionState.MarkOffline();
                Debug.LogError($"[ResilientLiveOpsService] Background sync failed for {request.GetType().Name}; local progress remains available. {exception}");
            }
        }

        private async Task<TResponse> InvokeServerAsync<TResponse>(
            ModuleRequest<TResponse> request,
            CancellationToken cancellationToken)
            where TResponse : ModuleResponse
        {
            GameApiEnvelopeRequest envelope = new GameApiEnvelopeRequest
            {
                RequestKey = KeyOf.WireOf(request),
                Payload = JObject.FromObject(request, JsonSerializer.Create(s_jsonSettings)),
            };
            GameApiEnvelopeResponse response = await cloudCodeService.CallEndpointAsync<GameApiEnvelopeResponse>(
                request.ModuleName,
                "GameApi",
                envelope,
                cancellationToken);
            if (response == null)
            {
                throw new InvalidOperationException("GameApi returned no response.");
            }

            if (response.StatusType == ResponseStatusType.Exception)
            {
                throw new InvalidOperationException(string.IsNullOrEmpty(response.Message)
                    ? "GameApi request failed."
                    : response.Message);
            }

            if (response.Result is not TResponse typed)
            {
                throw new InvalidOperationException($"GameApi returned an invalid result for {request.GetType().Name}.");
            }

            if (response.NestedResponses != null && response.NestedResponses.Count > 0)
            {
                typed.Responses.AddRange(response.NestedResponses);
            }

            return typed;
        }

        private bool CanAttemptNetwork()
        {
            return !offlineSessionState.IsOffline
                && Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}
