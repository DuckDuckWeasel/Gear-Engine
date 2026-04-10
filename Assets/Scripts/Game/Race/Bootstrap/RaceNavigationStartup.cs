using System.Threading;
using System.Threading.Tasks;
using Game.Race.Navigation;
using Scaffold.Navigation;
using VContainer.Unity;

namespace Game.Race
{
    /// <summary>
    /// Opens the track-preview screen once the scene container is built.
    /// </summary>
    public sealed class RaceNavigationStartup : IAsyncStartable
    {
        private readonly INavigator navigator;
        private readonly TrackPreviewViewConfigRef trackPreviewConfig;

        public RaceNavigationStartup(INavigator navigator, TrackPreviewViewConfigRef trackPreviewConfig)
        {
            this.navigator = navigator;
            this.trackPreviewConfig = trackPreviewConfig;
        }

        public async Task StartAsync(CancellationToken cancellation)
        {
            await navigator.OpenAsync(trackPreviewConfig.Config);
        }
    }
}
