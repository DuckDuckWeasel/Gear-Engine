using System;

namespace Game.GearEngine
{
    public sealed class GearEngineService : IGearEngineService
    {
        private readonly IGridManager gridManager;

        public GearEngineService(IGridManager gridManager)
        {
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
        }

        public bool IsRunning => gridManager.IsRunning;

        public void Play() => gridManager.Play();

        public void Stop() => gridManager.Stop();
    }
}
