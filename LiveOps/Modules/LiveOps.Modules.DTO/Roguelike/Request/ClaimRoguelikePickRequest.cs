using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("ClaimRoguelikePickRequest")]
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
