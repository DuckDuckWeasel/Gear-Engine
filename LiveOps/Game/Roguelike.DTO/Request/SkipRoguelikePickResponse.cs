using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    public sealed class SkipRoguelikePickResponse : ModuleResponse
    {
        [JsonProperty]
        public bool Success { get; set; }
    }
}
