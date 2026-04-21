using System.Collections.Generic;
using GameModuleDTO.Modules.Inventory;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    public sealed class SetInventoryResponse : ModuleResponse
    {
        [JsonProperty]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();
    }
}
