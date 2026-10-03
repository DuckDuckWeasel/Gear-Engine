using GearEngine.GearEngine.Presentation.UI;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>Owns only temporary race presentation. Restores the shared board before another screen uses it.</summary>
    internal sealed class ClubSportBoardLayout
    {
        private const float k_gridWidthFill = 0.86f;
        private const float k_gridHeightFill = 0.88f;

        private RectTransform root;
        private RectTransform grid;
        private Vector2 anchorsMin;
        private Vector2 anchorsMax;
        private Vector2 position;
        private Vector2 size;
        private Vector3 scale;
        private Vector3 gridScale;
        private TMPro.TMP_Text capacity;
        private bool capacityEnabled;

        internal void Apply(BoardViewComponent board, ClubSportHudLayout layout)
        {
            Capture(board);
            Canvas.ForceUpdateCanvases();
            Rect region = layout.BoardAnchors();
            root.anchorMin = region.min;
            root.anchorMax = region.max;
            root.anchoredPosition = root.sizeDelta = Vector2.zero;
            root.localScale = Vector3.one;
            FitGrid(board);
        }

        internal void ApplyToCurrentRegion(BoardViewComponent board)
        {
            Capture(board);
            Canvas.ForceUpdateCanvases();
            FitGrid(board);
        }

        internal void Restore()
        {
            if (root == null)
            {
                return;
            }

            root.anchorMin = anchorsMin;
            root.anchorMax = anchorsMax;
            root.anchoredPosition = position;
            root.sizeDelta = size;
            root.localScale = scale;
            grid.localScale = gridScale;
            if (capacity != null)
            {
                capacity.enabled = capacityEnabled;
            }

            capacity = null;
            root = grid = null;
        }

        private void Capture(BoardViewComponent board)
        {
            if (root != null)
            {
                return;
            }

            root = (RectTransform)board.transform;
            grid = board.GetBoardSpaceRoot();
            anchorsMin = root.anchorMin;
            anchorsMax = root.anchorMax;
            position = root.anchoredPosition;
            size = root.sizeDelta;
            scale = root.localScale;
            gridScale = grid.localScale;
            capacity = root.Find("BoardCapacityLabel")?.GetComponent<TMPro.TMP_Text>();
            if (capacity != null)
            {
                capacityEnabled = capacity.enabled;
                capacity.enabled = false;
            }
        }

        private void FitGrid(BoardViewComponent board)
        {
            root.ForceUpdateRectTransforms();
            float spacing = board.BoardLayout.CellSpacing;
            float fit = Mathf.Min(root.rect.width * k_gridWidthFill / (7f * spacing),
                root.rect.height * k_gridHeightFill / (4f * spacing));
            grid.localScale = Vector3.one * fit;
        }
    }
}
