using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private GearEngineCoreViewComponent coreView;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        protected override void OnBind()
        {
            coreView?.BindPresentation(viewModel);
            InitializeTrashZone();
            Bind<bool, bool>(() => viewModel.IsRunning, OnSimulationStateChanged);
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(OnToggleButtonClicked);
            }
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(OnToggleButtonClicked);
            }
        }

        private void OnToggleButtonClicked()
        {
            viewModel.ToggleSimulation();
        }

        private void OnSimulationStateChanged(bool isRunning)
        {
            if (buttonText != null)
            {
                buttonText.text = isRunning ? "STOP ENGINE" : "START ENGINE";
                buttonText.color = isRunning ? Color.red : Color.green;
            }
        }

        private void InitializeTrashZone()
        {
            TrashDropZoneViewComponent trashDropZone = coreView != null ? coreView.TrashDropZone : null;
            GearEngineFeatureToggleSO toggle = viewModel.FeatureToggle;

            if (toggle != null && !toggle.EnableTrashDeletion)
            {
                if (trashDropZone != null)
                {
                    trashDropZone.ZoneRect.gameObject.SetActive(false);
                }

                return;
            }

            Assert.IsNotNull(trashDropZone, "[GearEngineView] TrashDropZone reference is not assigned. Trash deletion will not work.");

            trashDropZone.ZoneRect.gameObject.SetActive(false);
            RepositionTrashZone(trashDropZone);
        }

        private void RepositionTrashZone(TrashDropZoneViewComponent trashDropZone)
        {
            if (trashDropZone == null || viewModel.Board.BoardConfig == null)
            {
                return;
            }

            TrashZoneAlignment alignment = TrashZoneAlignment.Right;
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
    }
}
