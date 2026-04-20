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
    public sealed class SendProjectPushHandler : IGameApiHandler<SendProjectPushRequest, SendPushResponse>
    {
        private readonly ILogger<SendProjectPushHandler> _logger;
        private readonly PushClient _pushClient;

        public SendProjectPushHandler(ILogger<SendProjectPushHandler> logger, PushClient pushClient)
        {
            _logger = logger;
            _pushClient = pushClient;
        }

        public async Task<SendPushResponse> HandleAsync(GameApiSession session, SendProjectPushRequest request)
        {
            bool valid = await AccessKeyValidator.ValidServer(session.GameState, session.Context, request.Guid).ConfigureAwait(false);
            if (!valid)
            {
                SendPushResponse errorResponse = new SendPushResponse();
                errorResponse.SetResponseFailure("Invalid access");
                return errorResponse;
            }

            _logger.LogInformation(
                "[DirectPush] Broadcasting project push type '{MessageType}'",
                request.MessageType);

            await _pushClient.SendProjectMessageAsync(
                session.Context, request.Message, request.MessageType).ConfigureAwait(false);

            SendPushResponse response = new SendPushResponse();
            response.SetResponse(ResponseStatusType.Success, "Project message sent");
            return response;
        }
    }
}
