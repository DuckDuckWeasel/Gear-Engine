using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.ModuleRequests;
using PurchasePerkResponse = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkResponse;
using BurnPerkResponse = LiveOps.Modules.DTO.ModuleRequests.BurnPerkResponse;

namespace GearEngine.Campaign.Bootstrap.Perks
{
    /// <summary>
    /// Contract for the perks client module consumed by view-models and tests.
    /// </summary>
    public interface IPerksClientModule
    {
        /// <summary>All perk IDs owned by the player (duplicates = multiple copies).</summary>
        IReadOnlyList<string> Unlocked { get; }

        long NextCost { get; }

        long BurnReward { get; }

        string CurrencyId { get; }

        Task InitializeAsync(CancellationToken cancellationToken = default);

        Task<PurchasePerkResponse> PurchaseAsync(CancellationToken cancellationToken = default);

        Task<BurnPerkResponse> BurnAsync(string perkId, CancellationToken cancellationToken = default);
    }
}
