using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Services
{
    public interface IRoguelikeRollService
    {
        Task<IReadOnlyList<GearConfig>> GetCurrentRollAsync(CancellationToken cancellationToken = default);

        Task ConsumePickAsync(GearConfig picked, CancellationToken cancellationToken = default);
        Task SkipPickAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<GearConfig>> RerollAsync(CancellationToken cancellationToken = default);
    }
}
