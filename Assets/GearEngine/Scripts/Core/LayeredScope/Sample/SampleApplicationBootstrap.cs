using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.LayeredScope;
using GearEngine.LayeredScope.Sample.Layers;
using UnityEngine;

namespace GearEngine.LayeredScope.Sample
{
    public sealed class SampleApplicationBootstrap : ApplicationBootstrap
    {
        protected override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            yield return new SampleAssetsLayer();
            yield return new SampleConfigsLayer();
        }

        protected override async Task OnReadyAsync(CancellationToken ct)
        {
            await Host.PushAsync(new SampleFeatureLayer(), ct);
            await Task.Delay(1000, ct);
            await Host.PopAsync(ct);
            Debug.Log("[Sample] feature popped; assets/configs still alive.");
        }
    }
}
