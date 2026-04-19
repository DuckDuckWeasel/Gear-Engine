using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GearEngine.LayeredScope
{
    public interface IInLayerScheduler
    {
        Task RunAsync(IReadOnlyList<IAsyncInitializable> fresh, CancellationToken ct);
    }
}
