using GameModuleDTO.GameApi;

namespace GameModuleDTO.ModuleRequests
{
    /// <summary>
    /// Request initiating the ad watching process.
    /// </summary>
    [UsesGameApi]
    public class WatchAdRequest : ModuleRequest<WatchAdResponse>
    {
        public string PlacementId { get; set; }
    }
}
