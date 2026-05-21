using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(BurnPerkRequest))]
    public sealed class BurnPerkRequest : ModuleRequest<BurnPerkResponse>
    {
        /// <summary>ID of the Perk to burn. The player must own at least one copy.</summary>
        [JsonProperty("PerkId")]
        public string PerkId { get; set; } = string.Empty;
    }
}
