using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Cards;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Cards
{
    public class CardsModule : GameModule<CardGameData>
    {
        public const string PersistenceKey = nameof(CardPersistence);
        public const string ConfigKey = nameof(CardConfig);

        public CardsModule()
        {
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            CardConfig config = await remoteConfig.Get(context, ConfigKey, new CardConfig()).ConfigureAwait(false);
            CardPersistence persistence = await player.Get(context, PersistenceKey, new CardPersistence()).ConfigureAwait(false);
            return new CardGameData(persistence, config);
        }
    }
}
