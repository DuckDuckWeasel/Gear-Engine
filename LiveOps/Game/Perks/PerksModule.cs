using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.Keys;
using LiveOps.Modules.DTO.Perks;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Perks
{
    public class PerksModule : GameModule<PerkGameData>
    {
        public static readonly string PersistenceKey = KeyOf<PerkPersistence>.Module;
        public static readonly string ConfigKey = KeyOf<PerkConfig>.Module;

        public PerksModule()
        {
        }

        public override async Task<IGameModuleData> InitializeAsync(GameApiSession session, CancellationToken cancellationToken = default)
        {
            IExecutionContext context = session.Context;
            IPlayerData player = session.Player;
            IRemoteConfig remoteConfig = session.RemoteConfig;
            PerkConfig config = await remoteConfig.Get(context, ConfigKey, new PerkConfig());
            PerkPersistence persistence = await player.Get(context, PersistenceKey, new PerkPersistence());
            return new PerkGameData(persistence, config);
        }
    }
}
