using GearEngine.GearEngine.Presentation.World;
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

        protected override void OnBind()
        {
            if (simControlView)
            {
                simControlView.Bind(viewModel.SimControl);
            }

            BindBoard();
            BindInventory();
            BindLimits();
            SubscribeGearEvents();
            InitializeTrashZone();
        }

        private void BindBoard()
        {
            boardView.Bind(viewModel.Board);

            FrustumFit frustumFit = FindObjectOfType<FrustumFit>();
            if (frustumFit != null)
            {
                frustumFit.Apply();
            }
        }

        private void BindInventory()
        {
            inventoryView.Bind(viewModel.Inventory);
        }

        private void BindLimits()
        {
            Bind<string, string>(() => viewModel.BoardLimitText, text => {
                if (boardLimitLabel != null) boardLimitLabel.text = text;
            });

            Bind<string, string>(() => viewModel.InventoryLimitText, text => {
                if (inventoryLimitLabel != null) inventoryLimitLabel.text = text;
            });
        }

        private void SubscribeGearEvents()
        {
            boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;
            viewModel.Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;
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

            trashDropZone.gameObject.SetActive(false);
            trashDropZone.Bind(viewModel.TrashZone);
            RepositionTrashZone(toggle);

            if (viewModel.TrashService != null)
            {
                boardView.OnTrashDropRequested += viewModel.TrashService.RequestTrashDrop;
            }
        }

        // ── Handlers ────────────────────────────────────────────

        private void HandleGearDroppedOverUI(GearConfigData config, Vector3 _)
        {
            viewModel.ReturnGearToInventory(config);
        }

        private void HandleGearDraggedToBoard(Vector3 worldPos, GearConfigData gearData)
        {
            Vector3 localPos = worldPos - boardView.transform.position;
            viewModel.TryPlaceFromInventory(localPos, gearData);
        }

        // ── Trash Zone Positioning ──────────────────────────────

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

        // ── Cleanup ─────────────────────────────────────────────

        private void OnDestroy()
        {
            if (boardView != null)
            {
                boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
                if (viewModel?.TrashService != null)
                {
                    boardView.OnTrashDropRequested -= viewModel.TrashService.RequestTrashDrop;
                }
                boardView.Unbind();
            }

            if (viewModel != null)
            {
                viewModel.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            }

            if (trashDropZone != null)
            {
                trashDropZone.Unbind();
            }
        }
    }
}
