using System.Collections.Generic;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Loadout;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(SaveBoardLayoutRequest))]
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
