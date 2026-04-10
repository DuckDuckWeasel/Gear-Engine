using Scaffold.Navigation;

namespace Game.Race.Navigation
{
    /// <summary>
    /// DI handle for the race screen config (multiple <see cref="ViewConfig"/> instances in one scope).
    /// </summary>
    public sealed class RaceViewConfigRef
    {
        public RaceViewConfigRef(ViewConfig config)
        {
            Config = config;
        }

        public ViewConfig Config { get; }
    }
}
