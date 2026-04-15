using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Tracks;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.Race;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Race.Presentation
{
    public sealed class RaceView : View<RaceViewModel>
    {
        [SerializeField] private Track track;
        [SerializeField] private Button raceButton;
        
        [Header("Gear UI")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private TextMeshProUGUI boardLimitLabel;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;

        [Header("Trash Zone")]
        [Tooltip("Assign the TrashDropZone prefab instance from the scene Canvas.")]
        [SerializeField] private TrashDropZoneView trashDropZone;

        private GearInteractionBinder interactionBinder;

        protected override void OnBind()
        {
            ValidateRaceViewHierarchy();

            track.Bind(viewModel.Track);

            interactionBinder = new GearInteractionBinder(
                boardView,
                inventoryView,
                viewModel.Board,
                viewModel.Inventory,
                boardLimitLabel,
                inventoryLimitLabel,
                () => viewModel.IsRaceRunning);
            interactionBinder.Bind();

            InitializeTrashZone();
            SubscribeRaceUi();
        }

        protected override void OnUnbind()
        {
            UnsubscribeRaceUi();
            CleanupTrashZone();

            interactionBinder?.Dispose();
            interactionBinder = null;

            if (track != null) track.Unbind();

            base.OnUnbind();
        }

        private void InitializeTrashZone()
        {
            GearEngineFeatureToggleSO toggle = viewModel.FeatureToggle;

            if (toggle != null && !toggle.EnableTrashDeletion)
            {
                Debug.Log("[RaceView] Trash deletion is disabled by FeatureToggle.");
                if (trashDropZone != null)
                {
                    trashDropZone.gameObject.SetActive(false);
                }
                return;
            }

            if (trashDropZone == null)
            {
                Debug.LogWarning("[RaceView] TrashDropZone reference is not assigned in the inspector. Trash deletion will not work.");
                return;
            }

            trashDropZone.gameObject.SetActive(false);

            if (viewModel.TrashService != null)
            {
                trashDropZone.OnInventoryGearDropped += viewModel.TrashService.HandleInventoryGearDropped;
                boardView.OnTrashDropRequested += viewModel.TrashService.RequestTrashDrop;
            }
            else
            {
                Debug.LogWarning("[RaceView] TrashService is null. Trash drop events will not fire.");
            }

            if (viewModel.DragService != null)
            {
                viewModel.DragService.OnDragStarted += HandleDragStartedForTrash;
                viewModel.DragService.OnDragEnded += trashDropZone.OnDragEnded;
                Debug.Log("[RaceView] Trash zone wired to DragService successfully.");
            }
            else
            {
                Debug.LogWarning("[RaceView] DragService is null. Trash zone will not show/hide during drag.");
            }
        }

        private void CleanupTrashZone()
        {
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

        private void HandleDragStartedForTrash(object data)
        {
            if (trashDropZone == null)
            {
                return;
            }

            if (data is GearConfigData gearData)
            {
                trashDropZone.OnDragStarted(gearData);
            }
        }

        private void OnRaceButtonClicked()
        {
            try
            {
                viewModel?.ToggleRace();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceView] ToggleRace failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SubscribeRaceUi()
        {
            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => viewModel.Track.State, OnTrackStateChanged);
            raceButton.onClick.AddListener(OnRaceButtonClicked);
        }

        private void OnTrackStateChanged(SimulationLifecycleState state)
        {
            TextMeshProUGUI label = raceButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = state == SimulationLifecycleState.Running ? "Stop" : "Start";
            }
        }

        private void UnsubscribeRaceUi()
        {
            if (raceButton != null)
            {
                raceButton.onClick.RemoveListener(OnRaceButtonClicked);
            }
        }

        private void ValidateRaceViewHierarchy()
        {
            ThrowIfMissing(track, "track");
            ThrowIfMissing(raceButton, "raceButton");
            ThrowIfMissing(boardView, "boardView");
            ThrowIfMissing(inventoryView, "inventoryView");
        }

        private void ThrowIfMissing(object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[RaceView] {name} reference is missing.");
            }
        }
    }
}
