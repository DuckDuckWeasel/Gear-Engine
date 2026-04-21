using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Roguelike;

namespace GameModule.Modules.Roguelike
{
    public sealed class DrawRoguelikeRollHandler : IGameApiHandler<DrawRoguelikeRollRequest, DrawRoguelikeRollResponse>
    {
        private readonly IRoguelikeSelectionStrategy strategy;

        public DrawRoguelikeRollHandler()
            : this(new RandomRoguelikeSelectionStrategy())
        {
        }

        public DrawRoguelikeRollHandler(IRoguelikeSelectionStrategy strategy)
        {
            this.strategy = strategy;
        }

        public async Task<DrawRoguelikeRollResponse> HandleAsync(GameApiSession session, DrawRoguelikeRollRequest request)
        {
            RoguelikePersistence persistence = await session.Player.Get(session.Context, RoguelikeModule.PersistenceKey, new RoguelikePersistence()).ConfigureAwait(false);

            if (persistence.CurrentRollIds.Count > 0)
            {
                return new DrawRoguelikeRollResponse { CurrentRollIds = new List<string>(persistence.CurrentRollIds) };
            }

            RoguelikeConfig config = await session.RemoteConfig.Get(session.Context, RoguelikeModule.ConfigKey, new RoguelikeConfig()).ConfigureAwait(false);
            IReadOnlyList<string> drawn = strategy.Select(config.GearPool, config.OptionsPerRoll);
            persistence.CurrentRollIds = new List<string>(drawn);
            return new DrawRoguelikeRollResponse { CurrentRollIds = new List<string>(persistence.CurrentRollIds) };
        }
    }
}
