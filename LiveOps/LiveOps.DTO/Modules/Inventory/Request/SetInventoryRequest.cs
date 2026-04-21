using System.Collections.Generic;
using GameModuleDTO.GameApi;
using GameModuleDTO.Modules.Inventory;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class SetInventoryRequest : ModuleRequest<SetInventoryResponse>
    {
        public SetInventoryRequest()
        {
        }

        public SetInventoryRequest(IEnumerable<OwnedGearEntry> gears)
        {
            Gears = gears != null ? new List<OwnedGearEntry>(gears) : new List<OwnedGearEntry>();
        }

        [JsonProperty]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();
    }
}
