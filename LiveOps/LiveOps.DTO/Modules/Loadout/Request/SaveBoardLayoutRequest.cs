using System.Collections.Generic;
using GameModuleDTO.GameApi;
using GameModuleDTO.Modules.Loadout;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
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
