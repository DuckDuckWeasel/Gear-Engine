using System.Collections.Generic;
using GearEngine.CarSimulation;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GearEngine.CarSimulation.Tracks;

namespace GearEngine.CarSimulation.Presentation
{
    /// <summary>Sample view for the spline track test scene: hosts <see cref="TrackViewComponent"/> as a reusable ViewComponent.</summary>
    public sealed class CarTrackTestView : View<TrackListViewModel>
    {
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private Button raceButton;

        private readonly List<CarView> spawnedCars = new List<CarView>();
        private CarViewModel primaryCar;

        protected override void OnBind()
        {
            if (track == null)
            {
                Debug.LogError("[CarTrackTestView] Assign the Track ViewComponent reference.");
                return;
            }

            if (viewModel.Sessions.Count == 0)
            {
                return;
            }

            track.Bind(new TrackViewModel(viewModel.Sessions[0], viewModel.RaceManager, viewModel.AiRunner, viewModel.Factory));

            foreach (Simulation.RaceState session in viewModel.Sessions)
            {
                TrySpawnCar(session);
                session.PresentationChanged += RefreshButtonState;
            }

            if (raceButton != null)
            {
                raceButton.onClick.AddListener(viewModel.ToggleRace);
            }

            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            if (raceButton == null || viewModel.Sessions.Count == 0) return;
            
            TextMeshProUGUI label = raceButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = viewModel.Sessions[0].Phase == SimulationLifecycleState.Running ? "Stop" : "Start";
            }
        }

        private void TrySpawnCar(Simulation.RaceState session)
        {
            GameObject prefab = session.Car.Definition.CarPrefab;
            if (prefab == null)
            {
                Debug.LogError("[CarTrackTestView] CarPrefab is missing on CarDefinition.");
                return;
            }

            GameObject go = Instantiate(prefab, track.transform);
            if (!TryRegisterSpawnedCar(go, session))
            {
                return;
            }
        }

        private bool TryRegisterSpawnedCar(GameObject go, Simulation.RaceState session)
        {
            if (!go.TryGetComponent(out CarView carView))
            {
                Debug.LogError("[CarTrackTestView] Spawned prefab is missing CarView.");
                Destroy(go);
                return false;
            }

            carView.SplineContainer = track.SplineContainer;
            var cvm = new CarViewModel(session, viewModel.AiRunner);
            carView.Bind(cvm);
            
            if (primaryCar == null)
            {
                primaryCar = cvm;
            }

            spawnedCars.Add(carView);
            return true;
        }

        private void Update()
        {
            if (primaryCar != null && track != null)
            {
                track.UpdateTelemetryUI(
                    primaryCar.Speed,
                    primaryCar.Progress,
                    primaryCar.IsBraking,
                    primaryCar.IsDrifting,
                    primaryCar.IsAccelerating,
                    primaryCar.Session.CurrentLap,
                    primaryCar.Session.TotalLaps,
                    primaryCar.CurrentAcceleration,
                    primaryCar.Session.RaceTime,
                    primaryCar.Session.LapTimes
                );
            }
        }

        protected override void OnUnbind()
        {
            foreach (CarView car in spawnedCars)
            {
                if (car != null)
                {
                    Destroy(car.gameObject);
                }
            }

            spawnedCars.Clear();
            primaryCar = null;

            if (track != null)
            {
                track.Unbind();
            }

            foreach (Simulation.RaceState session in viewModel.Sessions)
            {
                session.PresentationChanged -= RefreshButtonState;
            }

            if (raceButton != null)
            {
                raceButton.onClick.RemoveListener(viewModel.ToggleRace);
            }

            base.OnUnbind();
        }
    }
}
