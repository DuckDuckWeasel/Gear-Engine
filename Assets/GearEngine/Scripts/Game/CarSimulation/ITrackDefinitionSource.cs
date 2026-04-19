using GearEngine.CarSimulation.Definitions;

namespace GearEngine.CarSimulation
{
    /// <summary>Exposes the <see cref="TrackDefinition"/> used to drive <see cref="Tracks.TrackViewComponent"/> spline visuals.</summary>
    public interface ITrackDefinitionSource
    {
        TrackDefinition Track { get; }
    }
}
