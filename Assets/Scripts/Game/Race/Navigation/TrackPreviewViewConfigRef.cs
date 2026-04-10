using Scaffold.Navigation;

namespace Game.Race.Navigation
{
    /// <summary>
    /// DI handle for the track-preview screen config (multiple <see cref="ViewConfig"/> instances in one scope).
    /// </summary>
    public sealed class TrackPreviewViewConfigRef
    {
        public TrackPreviewViewConfigRef(ViewConfig config)
        {
            Config = config;
        }

        public ViewConfig Config { get; }
    }
}
