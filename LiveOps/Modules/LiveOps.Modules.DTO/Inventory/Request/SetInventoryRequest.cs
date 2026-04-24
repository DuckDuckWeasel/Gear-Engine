using System.Collections.Generic;
using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Inventory;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("SetInventoryRequest")]
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
