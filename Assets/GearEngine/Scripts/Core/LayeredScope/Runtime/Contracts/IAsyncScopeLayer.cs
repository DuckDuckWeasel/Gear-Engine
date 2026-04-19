using System.Threading;
using System.Threading.Tasks;
using VContainer;

namespace GearEngine.LayeredScope
{
    public interface IAsyncScopeLayer : IScopeLayer
    {
        Task PrepareAsync(IObjectResolver parent, CancellationToken ct);
    }
}
