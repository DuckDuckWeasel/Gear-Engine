using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.Modules.DTO.Tracks;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Tracks
{
    public class TracksModule : GameModule<TrackGameData>
    {
        public const string PersistenceKey = nameof(TrackPersistence);
        public const string ConfigKey = nameof(TrackConfig);

        public TracksModule()
        {
        }

        public override async Task<IGameModuleData> InitializeAsync(GameApiSession session, CancellationToken cancellationToken = default)
        {
            IExecutionContext context = session.Context;
            IPlayerData player = session.Player;
            IRemoteConfig remoteConfig = session.RemoteConfig;
            TrackConfig config = await remoteConfig.Get(context, ConfigKey, new TrackConfig());
            TrackPersistence persistence = await player.Get(context, PersistenceKey, new TrackPersistence());

            if (string.IsNullOrEmpty(persistence.CurrentTrackId) && config.Entries.Count > 0)
            {
                persistence.CurrentTrackId = config.Entries[0].Id;
                await player.Set(context, PersistenceKey, persistence);
            }

            return new TrackGameData(persistence, config);
        }
    }
}
