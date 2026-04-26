using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.Keys;
using LiveOps.Modules.DTO.Tracks;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Tracks
{
    public class TracksModule : GameModule<TrackGameData>
    {
        public static readonly string PersistenceKey = KeyOf<TrackPersistence>.Module;
        public static readonly string ConfigKey = KeyOf<TrackConfig>.Module;

        private readonly ILogger<TracksModule> _logger;

        public TracksModule(ILogger<TracksModule> logger)
        {
            _logger = logger;
        }

        public override async Task<IGameModuleData> InitializeAsync(GameApiSession session, CancellationToken cancellationToken = default)
        {
            IExecutionContext context = session.Context;
            IPlayerData player = session.Player;
            IRemoteConfig remoteConfig = session.RemoteConfig;

            _logger.LogInformation(
                "[TracksModule] Loading remote config key {ConfigKey} and player persistence key {PersistenceKey}.",
                ConfigKey,
                PersistenceKey);

            TrackConfig config = await remoteConfig.Get(context, ConfigKey, new TrackConfig());
            TrackPersistence persistence = await player.Get(context, PersistenceKey, new TrackPersistence());

            if (string.IsNullOrEmpty(persistence.CurrentTrackId) && config.Entries.Count > 0)
            {
                persistence.CurrentTrackId = config.Entries[0].Id;
                await player.Set(context, PersistenceKey, persistence);
            }

            TrackGameData gameData = new TrackGameData(persistence, config);
            _logger.LogInformation(
                "[TracksModule] Loaded config entries={ConfigEntryCount}, currentTrackId='{CurrentTrackId}', produced orderedTrackIds={OrderedCount}.",
                config.Entries.Count,
                persistence.CurrentTrackId,
                gameData.OrderedTrackIds.Count);

            return gameData;
        }
    }
}
