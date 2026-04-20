using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using AccessKeyValidator = GameModule.AccessKey.AccessKey;

namespace GameModule.Modules.DirectPush
{
    public sealed class SendPlayerPushHandler : IGameApiHandler<SendPlayerPushRequest, SendPushResponse>
    {
        private readonly ILogger<SendPlayerPushHandler> _logger;
        private readonly PushClient _pushClient;

        public SendPlayerPushHandler(ILogger<SendPlayerPushHandler> logger, PushClient pushClient)
        {
            _logger = logger;
            _pushClient = pushClient;
        }

        public async Task<SendPushResponse> HandleAsync(GameApiSession session, SendPlayerPushRequest request)
        {
            bool valid = await AccessKeyValidator.ValidServer(session.GameState, session.Context, request.Guid).ConfigureAwait(false);
            if (!valid)
            {
                SendPushResponse errorResponse = new SendPushResponse();
                errorResponse.SetResponseFailure("Invalid access");
                return errorResponse;
            }

            _logger.LogInformation(
                "[DirectPush] Sending player push type '{MessageType}' to playerId '{PlayerId}'",
                request.MessageType, request.PlayerId);

            await _pushClient.SendPlayerMessageAsync(
                session.Context, request.Message, request.MessageType, request.PlayerId).ConfigureAwait(false);

            SendPushResponse response = new SendPushResponse();
            response.SetResponse(ResponseStatusType.Success, "Player message sent");
            return response;
        }
    }
}
