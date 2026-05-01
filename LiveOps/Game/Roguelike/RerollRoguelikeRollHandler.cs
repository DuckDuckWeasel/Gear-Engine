using System.Collections.Generic;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Roguelike;

namespace LiveOps.Modules.Roguelike
{
    public sealed class RerollRoguelikeRollHandler : IGameApiHandler<RerollRoguelikeRollRequest, RerollRoguelikeRollResponse>
    {
        private readonly IRoguelikeSelectionStrategy strategy;

        public RerollRoguelikeRollHandler()
            : this(new RandomRoguelikeSelectionStrategy())
        {
        }

        public RerollRoguelikeRollHandler(IRoguelikeSelectionStrategy strategy)
        {
            this.strategy = strategy;
        }

        public async Task<RerollRoguelikeRollResponse> HandleAsync(GameApiSession session, RerollRoguelikeRollRequest request)
        {
            RoguelikePersistence persistence = await session.Player.Get(session.Context, RoguelikeModule.PersistenceKey, new RoguelikePersistence());
            RoguelikeConfig config = await session.RemoteConfig.Get(session.Context, RoguelikeModule.ConfigKey, new RoguelikeConfig());
            
            // Draw a new roll
            IReadOnlyList<string> drawn = strategy.Select(config.GearPool, config.OptionsPerRoll);
            persistence.CurrentRollIds = new List<string>(drawn);
            
            return new RerollRoguelikeRollResponse { CurrentRollIds = new List<string>(persistence.CurrentRollIds) };
        }
    }
}
