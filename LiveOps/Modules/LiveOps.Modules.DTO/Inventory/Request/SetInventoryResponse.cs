using LiveOps.DTO.ModuleRequest;
using System.Collections.Generic;
using LiveOps.Modules.DTO.Inventory;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    public sealed class SetInventoryResponse : ModuleResponse
    {
        [JsonProperty]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();
    }
}
