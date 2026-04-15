using System.Collections.Generic;
using GearEngine.CarSimulation;
using Scaffold.MVVM;
using UnityEngine;
using TrackViewComponent = GearEngine.CarSimulation.Tracks.Track;

namespace GearEngine.CarSimulation.Presentation
{
    /// <summary>Sample view for the spline track test scene: hosts <see cref="GearEngine.CarSimulation.Tracks.Track"/> as a reusable ViewComponent.</summary>
    public sealed class CarTrackTestView : View<TrackListViewModel>
    {
        [SerializeField] private TrackViewComponent track;

        private readonly List<CarView> spawnedCars = new List<CarView>();

        protected override void OnBind()
        {
            if (track == null)
            {
                Debug.LogError("[CarTrackTestView] Assign the Track ViewComponent reference.");
                return;
            }

            if (viewModel.Simulations.Count == 0)
            {
                return;
            }

            track.Bind(new TrackViewModel(viewModel.Simulations[0]));

            foreach (TrackSimulation sim in viewModel.Simulations)
            {
                SpawnCar(sim);
            }
        }

        private void SpawnCar(TrackSimulation sim)
        {
            GameObject prefab = sim.Car.CarPrefab;
            if (prefab == null)
            {
                Debug.LogError("[CarTrackTestView] CarPrefab is missing on CarDefinition.");
                return;
            }

            GameObject go = Instantiate(prefab, track.transform);
            if (!go.TryGetComponent(out CarView carView))
            {
                Debug.LogError("[CarTrackTestView] Spawned prefab is missing CarView.");
                Destroy(go);
                return;
            }

            carView.Initialize(sim.Car, track.SplineContainer, sim);
            carView.OnRunningChanged(SimulationLifecycleState.Running);
            spawnedCars.Add(carView);
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
            if (track != null)
            {
                track.Unbind();
            }

            base.OnUnbind();
        }
    }
}
