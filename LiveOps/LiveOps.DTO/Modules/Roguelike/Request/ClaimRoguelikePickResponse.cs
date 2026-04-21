using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    public sealed class ClaimRoguelikePickResponse : ModuleResponse
    {
        [JsonProperty]
        public bool Success { get; set; }
    }
}
