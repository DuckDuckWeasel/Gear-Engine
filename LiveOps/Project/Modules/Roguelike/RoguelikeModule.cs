using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Roguelike;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Roguelike
{
    public class RoguelikeModule : GameModule<RoguelikeGameData>
    {
        public const string PersistenceKey = nameof(RoguelikePersistence);
        public const string ConfigKey = nameof(RoguelikeConfig);

        public RoguelikeModule()
        {
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            RoguelikeConfig config = await remoteConfig.Get(context, ConfigKey, new RoguelikeConfig()).ConfigureAwait(false);
            RoguelikePersistence persistence = await player.Get(context, PersistenceKey, new RoguelikePersistence()).ConfigureAwait(false);
            return new RoguelikeGameData(persistence, config);
        }
    }
}
