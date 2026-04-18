namespace GearEngine.GearEngine
{
    public interface IGearEngineService
    {
        bool IsRunning { get; }
        void Play();
        void Stop();

        /// <summary>Stops the engine and clears per-run gear simulation state (rotation, charge, etc.).</summary>
        void ResetGridSimulationState();
    }
}
