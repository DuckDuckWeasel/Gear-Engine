using System;

namespace GearEngine.GearEngine
{
    public sealed class GearEngineService : IGearEngineService
    {
        public GearEngineService(IGridManager gridManager)
        {
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
        }

        public bool IsRunning => gridManager.IsRunning;

        private readonly IGridManager gridManager;

        public void Play()
        {
            gridManager.Play();
        }

        public void Stop()
        {
            gridManager.Stop();
        }

        public void ResetGridSimulationState()
        {
            gridManager.Stop();
            gridManager.ResetAllNodeSimulationState();
        }
    }
}
