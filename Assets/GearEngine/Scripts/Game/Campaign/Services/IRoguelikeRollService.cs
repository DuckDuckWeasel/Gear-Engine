using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.GearEngine.Services.Inventory;

namespace GearEngine.Campaign.Services
{
    public interface IRoguelikeRollService
    {
        Task<IReadOnlyList<IItem>> GetCurrentRollAsync(CancellationToken cancellationToken = default);
        Task ConsumePickAsync(string pickedId, CancellationToken cancellationToken = default);
        Task SkipPickAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<IItem>> RerollAsync(CancellationToken cancellationToken = default);
    }
}
