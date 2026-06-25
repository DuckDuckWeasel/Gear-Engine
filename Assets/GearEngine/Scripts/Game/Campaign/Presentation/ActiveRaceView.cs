using System;
using System.Collections.Generic;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Tracks;
using GearEngine.FrustumFit;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using Ami.BroAudio;

namespace GearEngine.Campaign.Presentation
{
    public class ActiveRaceView : View<ActiveRaceViewModel>
    {
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private BoardViewComponent board;
        [SerializeField] private TrackTelemetryViewComponent telemetry;
        [SerializeField] private FrustumFitAnchor[] openTransitionAnchors;
        [SerializeField] private float openTransitionDurationSeconds = 0.35f;

        [Header("Audio")]
        [SerializeField] private SoundID startRaceSound;
        [SerializeField] private SoundID lapCompletedSound;
        [SerializeField] private SoundID raceFinishedSound;

        [Header("Telemetry UI")]
        [SerializeField] private TMP_Text raceTimeText;
        [SerializeField] private TMP_Text currentVelocityText;
        [SerializeField] private TMP_Text currentLapText;
        [SerializeField] private TMP_Text currentRpmText;

        [Header("Roguelike Stats UI")]
        [SerializeField] private TMP_Text speedCapabilityText;
        [SerializeField] private TMP_Text corneringSkillText;
        [SerializeField] private TMP_Text driftText;
        [SerializeField] private TMP_Text precisionText;
        [SerializeField] private TMP_Text smoothnessText;

        private readonly List<CarView> spawnedCars = new List<CarView>();
        private bool raceStartPending;
        private float displayedRpm;
        private int lastDisplayedLap = 0;
        private SimulationLifecycleState lastTrackState = SimulationLifecycleState.Created;

        protected override void OnBind()
        {
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[ActiveRaceView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

            track.Bind(viewModel.Track);
            SpawnAndBindCar();

            lastDisplayedLap = 0;
            lastTrackState = viewModel.Track?.State ?? SimulationLifecycleState.Created;

            // Defer race start (and prop generation) until after the FrustumFit
            // open transition has positioned the track at its final screen location.
            raceStartPending = true;
            UpdateStatsUI();
        }

        private void LateUpdate()
        {
            if (viewModel == null) return;

            if (telemetry != null && viewModel.Car != null)
            {
                telemetry.UpdateFrom(viewModel.Car);
            }

            if (viewModel.Track != null)
            {
                if (lastTrackState != SimulationLifecycleState.Completed && viewModel.Track.State == SimulationLifecycleState.Completed)
                {
                    if (raceFinishedSound.IsValid()) BroAudio.Play(raceFinishedSound);
                }
                lastTrackState = viewModel.Track.State;
            }

            if (viewModel.Track?.Session != null)
            {
                if (raceTimeText != null)
                {
                    TimeSpan time = TimeSpan.FromSeconds(viewModel.Track.Session.RaceTime);
                    raceTimeText.text = $"{(int)time.TotalSeconds:00}:{time:ff}";
                }

                if (currentLapText != null)
                {
                    int currentLapRaw = viewModel.Track.Session.CurrentLap;
                    if (currentLapRaw > lastDisplayedLap && lastDisplayedLap > 0)
                    {
                        if (lapCompletedSound.IsValid()) BroAudio.Play(lapCompletedSound);
                    }
                    lastDisplayedLap = currentLapRaw;

                    // Optionally clamp to 1 if the race starts at lap 0 before crossing the line
                    int displayLap = Mathf.Clamp(currentLapRaw, 1, viewModel.Track.Session.TotalLaps);
                    currentLapText.text = $"Lap {displayLap}/{viewModel.Track.Session.TotalLaps}";
                }

                if (currentVelocityText != null && viewModel.Car != null)
                {
                    currentVelocityText.text = $"{viewModel.Car.Speed:F0}";
                }
            }

            UpdateStatsUI();
            UpdateFakeRpmUI();
        }

        private void UpdateFakeRpmUI()
        {
            if (currentRpmText == null || viewModel?.Car == null) return;

            if (viewModel.Track?.State == SimulationLifecycleState.Completed)
            {
                float lerpSpeedDown = 5f; 
                displayedRpm = Mathf.Lerp(displayedRpm, 0f, Time.deltaTime * lerpSpeedDown);
                currentRpmText.text = $"{displayedRpm:F0}";
                return;
            }

            float absSpeed = Mathf.Abs(viewModel.Car.Speed);
            float gearSpeedRange = 35f; // Simulate a gear shift every 35 km/h
            
            int currentGear = Mathf.FloorToInt(absSpeed / gearSpeedRange) + 1;
            float speedInGear = absSpeed % gearSpeedRange;
            float t = speedInGear / gearSpeedRange;
            
            // Adding a small jitter for realism
            float jitter = UnityEngine.Random.Range(-50f, 50f);
            
            float baseRpm = currentGear == 1 ? 1000f : 4000f;
            float targetRpm = 7500f;
            
            // Idle state
            if (absSpeed < 1f)
            {
                baseRpm = 1000f;
                targetRpm = 1000f;
                jitter = UnityEngine.Random.Range(-20f, 20f);
            }

            float rawRpm = Mathf.Lerp(baseRpm, targetRpm, t) + jitter;
            
            // Lerp towards the raw RPM for smoother UI transitions
            float lerpSpeed = 2; 
            displayedRpm = Mathf.Lerp(displayedRpm, rawRpm, Time.deltaTime * lerpSpeed);

            currentRpmText.text = $"{displayedRpm:F0}";
        }

        private void UpdateStatsUI()
        {
            if (viewModel?.Track?.Session?.Config == null) return;

            RoguelikeCarStats stats = viewModel.Track.Session.Config.RoguelikeStats;

            if (speedCapabilityText != null) speedCapabilityText.text = $"Speed Cap: {stats.SpeedCapability:F0}";
            if (corneringSkillText != null) corneringSkillText.text = $"Cornering: {stats.CorneringSkill:F0}";
            if (driftText != null) driftText.text = $"Drift: {stats.Drift:F0}";
            if (precisionText != null) precisionText.text = $"Precision: {stats.Precision:F0}";
            if (smoothnessText != null) smoothnessText.text = $"Smoothness: {stats.Smoothness:F0}";
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

        protected override void OnOpen(bool wasHidden)
        {
            base.OnOpen(wasHidden);
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

            if (startRaceSound.IsValid())
            {
                BroAudio.Play(startRaceSound).OnEnd(_ => 
                {
                    if (this != null && viewModel != null)
                    {
                        viewModel.StartRaceAfterCarReady();
                    }
                });
            }
            else
            {
                viewModel.StartRaceAfterCarReady();
            }
        }

        protected override void OnClose(bool hiding)
        {
            base.OnClose(hiding);
            if (hiding)
            {
                return;
            }

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
