using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Visuals;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Owns a single drag ghost parented under the board root so scale matches placed gears without canvas/world ratio math.
    /// </summary>
    public sealed class DragGhostController
    {
        private readonly Transform boardRoot;
        private GameObject ghost;

        public DragGhostController(Transform boardRoot)
        {
            this.boardRoot = boardRoot;
        }

        public GameObject Ghost => ghost;

        /// <summary>
        /// Spawns <see cref="GearConfigData.ViewPrefab"/> under <see cref="boardRoot"/> via <see cref="GearView.BindForDisplay"/>.
        /// </summary>
        /// <param name="config">Gear config; requires a non-null <see cref="GearConfigData.ViewPrefab"/>.</param>
        /// <param name="ghostAlpha">CanvasGroup alpha for semi-transparent feedback.</param>
        public void CreateGhost(GearConfigData config, float ghostAlpha = 0.6f)
        {
            try
            {
                DestroyGhost();
                if (config == null || boardRoot == null || config.ViewPrefab == null)
                {
                    return;
                }

                GearView view = UnityEngine.Object.Instantiate(config.ViewPrefab, boardRoot, false);
                view.name = "DragGhost";
                view.BindForDisplay(config, DisplayOptions.Ghost(ghostAlpha));
                ghost = view.gameObject;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DragGhostController] CreateGhost failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void MoveGhostTo(Vector3 worldPosition)
        {
            if (ghost == null)
            {
                return;
            }

            ghost.transform.position = worldPosition;
        }

        public void DestroyGhost()
        {
            if (ghost == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(ghost);
            ghost = null;
        }
    }
}
