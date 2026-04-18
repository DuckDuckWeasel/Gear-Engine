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

            if (viewModel.Sessions.Count == 0)
            {
                return;
            }

            track.Bind(new TrackViewModel(viewModel.Sessions[0]));

            foreach (LapRaceSession session in viewModel.Sessions)
            {
                TrySpawnCar(session);
            }
        }

        private void TrySpawnCar(LapRaceSession session)
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

        private bool TryRegisterSpawnedCar(GameObject go, LapRaceSession session)
        {
            if (!go.TryGetComponent(out CarView carView))
            {
                Debug.LogError("[CarTrackTestView] Spawned prefab is missing CarView.");
                Destroy(go);
                return false;
            }

            carView.Initialize(session.Car, track.SplineContainer, session);
            session.SetClockRunning(true);
            spawnedCars.Add(carView);
            return true;
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
            base.OnUnbind();
        }
    }
}
