using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.Keys;
using LiveOps.Modules.DTO.Roguelike;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Roguelike
{
    public class RoguelikeModule : GameModule<RoguelikeGameData>
    {
        public static readonly string PersistenceKey = KeyOf<RoguelikePersistence>.Module;
        public static readonly string ConfigKey = KeyOf<RoguelikeConfig>.Module;

        public RoguelikeModule()
        {
        }

        public override async Task<IGameModuleData> InitializeAsync(GameApiSession session, CancellationToken cancellationToken = default)
        {
            IExecutionContext context = session.Context;
            IPlayerData player = session.Player;
            IRemoteConfig remoteConfig = session.RemoteConfig;
            RoguelikeConfig config = await remoteConfig.Get(context, ConfigKey, new RoguelikeConfig());
            RoguelikePersistence persistence = await player.Get(context, PersistenceKey, new RoguelikePersistence());
            return new RoguelikeGameData(persistence, config);
        }
    }
}
