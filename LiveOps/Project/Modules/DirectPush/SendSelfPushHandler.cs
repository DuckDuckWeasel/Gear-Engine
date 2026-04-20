using System.Threading.Tasks;
using GameModule.GameApi;
using GameModuleDTO.ModuleRequests;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.DirectPush
{
    public sealed class SendSelfPushHandler : IGameApiHandler<SendSelfPushRequest, SendPushResponse>
    {
        private readonly ILogger<SendSelfPushHandler> _logger;
        private readonly PushClient _pushClient;

        public SendSelfPushHandler(ILogger<SendSelfPushHandler> logger, PushClient pushClient)
        {
            _logger = logger;
            _pushClient = pushClient;
        }

        public async Task<SendPushResponse> HandleAsync(GameApiSession session, SendSelfPushRequest request)
        {
            _logger.LogInformation(
                "[DirectPush] Sending self push type '{MessageType}' to playerId '{PlayerId}'",
                request.MessageType, session.Context.PlayerId);

            await _pushClient.SendPlayerMessageAsync(
                session.Context, request.Message, request.MessageType, session.Context.PlayerId).ConfigureAwait(false);

            SendPushResponse response = new SendPushResponse();
            response.SetResponse(ResponseStatusType.Success, "Player message sent");
            return response;
        }
    }
}
