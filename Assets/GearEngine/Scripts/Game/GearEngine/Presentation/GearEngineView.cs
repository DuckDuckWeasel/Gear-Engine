using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private SimulationControlView simControlView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private BoardView boardView;
        [SerializeField] private TextMeshProUGUI boardLimitLabel;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;

        [Header("Trash Zone")]
        [Tooltip("Assign the TrashDropZone prefab instance from the scene Canvas.")]
        [SerializeField] private TrashDropZoneView trashDropZone;

        private GearInteractionBinder interactionBinder;

        protected override void OnBind()
        {
            simControlView.Bind(viewModel.SimControl);

            interactionBinder = new GearInteractionBinder(
                boardView,
                inventoryView,
                viewModel.Board,
                viewModel.Inventory,
                boardLimitLabel,
                inventoryLimitLabel);
            interactionBinder.Bind();

            InitializeTrashZone();
        }

        private void InitializeTrashZone()
        {
            GearEngineFeatureToggleSO toggle = viewModel.FeatureToggle;

            if (toggle != null && !toggle.EnableTrashDeletion)
            {
                if (trashDropZone != null)
                {
                    trashDropZone.gameObject.SetActive(false);
                }
                return;
            }

            if (trashDropZone == null)
            {
                Debug.LogWarning("[GearEngineView] TrashDropZone reference is not assigned. Trash deletion will not work.");
                return;
            }

            // Ensure it starts hidden (the view manages its own show/hide via OnDragStarted/OnDragEnded)
            trashDropZone.gameObject.SetActive(false);

            // Position the zone relative to the board grid
            RepositionTrashZone(toggle);

            // Wire events
            if (viewModel.TrashService != null)
            {
                trashDropZone.OnInventoryGearDropped += viewModel.TrashService.HandleInventoryGearDropped;
                boardView.OnTrashDropRequested += viewModel.TrashService.RequestTrashDrop;
            }

            viewModel.DragService.OnDragStarted += HandleDragStartedForTrash;
            viewModel.DragService.OnDragEnded += trashDropZone.OnDragEnded;
        }

        private void RepositionTrashZone(GearEngineFeatureToggleSO toggle)
        {
            if (trashDropZone == null || viewModel.Board.BoardConfig == null)
            {
                return;
            }

            TrashZoneAlignment alignment = toggle != null ? toggle.TrashAlignment : TrashZoneAlignment.Right;
            float yOffset = viewModel.Board.BoardConfig.TrashZoneYOffset;

            Vector3 gridAnchorPoint = ComputeGridAnchor(viewModel.Board.BoardConfig, alignment);
            Vector2 pivot = ComputePivot(alignment);

            // Find the parent canvas for projection
            Canvas parentCanvas = trashDropZone.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                RectTransform rect = trashDropZone.GetComponent<RectTransform>();
                if (rect != null)
                {
                    CanvasPositionUtility.AnchorToWorldPosition(
                        rect, parentCanvas, gridAnchorPoint, new Vector2(0f, yOffset), pivot);
                }
            }
        }

        private void HandleDragStartedForTrash(object data)
        {
            if (trashDropZone == null)
            {
                Debug.LogWarning("[GearEngineView] HandleDragStartedForTrash: trashDropZone reference is null!");
                return;
            }

            if (data is GearConfigData gearData)
            {
                Debug.Log($"<color=#00ffff>[GearEngineView]</color> Forwarding drag start to trash zone for gear '{gearData.Id}'");
                trashDropZone.OnDragStarted(gearData);
            }
            else
            {
                Debug.LogWarning($"[GearEngineView] HandleDragStartedForTrash: data is not GearConfigData, type={data?.GetType().Name ?? "null"}");
            }
        }

        private static Vector2 ComputePivot(TrashZoneAlignment alignment)
        {
            switch (alignment)
            {
                case TrashZoneAlignment.Left:
                    return new Vector2(0f, 0.5f);
                case TrashZoneAlignment.Center:
                    return new Vector2(0.5f, 0.5f);
                case TrashZoneAlignment.Right:
                default:
                    return new Vector2(1f, 0.5f);
            }
        }

        private static Vector3 ComputeGridAnchor(BoardConfigSO boardConfig, TrashZoneAlignment alignment)
        {
            if (boardConfig == null)
            {
                return Vector3.zero;
            }

            int topY = boardConfig.GridHeight - 1;
            Vector3 topLeft = boardConfig.GetWorldPosition(new Vector2Int(0, topY));
            Vector3 topRight = boardConfig.GetWorldPosition(new Vector2Int(boardConfig.GridWidth - 1, topY));

            switch (alignment)
            {
                case TrashZoneAlignment.Left:
                    return topLeft;
                case TrashZoneAlignment.Center:
                    return (topLeft + topRight) * 0.5f;
                case TrashZoneAlignment.Right:
                default:
                    return topRight;
            }
        }

        private void OnDestroy()
        {
            interactionBinder?.Dispose();

            if (trashDropZone != null)
            {
                if (viewModel?.TrashService != null)
                {
                    trashDropZone.OnInventoryGearDropped -= viewModel.TrashService.HandleInventoryGearDropped;
                    if (boardView != null)
                    {
                        boardView.OnTrashDropRequested -= viewModel.TrashService.RequestTrashDrop;
                    }
                }

                if (viewModel?.DragService != null)
                {
                    viewModel.DragService.OnDragStarted -= HandleDragStartedForTrash;
                    viewModel.DragService.OnDragEnded -= trashDropZone.OnDragEnded;
                }
            }
        }
    }
}
