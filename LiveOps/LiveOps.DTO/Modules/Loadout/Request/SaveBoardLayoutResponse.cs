using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    public sealed class SaveBoardLayoutResponse : ModuleResponse
    {
        [JsonProperty]
        public long SavedAtUtcTicks { get; set; }
    }
}
