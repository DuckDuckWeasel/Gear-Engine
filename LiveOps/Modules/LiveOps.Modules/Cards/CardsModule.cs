using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.Modules.DTO.Cards;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Cards
{
    public class CardsModule : GameModule<CardGameData>
    {
        public const string PersistenceKey = nameof(CardPersistence);
        public const string ConfigKey = nameof(CardConfig);

        public CardsModule()
        {
        }

        public override async Task<IGameModuleData> InitializeAsync(GameApiSession session, CancellationToken cancellationToken = default)
        {
            IExecutionContext context = session.Context;
            IPlayerData player = session.Player;
            IRemoteConfig remoteConfig = session.RemoteConfig;
            CardConfig config = await remoteConfig.Get(context, ConfigKey, new CardConfig());
            CardPersistence persistence = await player.Get(context, PersistenceKey, new CardPersistence());
            return new CardGameData(persistence, config);
        }
    }
}
