using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(ClaimRoguelikePickRequest))]
    public sealed class ClaimRoguelikePickRequest : ModuleRequest<ClaimRoguelikePickResponse>
    {
        public ClaimRoguelikePickRequest()
        {
        }

        public ClaimRoguelikePickRequest(string pickedGearId)
        {
            PickedGearId = pickedGearId;
        }

        [JsonProperty]
        public string PickedGearId { get; set; } = string.Empty;
    }
}
