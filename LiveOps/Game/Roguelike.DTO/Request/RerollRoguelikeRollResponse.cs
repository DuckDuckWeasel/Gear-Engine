using LiveOps.DTO.ModuleRequest;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    public sealed class RerollRoguelikeRollResponse : ModuleResponse
    {
        [JsonProperty]
        public List<string> CurrentRollIds { get; set; } = new List<string>();
    }
}
