using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;

namespace LiveOps.Modules.DTO.GameData
{
    /// <summary>
    /// Represents a network request for aggregated game module data.
    /// The server builds <see cref="GameDataResponse"/> from every game module registered in cloud DI (see <c>ModuleConfig</c>).
    /// </summary>
    [UsesGameApi]
    [GameApiKey("GameDataRequest")]
    public class GameDataRequest : ModuleRequest<GameDataResponse>
    {
        public GameDataRequest()
        {
        }
    }
}
