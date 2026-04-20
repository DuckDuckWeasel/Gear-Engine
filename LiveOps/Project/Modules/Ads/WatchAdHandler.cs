using System.Threading.Tasks;
using GameModule.GameApi;
using GameModuleDTO.GameModule;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Ads;
using GameModuleDTO.Modules.Gold;
using Microsoft.Extensions.Logging;

namespace GameModule.Modules.Ads
{
    /// <summary>
    /// GameApi handler for <see cref="WatchAdRequest"/>.
    /// </summary>
    public sealed class WatchAdHandler : IGameApiHandler<WatchAdRequest, WatchAdResponse>
    {
        private readonly ILogger<WatchAdHandler> _logger;

        public WatchAdHandler(ILogger<WatchAdHandler> logger)
        {
            _logger = logger;
        }

        public async Task<WatchAdResponse> HandleAsync(GameApiSession session, WatchAdRequest request)
        {
            _logger.LogInformation("[WatchAdHandler] Starting for placement: {PlacementId}", request?.PlacementId);
            AdsConfig config = await session.RemoteConfig.Get(session.Context, new AdsConfig()).ConfigureAwait(false);
            AdsPersistence persistence = await session.Player.GetOrSet(session.Context, new AdsPersistence()).ConfigureAwait(false);

            string placementId = request?.PlacementId ?? "default";
            AdPlacementConfig placementConfig = config.GetPlacement(placementId);

            if (persistence.HasReachedMaxViews(placementId, placementConfig.MaxViews))
            {
                _logger.LogWarning("[WatchAdHandler] Cannot watch ad. Max views reached for placement: {PlacementId}", placementId);
            }
            else if (persistence.IsCooldownElapsed(placementId, placementConfig.CooldownSeconds))
            {
                persistence.RecordAdWatched(placementId);
                await GrantReward(session, placementConfig, placementId).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("[WatchAdHandler] Cannot watch ad yet. On cooldown for placement: {PlacementId}", placementId);
            }

            AdData adData = new AdData(persistence, config);
            return new WatchAdResponse(adData);
        }

        private async Task GrantReward(GameApiSession session, AdPlacementConfig placementConfig, string placementId)
        {
            if (placementConfig.RewardAmount <= 0 || string.IsNullOrEmpty(placementConfig.RewardType))
            {
                _logger.LogInformation("[WatchAdHandler] No reward configured for placement: {PlacementId}", placementId);
                return;
            }

            if (placementConfig.RewardType == typeof(GoldGameData).Name)
            {
                await session.InvokeAsync<AddGoldRequest, GoldChangedResponse>(new AddGoldRequest(placementConfig.RewardAmount)).ConfigureAwait(false);
                _logger.LogInformation("[WatchAdHandler] Granted {Amount} gold for placement: {PlacementId}", placementConfig.RewardAmount, placementId);
            }
            else
            {
                _logger.LogWarning("[WatchAdHandler] Unknown RewardType '{RewardType}' for placement: {PlacementId}", placementConfig.RewardType, placementId);
            }
        }
    }
}
