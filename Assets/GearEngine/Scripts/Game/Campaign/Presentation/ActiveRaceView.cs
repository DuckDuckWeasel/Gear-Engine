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
        [SerializeField] private TMP_Text currentGearText;

        [Header("Roguelike Stats UI")]
        [SerializeField] private TMP_Text speedCapabilityText;
        [SerializeField] private TMP_Text corneringSkillText;
        [SerializeField] private TMP_Text driftText;
        [SerializeField] private TMP_Text precisionText;
        [SerializeField] private TMP_Text smoothnessText;

        private readonly List<CarView> spawnedCars = new List<CarView>();
        private bool raceStartPending;
        private float displayedRpm;
        private float displayedSpeed;
        private int lastDisplayedLap = 0;
        private int currentSimulatedGear = 1;
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
                    
                    // The race starts at lap 0. Crossing the line the first time completes lap 1 (currentLapRaw becomes 1).
                    if (currentLapRaw > lastDisplayedLap)
                    {
                        if (lapCompletedSound.IsValid()) BroAudio.Play(lapCompletedSound);
                    }
                    lastDisplayedLap = currentLapRaw;

                    int displayLap = Mathf.Clamp(currentLapRaw, 0, viewModel.Track.Session.TotalLaps);
                    currentLapText.text = $"Lap {displayLap}/{viewModel.Track.Session.TotalLaps}";
                }

                if (currentVelocityText != null && viewModel.Car != null)
                {
                    float targetSpeed = Mathf.Abs(viewModel.Car.Speed);
                    float speedLerp = targetSpeed < displayedSpeed ? 15f : 2f;
                    displayedSpeed = Mathf.Lerp(displayedSpeed, targetSpeed, Time.deltaTime * speedLerp);
                    currentVelocityText.text = $"{displayedSpeed:F0}";
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
                if (currentGearText != null) currentGearText.text = "N";
                return;
            }

            float speed = viewModel.Car.Speed;
            float absSpeed = Mathf.Abs(speed);
            
            float maxSpeed = viewModel.Car.MaxSpeed;
            if (maxSpeed < 10f) maxSpeed = 200f;
            
            int totalGears = 6;
            // Progressive distribution: lower gears have smaller speed ranges, so they shift faster
            float[] gearSpeedPercents = { 0f, 0.12f, 0.28f, 0.48f, 0.72f, 1.00f, 1.30f }; 
            
            float shiftUpSpeed = gearSpeedPercents[currentSimulatedGear] * maxSpeed;
            float shiftDownSpeed = gearSpeedPercents[currentSimulatedGear - 1] * maxSpeed - 8f; // Hysteresis
            
            if (absSpeed > shiftUpSpeed && currentSimulatedGear < totalGears)
            {
                currentSimulatedGear++;
            }
            else if (absSpeed < shiftDownSpeed && currentSimulatedGear > 1)
            {
                currentSimulatedGear--;
            }
            
            float gearMinSpeed = gearSpeedPercents[currentSimulatedGear - 1] * maxSpeed;
            float gearMaxSpeed = gearSpeedPercents[currentSimulatedGear] * maxSpeed;
            float currentGearRange = gearMaxSpeed - gearMinSpeed;

            float speedInGear = Mathf.Clamp(absSpeed - gearMinSpeed, 0f, currentGearRange);
            float t = currentGearRange > 0f ? speedInGear / currentGearRange : 1f; 
            
            float baseRpm = currentSimulatedGear * 1000f;
            // Gear 1 targets 3000, Gear 2 targets 4000, ..., Gear 6 targets 8000
            float targetRpm = 2000f + (currentSimulatedGear * 1000f); 
            string gearString = currentSimulatedGear.ToString();
            
            // Reverse state
            if (speed < -1f)
            {
                gearString = "R";
                baseRpm = 1000f;
                targetRpm = 4500f;
            }
            // Idle state
            else if (absSpeed < 1f)
            {
                gearString = "N";
                baseRpm = 0f;
                targetRpm = 0f;
                t = 0f;
            }
            // Coasting / decelerating
            else if (!viewModel.Car.IsAccelerating)
            {
                t *= 0.2f; // Drop RPM significantly when off-throttle
            }

            // Simple linear interpolation without jitter for clear, readable values
            float rawRpm = Mathf.Lerp(baseRpm, targetRpm, t);
            
            // Snappy drop (gear shift / brake), smooth rise (acceleration)
            float lerpSpeed = rawRpm < displayedRpm ? 20f : 2f; 
            displayedRpm = Mathf.Lerp(displayedRpm, rawRpm, Time.deltaTime * lerpSpeed);

            // Diegetic RPM rounding (nearest 50)
            float diegeticRpm = Mathf.Round(displayedRpm / 50f) * 50f;
            currentRpmText.text = $"{diegeticRpm:F0}";
            if (currentGearText != null)
            {
                currentGearText.text = gearString;
            }
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
