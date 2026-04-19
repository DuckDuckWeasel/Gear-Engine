using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Tracks;
using GearEngine.FrustumFit;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public class ActiveRaceView : View<ActiveRaceViewModel>
    {
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private BoardViewComponent board;
        [SerializeField] private TrackTelemetryViewComponent telemetry;
        [SerializeField] private FrustumFitAnchor[] openTransitionAnchors;
        [SerializeField] private float openTransitionDurationSeconds = 0.35f;

        private readonly List<CarView> spawnedCars = new List<CarView>();

        protected override void OnBind()
        {
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[ActiveRaceView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

            track.Bind(viewModel.Track);
            SpawnAndBindCar();
            viewModel.StartRaceAfterCarReady();
        }

        private void LateUpdate()
        {
            if (telemetry != null && viewModel?.Car != null)
            {
                telemetry.UpdateFrom(viewModel.Car);
            }
        }

        private void SpawnAndBindCar()
        {
            CarViewModel carVm = viewModel.Car;
            if (carVm == null)
            {
                Debug.LogError("[ActiveRaceView] Car view-model is missing.");
                return;
            }

            GameObject prefab = carVm.Session.Car.Definition.CarPrefab;
            if (prefab == null)
            {
                Debug.LogError("[ActiveRaceView] CarPrefab is missing on CarDefinition.");
                return;
            }

            GameObject go = Instantiate(prefab, track.transform);
            if (!go.TryGetComponent(out CarView carView))
            {
                Debug.LogError("[ActiveRaceView] Spawned prefab is missing CarView.");
                Destroy(go);
                return;
            }

            carView.SplineContainer = track.SplineContainer;
            carView.Bind(carVm);
            carView.AttachRunner();
            spawnedCars.Add(carView);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            SetRaceSceneRootsActive(true);
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(this, openTransitionAnchors, openTransitionDurationSeconds);
        }

        protected override void OnFocus()
        {
            base.OnFocus();
            SetRaceSceneRootsActive(true);
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(this, openTransitionAnchors, openTransitionDurationSeconds);
        }

        protected override void OnClose()
        {
            base.OnClose();
            SetRaceSceneRootsActive(false);
        }

        protected override void OnUnbind()
        {
            DestroySpawnedCars();
            if (track != null)
            {
                track.Unbind();
            }

            SetRaceSceneRootsActive(false);
            base.OnUnbind();
        }

        private void DestroySpawnedCars()
        {
            foreach (CarView car in spawnedCars)
            {
                if (car != null)
                {
                    Destroy(car.gameObject);
                }
            }

            spawnedCars.Clear();
        }

        private void SetRaceSceneRootsActive(bool active)
        {
            if (track != null)
            {
                track.gameObject.SetActive(active);
            }

            if (board != null)
            {
                board.gameObject.SetActive(active);
            }

            if (telemetry != null)
            {
                telemetry.gameObject.SetActive(active);
            }
        }
    }
}
