using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.AppFlow;

namespace GearEngine.App.Bootstrap
{
    /// <summary>todo: Runs <see cref="IAsyncInitializable"/> instances in registration order so UGS finishes before LiveOps game data and client modules.</summary>
    public sealed class SequentialInLayerScheduler : IInLayerScheduler
    {
        public async Task RunAsync(IReadOnlyList<IAsyncInitializable> fresh, CancellationToken ct)
        {
            if (fresh == null || fresh.Count == 0)
            {
                return;
            }

            for (int i = 0; i < fresh.Count; i++)
            {
                await fresh[i].InitializeAsync(ct);
            }
        }
    }
}
