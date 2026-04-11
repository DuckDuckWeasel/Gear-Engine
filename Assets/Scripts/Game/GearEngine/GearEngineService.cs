using System;

namespace Game.GearEngine
{
    public sealed class GearEngineService : IGearEngineService
    {
        private readonly IGridManager gridManager;
        private readonly IGearSceneElement sceneElement;

        public GearEngineService(IGridManager gridManager, IGearSceneElement sceneElement)
        {
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.sceneElement = sceneElement ?? throw new ArgumentNullException(nameof(sceneElement));
        }

        /// <summary>Exposes the scene element for hosts that need explicit lifecycle (composition root), not used by Play/Stop.</summary>
        public IGearSceneElement SceneElement => sceneElement;

        public bool IsRunning => gridManager.IsRunning;

        public void Play() => gridManager.Play();

        public void Stop() => gridManager.Stop();
    }
}
