using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;

namespace LiveOps.Modules.Perks
{
    public sealed class BurnPerkHandler : IGameApiHandler<BurnPerkRequest, BurnPerkResponse>
    {
        public async Task<BurnPerkResponse> HandleAsync(GameApiSession session, BurnPerkRequest request)
        {
            if (string.IsNullOrEmpty(request?.PerkId))
            {
                return new BurnPerkResponse { Success = false };
            }

            PerkConfig config = await session.RemoteConfig.Get(session.Context, PerksModule.ConfigKey, new PerkConfig());
            PerkPersistence persistence = await session.Player.Get(session.Context, PerksModule.PersistenceKey, new PerkPersistence());

            int index = persistence.Unlocked.IndexOf(request.PerkId);
            if (index < 0)
            {
                // Player does not own this Perk.
                return new BurnPerkResponse { Success = false };
            }

            persistence.Unlocked.RemoveAt(index);

            long reward = config.BurnReward;
            AddCurrencyResponse addResp = await session.InvokeAsync<AddCurrencyRequest, AddCurrencyResponse>(
                new AddCurrencyRequest("gold", reward));

            await session.Player.Set(session.Context, PerksModule.PersistenceKey, persistence);

            return new BurnPerkResponse
            {
                Success = true,
                GoldEarned = reward,
                NewGoldBalance = addResp?.NewAmount ?? 0,
            };
        }
    }
}
