using GameModuleDTO.GameApi;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    /// <summary>
    /// Request to complete a specific level.
    /// </summary>
    [UsesGameApi]
    public class CompleteLevelRequest : ModuleRequest<CompleteLevelResponse>
    {
        public CompleteLevelRequest()
        {
        }

        public CompleteLevelRequest(int levelId)
        {
            LevelId = levelId;
        }

        [JsonProperty]
        public int LevelId { get; set; }
    }
}
