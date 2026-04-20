using System;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModuleDTO.Modules.Gold;
using GameModuleDTO.ModuleRequests;
using Microsoft.Extensions.Logging;

namespace GameModule.Modules.Gold
{
    /// <summary>
    /// GameApi handler for <see cref="AddGoldRequest"/>.
    /// </summary>
    public sealed class AddGoldHandler : IGameApiHandler<AddGoldRequest, GoldChangedResponse>
    {
        private readonly ILogger<AddGoldHandler> _logger;

        public AddGoldHandler(ILogger<AddGoldHandler> logger)
        {
            _logger = logger;
        }

        public async Task<GoldChangedResponse> HandleAsync(GameApiSession session, AddGoldRequest request)
        {
            long amount = request == null ? 0 : request.Amount;
            _logger.LogInformation("[AddGoldHandler] Rewarding player {PlayerId} with {Amount}", session.Context.PlayerId, amount);

            if (amount == 0)
            {
                GoldPersistence currentPersistence = await session.Player.GetOrSet(session.Context, new GoldPersistence()).ConfigureAwait(false);
                return new GoldChangedResponse(currentPersistence.Current, 0);
            }

            GoldConfig config = await session.RemoteConfig.Get(session.Context, new GoldConfig()).ConfigureAwait(false);
            GoldPersistence goldPersistence = await session.Player.GetOrSet(session.Context, new GoldPersistence()).ConfigureAwait(false);
            long next = goldPersistence.Current + amount;
            long previous = goldPersistence.Current;
            _logger.LogInformation("[AddGoldHandler] GoldConfig is from {Min} to {Max}", config.Min, config.Max);
            goldPersistence.SetCurrent(Math.Clamp(next, config.Min, config.Max));
            long actualDelta = goldPersistence.Current - previous;
            _logger.LogInformation("[AddGoldHandler] GoldPersistence is {Current} on delta {Delta}", goldPersistence.Current, actualDelta);
            _logger.LogInformation("[AddGoldHandler] Added {Amount} gold to player {PlayerId}. New total: {Total}", amount, session.Context.PlayerId, goldPersistence.Current);
            return new GoldChangedResponse(goldPersistence.Current, actualDelta);
        }
    }
}
