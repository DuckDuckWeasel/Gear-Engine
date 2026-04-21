using System.Collections.Generic;
using GameModuleDTO.GameApi;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class SetInventoryRequest : ModuleRequest<SetInventoryResponse>
    {
        public SetInventoryRequest()
        {
        }

        public SetInventoryRequest(IEnumerable<string> gearIds)
        {
            GearIds = gearIds != null ? new List<string>(gearIds) : new List<string>();
        }

        [JsonProperty]
        public List<string> GearIds { get; set; } = new List<string>();
    }
}
