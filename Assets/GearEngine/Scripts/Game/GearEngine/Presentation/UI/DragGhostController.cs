using System;
using GearEngine.GearEngine.Config;
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
        /// Spawns the gear visual under <see cref="boardRoot"/> with the same local scale as board gears.
        /// </summary>
        /// <param name="config">Gear config; uses the GearVisual child under <see cref="GearConfigData.ViewPrefab"/> and <see cref="GearConfigData.RelativeScaleMultiplier"/>.</param>
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

                Transform template = config.ViewPrefab.transform.Find("GearVisual");
                if (template == null)
                {
                    Debug.LogError($"[DragGhostController] Gear '{config.Id}' ViewPrefab has no child named GearVisual.");
                    return;
                }

                ghost = UnityEngine.Object.Instantiate(template.gameObject, boardRoot, false);
                ghost.name = "DragGhost";
                float uniform = config.RelativeScaleMultiplier;
                ghost.transform.localScale = new Vector3(uniform, uniform, uniform);
                ApplyGhostCanvasGroup(ghost, ghostAlpha);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DragGhostController] CreateGhost failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void ApplyGhostCanvasGroup(GameObject root, float ghostAlpha)
        {
            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = root.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = ghostAlpha;
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
