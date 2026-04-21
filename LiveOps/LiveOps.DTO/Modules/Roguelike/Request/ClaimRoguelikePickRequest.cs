using GameModuleDTO.GameApi;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
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
