using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Tracks;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Tracks
{
    public class TracksModule : GameModule<TrackGameData>
    {
        public const string PersistenceKey = nameof(TrackPersistence);
        public const string ConfigKey = nameof(TrackConfig);

        public TracksModule()
        {
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            TrackConfig config = await remoteConfig.Get(context, ConfigKey, new TrackConfig()).ConfigureAwait(false);
            TrackPersistence persistence = await player.Get(context, PersistenceKey, new TrackPersistence()).ConfigureAwait(false);

            if (string.IsNullOrEmpty(persistence.CurrentTrackId) && config.Entries.Count > 0)
            {
                persistence.CurrentTrackId = config.Entries[0].Id;
                await player.Set(context, PersistenceKey, persistence).ConfigureAwait(false);
            }

            return new TrackGameData(persistence, config);
        }
    }
}
