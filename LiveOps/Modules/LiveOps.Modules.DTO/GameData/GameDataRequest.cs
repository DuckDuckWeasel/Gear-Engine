using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.GameData
{
    /// <summary>
    /// Represents a network request for aggregated game module data.
    /// The server builds <see cref="GameDataResponse"/> from every game module registered in cloud DI (see <c>ModuleConfig</c>).
    /// </summary>
    [LiveOpsKey(nameof(GameDataRequest))]
    public class GameDataRequest : ModuleRequest<GameDataResponse>
    {
        public GameDataRequest()
        {
        }
    }
}
