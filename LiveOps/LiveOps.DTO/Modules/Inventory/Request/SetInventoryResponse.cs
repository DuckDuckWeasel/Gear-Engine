using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    public sealed class SetInventoryResponse : ModuleResponse
    {
        [JsonProperty]
        public List<string> GearIds { get; set; } = new List<string>();
    }
}
