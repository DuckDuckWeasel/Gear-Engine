using System.Threading.Tasks;
using GameModuleDTO.ModuleRequests;

namespace GameModule.GameApi
{
    /// <summary>
    /// Handles a single <see cref="ModuleRequest{TResponse}"/> type in the GameApi pipeline.
    /// </summary>
    public interface IGameApiHandler<TRequest, TResponse>
        where TRequest : ModuleRequest<TResponse>
        where TResponse : ModuleResponse
    {
        Task<TResponse> HandleAsync(GameApiSession session, TRequest request);
    }
}
