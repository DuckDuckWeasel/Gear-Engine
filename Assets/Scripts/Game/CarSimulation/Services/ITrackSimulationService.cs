namespace Game.CarSimulation
{
    public interface ITrackSimulationService
    {
        TrackViewModel TrackViewModel { get; }

        void CreateSimulation(CarDefinition carDefinition, TrackDefinition trackDefinition);

        void ToggleSimulation(bool isRunning);

        void CompleteSimulation();
    }
}
