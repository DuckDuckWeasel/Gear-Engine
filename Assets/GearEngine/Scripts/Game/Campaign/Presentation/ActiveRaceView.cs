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
        private bool raceStartPending;

        protected override void OnBind()
        {
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[ActiveRaceView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

            track.Bind(viewModel.Track);
            SpawnAndBindCar();

            // Defer race start (and prop generation) until after the FrustumFit
            // open transition has positioned the track at its final screen location.
            raceStartPending = true;
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
            PlayFrustumTransitionThenStartRace();
        }

        protected override void OnFocus()
        {
            base.OnFocus();
            SetRaceSceneRootsActive(true);
            PlayFrustumTransitionThenStartRace();
        }

        /// <summary>
        /// Plays the FrustumFit open transition and, once the track is at its
        /// final screen-space position, starts the race (which triggers prop generation).
        /// </summary>
        private void PlayFrustumTransitionThenStartRace()
        {
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(
                this,
                openTransitionAnchors,
                openTransitionDurationSeconds,
                onComplete: OnFrustumTransitionComplete);
        }

        private void OnFrustumTransitionComplete()
        {
            if (!raceStartPending)
            {
                return;
            }

            raceStartPending = false;

            // Generate props now that the track is at its final position.
            if (track != null)
            {
                track.BroadcastMessage("Generate", SendMessageOptions.DontRequireReceiver);
            }

            viewModel.StartRaceAfterCarReady();
        }

        protected override void OnClose()
        {
            base.OnClose();
            SetRaceSceneRootsActive(false);
        }

        protected override void OnUnbind()
        {
            raceStartPending = false;
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
