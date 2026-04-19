using System.Threading;
using System.Threading.Tasks;

namespace GearEngine.LayeredScope
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync(CancellationToken ct);
    }
}
