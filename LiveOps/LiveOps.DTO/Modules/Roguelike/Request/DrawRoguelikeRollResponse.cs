using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    public sealed class DrawRoguelikeRollResponse : ModuleResponse
    {
        [JsonProperty]
        public List<string> CurrentRollIds { get; set; } = new List<string>();
    }
}
