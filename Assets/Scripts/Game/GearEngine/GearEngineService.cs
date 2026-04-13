using System;

namespace Scaffold.GearEngine
{
    public sealed class GearEngineService : IGearEngineService
    {
        public GearEngineService(IGridManager gridManager)
        {
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
        }

        public bool IsRunning => gridManager.IsRunning;

        public void Play()
        {
            gridManager.Play();
        }

        public void Stop()
        {
            gridManager.Stop();
        }

        private readonly IGridManager gridManager;
    }
}
