using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.Keys;
using LiveOps.Modules.DTO.Loadout;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Loadout
{
    public class LoadoutModule : GameModule<LoadoutGameData>
    {
        public static readonly string PersistenceKey = KeyOf<LoadoutPersistence>.Module;
        public static readonly string ConfigKey = KeyOf<LoadoutConfig>.Module;

        public LoadoutModule()
        {
        }

        public override async Task<IGameModuleData> InitializeAsync(GameApiSession session, CancellationToken cancellationToken = default)
        {
            IExecutionContext context = session.Context;
            IPlayerData player = session.Player;
            IRemoteConfig remoteConfig = session.RemoteConfig;
            LoadoutConfig config = await remoteConfig.Get(context, ConfigKey, new LoadoutConfig());
            LoadoutPersistence persistence = await player.Get(context, PersistenceKey, new LoadoutPersistence());
            return new LoadoutGameData(persistence, config);
        }
    }
}
