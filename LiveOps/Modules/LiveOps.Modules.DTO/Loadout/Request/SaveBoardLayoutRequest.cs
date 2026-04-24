using System.Collections.Generic;
using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Loadout;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("SaveBoardLayoutRequest")]
    public sealed class SaveBoardLayoutRequest : ModuleRequest<SaveBoardLayoutResponse>
    {
        public SaveBoardLayoutRequest()
        {
        }

        public SaveBoardLayoutRequest(IEnumerable<LoadoutPlacement> placements)
        {
            Placements = placements != null ? new List<LoadoutPlacement>(placements) : new List<LoadoutPlacement>();
        }

        [JsonProperty]
        public List<LoadoutPlacement> Placements { get; set; } = new List<LoadoutPlacement>();
    }
}
