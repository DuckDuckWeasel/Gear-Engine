using System;
using System.Collections.Generic;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.SplineSimulation;
using GearEngine.CarSimulation.Tracks;
using GearEngine.FrustumFit;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using Ami.BroAudio;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Splines;

namespace GearEngine.Campaign.Presentation
{
    public class ActiveRaceView : View<ActiveRaceViewModel>
    {
        private const float k_raceBoardAnchorMinY = 0.02f;
        private const float k_raceBoardAnchorMaxY = 0.32f;
        private const float k_raceCarVisualScale = 1.2f;
        private const int k_opponentCount = 3;
        private const float k_gridFrontDistance = 8f;
        private const float k_gridRowSpacing = 8f;
        private const float k_gridLaneOffset = 1.3f;
        private const float k_raceGroundMargin = 18f;
        private const float k_positionMarkerScreenOffset = 20f;
        private const int k_raceBoardSortingOrder = 30000;
        private const string k_boardCapacityChipName = "chips_cogs";
        private static readonly Color32 s_finalLapColor = new Color32(255, 204, 0, 255);
        private static readonly Color32 s_finishedLapColor = new Color32(239, 70, 78, 255);
        private static readonly Color32 s_playerPositionMarkerColor = new Color32(255, 213, 64, 255);
        private static readonly Color32 s_opponentPositionMarkerColor = new Color32(248, 239, 220, 255);
        private static readonly Color32 s_positionMarkerOutlineColor = new Color32(53, 65, 74, 255);
        private static readonly Vector2 s_trackViewportAnchorMin = new Vector2(0.12f, 0.58f);
        private static readonly Vector2 s_trackViewportAnchorMax = new Vector2(0.88f, 0.9f);
        private const float k_hudLabelOutlineWidth = 0.12f;
        private static readonly Color32 s_hudLabelOutlineColor = new Color32(245, 239, 226, 255);
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private BoardView board;
        [SerializeField] private TrackTelemetryViewComponent telemetry;
        [SerializeField] private RaceDriftScoreView driftScore;
        [SerializeField] private ClubSportHudView clubSportHud;
        private readonly ClubSportBoardLayout clubSportBoard = new ClubSportBoardLayout();
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
        [SerializeField] private TMP_Text rpmLabelText;
        [SerializeField] private TMP_Text scoreLabelText;
        [SerializeField] private Image[] rpmSegments = Array.Empty<Image>();

        [Header("Roguelike Stats UI")]
        [SerializeField] private TMP_Text speedCapabilityText;
        [SerializeField] private TMP_Text corneringSkillText;
        [SerializeField] private TMP_Text driftText;
        [SerializeField] private TMP_Text precisionText;
        [SerializeField] private TMP_Text smoothnessText;

        private readonly List<CarView> spawnedCars = new List<CarView>();
        private readonly List<SplineEvaluateDriver> opponentDrivers = new List<SplineEvaluateDriver>();
        private readonly List<RacePositionMarker> positionMarkers = new List<RacePositionMarker>();
        private readonly List<RacePositionMarker> rankedPositionMarkers = new List<RacePositionMarker>();
        private SplineEvaluateRunnerService opponentRunner;
        private bool raceStartPending;
        private float displayedRpm;
        private float displayedSpeed;
        private RectTransform raceBoardRect;
        private Vector2 boardAnchorMin;
        private Vector2 boardAnchorMax;
        private Canvas raceBoardCanvas;
        private RenderMode boardCanvasRenderMode;
        private Camera boardCanvasWorldCamera;
        private int boardCanvasSortingOrder;
        private bool hasBoardCanvasSnapshot;
        private int lastDisplayedLap = 0;
        private int currentSimulatedGear = 1;
        private SimulationLifecycleState lastTrackState = SimulationLifecycleState.Created;
        private string lastDisplayedGear = "";
        private Color normalLapColor;
        private Transform raceGround;
        private Vector3 originalGroundScale;
        private Vector3 originalGroundPosition;
        private int originalCanvasSortingOrder;
        private GameObject hudBackdrop;
        private Texture2D hudCardTexture;
        private Sprite hudCardSprite;
        private int raceDrivingSeed;
        private Vector2 lastHudCanvasSize;
        private bool raceFinishPresentationReleased;

        private void Awake()
        {
            ConfigureHudLabels();
            if (currentLapText != null)
            {
                normalLapColor = currentLapText.color;
            }
        }

        protected override void OnBind()
        {
            if (track == null || board == null)
            {
                throw new InvalidOperationException(
                    "[ActiveRaceView] Track and BoardView must be assigned on the scene instance.");
            }

            track.Bind(viewModel.Track);
            ConfigureRaceBackdrop();
            clubSportHud?.Bind(viewModel.Track.Session, viewModel.DriftScore);
            ConfigureTrackViewport();
            if (clubSportHud == null)
            {
                driftScore?.Bind(viewModel.DriftScore);
            }

            board.BindReadOnly(viewModel.Board);
            HideBoardCapacityChip();

            raceDrivingSeed = UnityEngine.Random.Range(1, int.MaxValue - 32768);
            SpawnAndBindCar();
            raceFinishPresentationReleased = positionMarkers.Count == 0;
            viewModel.SetRaceFinishPresentationReady(raceFinishPresentationReleased);

            lastDisplayedLap = 0;
            lastTrackState = viewModel.Track?.State ?? SimulationLifecycleState.Created;
            ResetTelemetryUI();
            UpdateHudBackdropLayout();

            // Defer race start (and prop generation) until after the FrustumFit
            // open transition has positioned the track at its final screen location.
            raceStartPending = true;
            UpdateStatsUI();
        }

        private void LateUpdate()
        {
            if (viewModel == null)
            {
                return;
            }

            ApplyRaceBoardCanvasState();
            HideBoardCapacityChip();

            RectTransform canvasRect = transform as RectTransform;
            if (hudBackdrop != null && canvasRect != null && canvasRect.rect.size != lastHudCanvasSize)
            {
                UpdateHudBackdropLayout();
            }

            if (clubSportHud != null && clubSportHud.Layout.Refresh())
            {
                ConfigureTrackViewport();
                transform.Find("TrackViewport").GetComponent<FrustumFitAnchor>().Apply();
                ApplyRaceBoardLayout();
            }
            viewModel.Tick(Time.deltaTime);
            clubSportHud?.Tick(Time.deltaTime);
            UpdateRacePositionMarkers();
            UpdateRaceFinishes();

            if (telemetry != null && viewModel.Car != null)
            {
                telemetry.UpdateFrom(viewModel.Car);
            }

            if (viewModel.Track != null)
            {
                if (lastTrackState != SimulationLifecycleState.Completed && viewModel.Track.State == SimulationLifecycleState.Completed)
                {
                    if (raceFinishedSound.IsValid())
                    {
                        BroAudio.Play(raceFinishedSound);
                    }
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

                    if (currentLapRaw > lastDisplayedLap)
                    {
                        if (lapCompletedSound.IsValid())
                        {
                            BroAudio.Play(lapCompletedSound);
                        }

                        currentLapText.transform.DOKill(true);
                        currentLapText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 1f);
                    }
                    lastDisplayedLap = currentLapRaw;

                    int displayLap = GetDisplayedLap(currentLapRaw, viewModel.Track.Session.TotalLaps);
                    currentLapText.text = $"Lap {displayLap}/{viewModel.Track.Session.TotalLaps}";
                    currentLapText.color = GetLapColor(
                        currentLapRaw, viewModel.Track.Session.TotalLaps, normalLapColor);
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
            UpdateRpmSegments();
        }

        private void UpdateRpmSegments()
        {
            float progress = Mathf.Clamp01(displayedRpm / 8000f) * rpmSegments.Length;
            for (int i = 0; i < rpmSegments.Length; i++)
            {
                if (rpmSegments[i] != null)
                {
                    rpmSegments[i].fillAmount = Mathf.Clamp01(progress - i);
                }
            }
        }

        private void UpdateFakeRpmUI()
        {
            if (currentRpmText == null || viewModel?.Car == null)
            {
                return;
            }

            if (viewModel.Track?.State == SimulationLifecycleState.Completed)
            {
                float lerpSpeedDown = 5f;
                displayedRpm = Mathf.Lerp(displayedRpm, 0f, Time.deltaTime * lerpSpeedDown);
                currentRpmText.text = ToRpmText(displayedRpm);
                UpdateGearText("N");
                return;
            }

            float speed = viewModel.Car.Speed;
            float absSpeed = Mathf.Abs(speed);

            float maxSpeed = viewModel.Car.MaxSpeed;
            if (maxSpeed < 10f)
            {
                maxSpeed = 200f;
            }

            int totalGears = 6;
            // Progressive distribution: lower gears have smaller speed ranges, so they shift faster
            float[] gearSpeedPercents = { 0f, 0.12f, 0.28f, 0.48f, 0.72f, 1.00f, 1.30f };

            float shiftUpSpeed = gearSpeedPercents[currentSimulatedGear] * maxSpeed;
            float shiftDownSpeed = gearSpeedPercents[currentSimulatedGear - 1] * maxSpeed - 8f; // Hysteresis

            if (absSpeed > shiftUpSpeed && currentSimulatedGear < totalGears)
            {
                currentSimulatedGear++;
                // Shift up jerk (momentary loss of torque)
                float severity = 0.10f / currentSimulatedGear;
                if (viewModel?.Car?.RunnerService != null)
                {
                    viewModel.Car.RunnerService.ApplyJerk(viewModel.Car.Session.Car, severity);
                }
            }
            else if (absSpeed < shiftDownSpeed && currentSimulatedGear > 1)
            {
                currentSimulatedGear--;
                // Shift down jerk (engine braking, slightly more severe)
                float severity = 0.15f / currentSimulatedGear;
                if (viewModel?.Car?.RunnerService != null)
                {
                    viewModel.Car.RunnerService.ApplyJerk(viewModel.Car.Session.Car, severity);
                }
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
            currentRpmText.text = ToRpmText(diegeticRpm);
            UpdateGearText(gearString);
        }

        private string ToRpmText(float rpm)
        {
            return $"{rpm:F0}";
        }

        private void ConfigureHudLabels()
        {
            ConfigureHudLabel(rpmLabelText);
            ConfigureHudLabel(scoreLabelText);
        }

        private void ConfigureHudLabel(TMP_Text label)
        {
            if (label == null)
            {
                return;
            }

            label.outlineColor = s_hudLabelOutlineColor;
            label.outlineWidth = k_hudLabelOutlineWidth;
        }

        private void ResetTelemetryUI()
        {
            displayedRpm = 0f;
            displayedSpeed = 0f;
            currentSimulatedGear = 1;
            lastDisplayedGear = string.Empty;

            if (currentLapText != null && viewModel?.Track?.Session != null)
            {
                currentLapText.text = $"Lap 1/{viewModel.Track.Session.TotalLaps}";
                currentLapText.color = normalLapColor;
            }

            if (currentVelocityText != null)
            {
                currentVelocityText.text = "0";
            }

            if (currentRpmText != null)
            {
                currentRpmText.text = ToRpmText(0f);
            }

            UpdateGearText("N");
            UpdateRpmSegments();
        }

        private void UpdateGearText(string gearString)
        {
            if (currentGearText == null)
            {
                return;
            }

            if (lastDisplayedGear != gearString)
            {
                bool isUp = false;
                bool isDown = false;

                if (int.TryParse(lastDisplayedGear, out int lastG) && int.TryParse(gearString, out int newG))
                {
                    if (newG > lastG)
                    {
                        isUp = true;
                    }
                    else if (newG < lastG)
                    {
                        isDown = true;
                    }
                }
                else if (gearString == "1" && (lastDisplayedGear == "N" || lastDisplayedGear == "R"))
                {
                    isUp = true;
                }
                else if ((gearString == "N" || gearString == "R") && int.TryParse(lastDisplayedGear, out _))
                {
                    isDown = true;
                }

                currentGearText.transform.DOKill(true);
                currentGearText.transform.localScale = Vector3.one;
                currentGearText.transform.localRotation = Quaternion.identity;

                if (clubSportHud != null && clubSportHud.ReducedMotion)
                {
                    lastDisplayedGear = gearString;
                    currentGearText.text = gearString;
                    return;
                }

                if (isUp)
                {
                    currentGearText.transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, 6, 1f);
                }
                else if (isDown)
                {
                    currentGearText.transform.DOPunchScale(Vector3.one * -0.3f, 0.3f, 6, 1f);
                    currentGearText.transform.DOPunchRotation(new Vector3(0, 0, -15f), 0.3f, 6, 1f);
                }
                else if (!string.IsNullOrEmpty(lastDisplayedGear))
                {
                    currentGearText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 1f);
                }

                lastDisplayedGear = gearString;
            }

            currentGearText.text = gearString;
        }

        private void UpdateStatsUI()
        {
            if (viewModel?.Track?.Session?.Config == null)
            {
                return;
            }

            RoguelikeCarStats stats = viewModel.Track.Session.Config.RoguelikeStats;

            if (speedCapabilityText != null)
            {
                speedCapabilityText.text = $"Speed Cap: {stats.SpeedCapability:F0}";
            }

            if (corneringSkillText != null)
            {
                corneringSkillText.text = $"Cornering: {stats.CorneringSkill:F0}";
            }

            if (driftText != null)
            {
                driftText.text = $"Drift: {stats.Drift:F0}";
            }

            if (precisionText != null)
            {
                precisionText.text = $"Precision: {stats.Precision:F0}";
            }

            if (smoothnessText != null)
            {
                smoothnessText.text = $"Smoothness: {stats.Smoothness:F0}";
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
            go.transform.localScale *= k_raceCarVisualScale;
            if (!go.TryGetComponent(out CarView carView))
            {
                Debug.LogError("[ActiveRaceView] Spawned prefab is missing CarView.");
                Destroy(go);
                return;
            }

            ApplyClubSportCarLivery(go, 0, true);
            carView.SplineContainer = track.SplineContainer;
            carView.Bind(carVm);
            carView.AttachRunner();
            spawnedCars.Add(carView);
            SpawnOpponents(prefab);

            if (opponentRunner != null)
            {
                SplineEvaluateDriver playerDriver = opponentRunner.GetDriver(carVm.Car);
                Vector2 grid = GetGridPlacement(4, track.SplineContainer.Spline.GetLength());
                if (playerDriver != null)
                {
                    playerDriver.SetCurveDecisionSeed(GetCarDrivingSeed(4));
                    playerDriver.SetStartGridPosition(grid.x, grid.y);
                    CreatePositionMarker(carView, playerDriver, 4, true);
                }
            }
        }

        private void ConfigureTrackViewport()
        {
            RectTransform viewport = transform.Find("TrackViewport") as RectTransform;
            FrustumFitAnchor anchor = viewport != null ? viewport.GetComponent<FrustumFitAnchor>() : null;
            if (viewport == null || anchor == null)
            {
                Debug.LogError("[ActiveRaceView] TrackViewport requires a FrustumFitAnchor.");
                return;
            }

            viewport.localScale = Vector3.one;
            viewport.anchorMin = s_trackViewportAnchorMin;
            viewport.anchorMax = s_trackViewportAnchorMax;
            if (clubSportHud != null)
            {
                Rect region = clubSportHud.Layout.RoadAnchors();
                viewport.anchorMin = region.min;
                viewport.anchorMax = region.max;
            }
            viewport.anchoredPosition = Vector2.zero;
            viewport.sizeDelta = Vector2.zero;
            anchor.SetTargetTransform(track.transform);
        }

        private void ConfigureRaceBackdrop()
        {
            raceGround = clubSportHud == null ? track.transform.Find("Floor") : null;
            if (raceGround != null)
            {
                originalGroundScale = raceGround.localScale;
                originalGroundPosition = raceGround.localPosition;
                MeshFilter floorFilter = raceGround.GetComponent<MeshFilter>();
                Transform road = track.transform.Find("Track/Path");
                MeshFilter roadFilter = road != null ? road.GetComponent<MeshFilter>() : null;
                if (floorFilter?.sharedMesh != null && roadFilter?.sharedMesh != null &&
                    roadFilter.sharedMesh.bounds.size.x > 1f && roadFilter.sharedMesh.bounds.size.z > 1f)
                {
                    Bounds roadBounds = roadFilter.sharedMesh.bounds;
                    Vector3 roadCenter = track.transform.InverseTransformPoint(road.TransformPoint(roadBounds.center));
                    Vector3 floorSize = floorFilter.sharedMesh.bounds.size;
                    raceGround.localPosition = new Vector3(roadCenter.x, originalGroundPosition.y, roadCenter.z);
                    raceGround.localScale = GetRaceGroundScale(roadBounds.size, floorSize, originalGroundScale.y);
                }
                else
                {
                    if (Application.isPlaying)
                    {
                        Debug.LogError("[ActiveRaceView] Race backdrop requires the road and floor meshes.");
                    }
                    raceGround.localScale = originalGroundScale * 0.5f;
                }
            }

            Canvas raceCanvas = GetComponent<Canvas>();
            if (raceCanvas != null)
            {
                originalCanvasSortingOrder = raceCanvas.sortingOrder;
                raceCanvas.sortingOrder = 20;
            }

            if (clubSportHud != null)
            {
                return;
            }

            hudBackdrop = new GameObject("RaceHudBackdrop", typeof(RectTransform));
            RectTransform backdropRect = hudBackdrop.GetComponent<RectTransform>();
            backdropRect.SetParent(transform, false);
            backdropRect.anchorMin = new Vector2(0.5f, 0.5f);
            backdropRect.anchorMax = backdropRect.anchorMin;
            Transform container = transform.Find("Container");
            if (container != null)
            {
                backdropRect.SetSiblingIndex(container.GetSiblingIndex());
            }

            hudCardSprite = CreateHudCardSprite();
            CreateHudCardLayer(backdropRect, "Shadow", new Color32(17, 24, 28, 90), -10f);
            CreateHudCardLayer(backdropRect, "Surface", new Color32(247, 238, 220, 250), 0f);
            UpdateHudBackdropLayout();
        }

        private Sprite CreateHudCardSprite()
        {
            const int size = 96;
            const float radius = 30f;
            float half = (size - 1) * 0.5f;
            float straight = half - radius;
            hudCardTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(Mathf.Abs(x - half) - straight, 0f);
                    float dy = Mathf.Max(Mathf.Abs(y - half) - straight, 0f);
                    float alpha = Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            hudCardTexture.SetPixels32(pixels);
            hudCardTexture.Apply(false, true);
            return Sprite.Create(hudCardTexture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(31f, 31f, 31f, 31f));
        }

        private void CreateHudCardLayer(RectTransform parent, string name, Color32 color, float verticalOffset)
        {
            GameObject layer = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = layer.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = new Vector2(0f, verticalOffset);

            Image image = layer.GetComponent<Image>();
            image.sprite = hudCardSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
        }

        private void UpdateHudBackdropLayout()
        {
            RectTransform card = hudBackdrop != null ? hudBackdrop.GetComponent<RectTransform>() : null;
            RectTransform canvasRect = transform as RectTransform;
            if (card == null || canvasRect == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            Vector2 minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            IncludeHudBounds(transform.Find("Container/CarStatusHud") as RectTransform,
                canvasRect, ref minimum, ref maximum);
            IncludeHudBounds(transform.Find("Container/CarStatusHud/Score_PanelRace") as RectTransform,
                canvasRect, ref minimum, ref maximum);
            IncludeHudBounds(currentVelocityText?.rectTransform.parent as RectTransform,
                canvasRect, ref minimum, ref maximum);
            IncludeHudBounds(currentGearText?.rectTransform, canvasRect, ref minimum, ref maximum);
            IncludeHudBounds(currentVelocityText?.rectTransform, canvasRect, ref minimum, ref maximum);
            IncludeHudBounds(rpmLabelText?.rectTransform, canvasRect, ref minimum, ref maximum);
            IncludeHudBounds(currentRpmText?.rectTransform, canvasRect, ref minimum, ref maximum);
            IncludeHudBounds(scoreLabelText?.rectTransform, canvasRect, ref minimum, ref maximum);
            foreach (Image segment in rpmSegments)
            {
                IncludeHudBounds(segment?.rectTransform, canvasRect, ref minimum, ref maximum);
            }

            if (minimum.x > maximum.x)
            {
                Debug.LogError("[ActiveRaceView] Race HUD card needs at least one telemetry element.");
                return;
            }

            Vector2 contentSize = maximum - minimum;
            Vector2 padding = new Vector2(Mathf.Max(24f, contentSize.x * 0.09f),
                Mathf.Max(18f, contentSize.y * 0.14f));
            card.anchoredPosition = (minimum + maximum) * 0.5f - canvasRect.rect.center;
            card.sizeDelta = contentSize + padding * 2f;
            lastHudCanvasSize = canvasRect.rect.size;
        }

        private static void IncludeHudBounds(RectTransform element, RectTransform canvasRect,
            ref Vector2 minimum, ref Vector2 maximum)
        {
            if (element == null)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            element.GetWorldCorners(corners);
            foreach (Vector3 corner in corners)
            {
                Vector3 local = canvasRect.InverseTransformPoint(corner);
                Vector2 point = new Vector2(local.x, local.y);
                minimum = Vector2.Min(minimum, point);
                maximum = Vector2.Max(maximum, point);
            }
        }

        private static Vector3 GetRaceGroundScale(Vector3 roadSize, Vector3 floorSize, float floorHeight)
        {
            return new Vector3(
                (roadSize.x + 2f * k_raceGroundMargin) / floorSize.x,
                floorHeight,
                (roadSize.z + 2f * k_raceGroundMargin) / floorSize.z);
        }

        private void RestoreRaceBackdrop()
        {
            if (raceGround != null)
            {
                raceGround.localScale = originalGroundScale;
                raceGround.localPosition = originalGroundPosition;
                raceGround = null;
            }

            Canvas raceCanvas = GetComponent<Canvas>();
            if (raceCanvas != null)
            {
                raceCanvas.sortingOrder = originalCanvasSortingOrder;
            }

            if (hudBackdrop != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(hudBackdrop);
                }
                else
                {
                    DestroyImmediate(hudBackdrop);
                }
                hudBackdrop = null;
            }

            if (hudCardSprite != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(hudCardSprite);
                }
                else
                {
                    DestroyImmediate(hudCardSprite);
                }
                hudCardSprite = null;
            }

            if (hudCardTexture != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(hudCardTexture);
                }
                else
                {
                    DestroyImmediate(hudCardTexture);
                }
                hudCardTexture = null;
            }
        }

        private void SpawnOpponents(GameObject prefab)
        {
            opponentRunner = viewModel.Car.RunnerService as SplineEvaluateRunnerService;
            if (opponentRunner == null)
            {
                Debug.LogError("[ActiveRaceView] Opponent cars require the spline simulation runner.");
                return;
            }

            float trackLength = track.SplineContainer.Spline.GetLength();
            CarEntityFactory factory = new CarEntityFactory();

            for (int i = 0; i < k_opponentCount; i++)
            {
                GameObject go = Instantiate(prefab, track.transform);
                go.transform.localScale *= k_raceCarVisualScale;
                go.name = $"OpponentCar{i + 1}";
                CarView carView = go.GetComponent<CarView>();
                if (carView == null || carView.DrivenTransform == null)
                {
                    Debug.LogError("[ActiveRaceView] Opponent prefab requires a configured CarView.");
                    Destroy(go);
                    continue;
                }

                carView.PrepareForSplineSimulation();
                ApplyClubSportCarLivery(go, i + 1, false);
                CarEntity entity = factory.Create(viewModel.Car.Car.Definition);
                int drivingSeed = GetCarDrivingSeed(i + 1);
                DriverPersonality personality = DriverPersonality.CreateOpponent(i, drivingSeed);
                SplineEvaluateDriver driver = opponentRunner.InitializeRun(
                    track.SplineContainer, carView.DrivenTransform, entity, personality);
                if (driver == null)
                {
                    Destroy(go);
                    continue;
                }

                Vector2 grid = GetGridPlacement(i + 1, trackLength);
                driver.SetCurveDecisionSeed(drivingSeed);
                driver.SetStartGridPosition(grid.x, grid.y);
                opponentDrivers.Add(driver);
                spawnedCars.Add(carView);
                CreatePositionMarker(carView, driver, i + 1, false);
            }
        }

        private void CreatePositionMarker(
            CarView carView, SplineEvaluateDriver driver, int gridPosition, bool isPlayer)
        {
            if (carView == null || carView.DrivenTransform == null || driver == null)
            {
                Debug.LogError("[ActiveRaceView] Race position marker requires a car transform and driver.");
                return;
            }

            RectTransform markerLayer = transform as RectTransform;
            if (markerLayer == null)
            {
                Debug.LogError("[ActiveRaceView] Race position markers require a screen-space RectTransform.");
                return;
            }

            TMP_FontAsset font = currentLapText != null ? currentLapText.font : TMP_Settings.defaultFontAsset;
            GameObject markerObject = CreatePositionMarkerVisual(markerLayer, font, gridPosition, isPlayer);
            TextMeshProUGUI label = markerObject.GetComponentInChildren<TextMeshProUGUI>();
            Transform arrow = markerObject.transform.Find("PositionArrow");
            Image leftArrowFill = arrow != null ? arrow.Find("LeftFill").GetComponent<Image>() : null;
            Image rightArrowFill = arrow != null ? arrow.Find("RightFill").GetComponent<Image>() : null;

            positionMarkers.Add(new RacePositionMarker(
                markerObject, carView.DrivenTransform, driver, label,
                leftArrowFill, rightArrowFill, gridPosition, isPlayer));
        }

        private static GameObject CreatePositionMarkerVisual(
            RectTransform parent, TMP_FontAsset font, int gridPosition, bool isPlayer)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            GameObject markerObject = new GameObject("RacePositionMarker", typeof(RectTransform));
            RectTransform markerRect = markerObject.GetComponent<RectTransform>();
            markerRect.SetParent(parent, false);
            markerRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerRect.anchorMax = markerRect.anchorMin;
            markerRect.pivot = new Vector2(0.5f, 0f);
            markerRect.sizeDelta = new Vector2(112f, 90f);
            markerRect.localScale = Vector3.one;
            markerRect.localRotation = Quaternion.identity;
            markerRect.SetAsLastSibling();

            GameObject labelObject = new GameObject("PositionLabel", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(markerRect, false);
            labelRect.anchorMin = isPlayer ? new Vector2(0f, 0.44f) : Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = 52f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = GetRacePositionMarkerColor(isPlayer);
            label.outlineColor = s_positionMarkerOutlineColor;
            label.outlineWidth = 0.25f;
            // Update the instance material directly so the rendered outline always matches the cached TMP color.
            // fontMaterial is owned by this TMP instance and disposed with the label.
            Material markerMaterial = label.fontMaterial;
            markerMaterial.SetColor(ShaderUtilities.ID_OutlineColor, s_positionMarkerOutlineColor);
            markerMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
            markerMaterial.DisableKeyword("UNDERLAY_ON");
            markerMaterial.DisableKeyword("UNDERLAY_INNER");
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.extraPadding = true;
            label.raycastTarget = false;
            label.text = GetRacePositionLabel(gridPosition);

            if (!isPlayer)
            {
                return markerObject;
            }

            GameObject arrowObject = new GameObject("PositionArrow", typeof(RectTransform));
            RectTransform arrowRect = arrowObject.GetComponent<RectTransform>();
            arrowRect.SetParent(markerRect, false);
            arrowRect.anchorMin = new Vector2(0.5f, 0f);
            arrowRect.anchorMax = arrowRect.anchorMin;
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(0f, 18f);
            arrowRect.localScale = Vector3.one * 0.8f;
            arrowRect.sizeDelta = new Vector2(48f, 34f);

            CreatePositionMarkerArrowStroke(
                arrowRect, "LeftOutline", new Vector2(-8f, 15f), new Vector2(32f, 12f), -42f,
                s_positionMarkerOutlineColor);
            CreatePositionMarkerArrowStroke(
                arrowRect, "RightOutline", new Vector2(8f, 15f), new Vector2(32f, 12f), 42f,
                s_positionMarkerOutlineColor);
            Color32 markerColor = GetRacePositionMarkerColor(isPlayer);
            CreatePositionMarkerArrowStroke(
                arrowRect, "LeftFill", new Vector2(-8f, 15f), new Vector2(27f, 6f), -42f, markerColor);
            CreatePositionMarkerArrowStroke(
                arrowRect, "RightFill", new Vector2(8f, 15f), new Vector2(27f, 6f), 42f, markerColor);
            return markerObject;
        }

        private static Image CreatePositionMarkerArrowStroke(
            RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size,
            float rotationDegrees, Color color)
        {
            GameObject strokeObject = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform strokeRect = strokeObject.GetComponent<RectTransform>();
            strokeRect.SetParent(parent, false);
            strokeRect.anchorMin = new Vector2(0.5f, 0f);
            strokeRect.anchorMax = strokeRect.anchorMin;
            strokeRect.pivot = new Vector2(0.5f, 0.5f);
            strokeRect.anchoredPosition = anchoredPosition;
            strokeRect.sizeDelta = size;
            strokeRect.localRotation = Quaternion.Euler(0f, 0f, rotationDegrees);

            Image stroke = strokeObject.GetComponent<Image>();
            stroke.color = color;
            stroke.raycastTarget = false;
            return stroke;
        }

        private void UpdateRacePositionMarkers()
        {
            Camera worldCamera = Camera.main;
            RectTransform markerLayer = transform as RectTransform;
            Canvas raceCanvas = GetComponent<Canvas>();
            Camera uiCamera = raceCanvas != null && raceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? raceCanvas.worldCamera
                : null;
            rankedPositionMarkers.Clear();
            foreach (RacePositionMarker marker in positionMarkers)
            {
                if (marker.Root == null || marker.Target == null || marker.Driver == null)
                {
                    continue;
                }

                marker.IsVisible = false;
                rankedPositionMarkers.Add(marker);
                if (worldCamera == null || markerLayer == null)
                {
                    marker.Root.SetActive(false);
                    continue;
                }

                Vector3 targetScreenPoint3D = worldCamera.WorldToScreenPoint(marker.Target.position);
                bool isVisible = targetScreenPoint3D.z > 0f;
                marker.Root.SetActive(isVisible);
                if (!isVisible)
                {
                    continue;
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    markerLayer, targetScreenPoint3D, uiCamera, out Vector2 carAnchor);
                marker.CarAnchoredPosition = carAnchor;
                marker.IsVisible = true;
            }

            UpdatePositionStandings(viewModel?.Track?.Session?.TotalLaps ?? 0);
            for (int i = 0; i < rankedPositionMarkers.Count; i++)
            {
                RacePositionMarker marker = rankedPositionMarkers[i];
                int position = marker.FinishedPosition > 0 ? marker.FinishedPosition : i + 1;
                if (marker.IsPlayer)
                {
                    clubSportHud?.UpdateStanding(position, rankedPositionMarkers.Count, marker.Driver.IsInStartingGrid,
                        marker.FinishedPosition > 0 || marker.FinishTriggered, Time.deltaTime);
                }
                marker.Label.text = GetRacePositionLabel(position);
                SetRacePositionMarkerColor(marker, GetRacePositionMarkerColor(marker.IsPlayer));
            }

            Rect markerBounds = markerLayer.rect;
            if (clubSportHud != null)
            {
                Rect roadRegion = clubSportHud.Layout.RoadAnchors();
                markerBounds.yMin = markerLayer.rect.yMin + roadRegion.yMin * markerLayer.rect.height;
                markerBounds.yMax = markerLayer.rect.yMin + roadRegion.yMax * markerLayer.rect.height;
            }
            foreach (RacePositionMarker marker in positionMarkers)
            {
                if (!marker.IsVisible)
                {
                    continue;
                }

                UpdatePositionMarkerLayout(marker, markerBounds);
            }
        }

        private static void UpdatePositionMarkerLayout(RacePositionMarker marker, Rect containerBounds)
        {
            RectTransform rect = (RectTransform)marker.Root.transform;
            rect.anchoredPosition = CalculatePositionMarkerAnchor(
                marker.CarAnchoredPosition, rect.sizeDelta, rect.pivot, containerBounds);
            rect.localRotation = Quaternion.identity;
            marker.Root.SetActive(true);
            if (marker.Arrow != null)
            {
                marker.Arrow.localRotation = Quaternion.identity;
            }
        }

        private static Vector2 CalculatePositionMarkerAnchor(
            Vector2 carAnchor, Vector2 markerSize, Vector2 pivot, Rect containerBounds)
        {
            Vector2 desired = carAnchor + Vector2.up * k_positionMarkerScreenOffset;
            return ClampPositionMarkerAnchor(desired, markerSize, pivot, containerBounds);
        }

        private void UpdatePositionStandings(int totalLaps)
        {
            rankedPositionMarkers.Sort(CompareRacePosition);
            if (totalLaps <= 0)
            {
                return;
            }
            int finishedCount = 0;
            bool hasNewFinisher = false;
            foreach (RacePositionMarker marker in rankedPositionMarkers)
            {
                if (marker.FinishedPosition > 0)
                {
                    finishedCount++;
                }
                else if (marker.Driver.State.CompletedLaps >= totalLaps)
                {
                    hasNewFinisher = true;
                }
            }
            if (!hasNewFinisher)
            {
                return;
            }
            // Compare simultaneous crossings by the fraction of this simulation tick before the line.
            rankedPositionMarkers.Sort((left, right) => CompareFinishCrossing(left, right, totalLaps));
            foreach (RacePositionMarker marker in rankedPositionMarkers)
            {
                if (marker.FinishedPosition == 0 && marker.Driver.State.CompletedLaps >= totalLaps)
                {
                    marker.FinishedPosition = ++finishedCount;
                }
            }
            rankedPositionMarkers.Sort(CompareRacePosition);
        }

        private static int CompareFinishCrossing(RacePositionMarker left, RacePositionMarker right, int totalLaps)
        {
            bool leftCrossed = left.FinishedPosition == 0 && left.Driver.State.CompletedLaps >= totalLaps;
            bool rightCrossed = right.FinishedPosition == 0 && right.Driver.State.CompletedLaps >= totalLaps;
            if (leftCrossed && rightCrossed)
            {
                float leftDistance = 1f - left.Driver.State.PreviousT;
                float rightDistance = 1f - right.Driver.State.PreviousT;
                float leftFraction = leftDistance / Mathf.Max(0.0001f, leftDistance + left.Driver.State.T);
                float rightFraction = rightDistance / Mathf.Max(0.0001f, rightDistance + right.Driver.State.T);
                int comparison = leftFraction.CompareTo(rightFraction);
                return comparison != 0 ? comparison : left.GridPosition.CompareTo(right.GridPosition);
            }
            return CompareRacePosition(left, right);
        }

        private static Vector2 ClampPositionMarkerAnchor(
            Vector2 anchor, Vector2 markerSize, Vector2 pivot, Rect containerBounds)
        {
            float x = Mathf.Clamp(
                anchor.x,
                containerBounds.xMin + markerSize.x * pivot.x,
                containerBounds.xMax - markerSize.x * (1f - pivot.x));
            float y = Mathf.Clamp(
                anchor.y,
                containerBounds.yMin + markerSize.y * pivot.y,
                containerBounds.yMax - markerSize.y * (1f - pivot.y));
            return new Vector2(x, y);
        }

        private void UpdateRaceFinishes()
        {
            if (raceFinishPresentationReleased || viewModel?.Track?.Session == null)
            {
                return;
            }

            int totalLaps = viewModel.Track.Session.TotalLaps;
            if (totalLaps <= 0 || positionMarkers.Count == 0)
            {
                ReleaseRaceFinishPresentation();
                return;
            }

            bool allFinishMovesComplete = true;
            foreach (RacePositionMarker marker in positionMarkers)
            {
                if (marker.Driver == null || marker.Driver.State.CompletedLaps < totalLaps)
                {
                    allFinishMovesComplete = false;
                    continue;
                }

                if (!marker.FinishTriggered)
                {
                    marker.Driver.TriggerCinematicFinish();
                    marker.Driver.SetPaused(true);
                    marker.FinishTriggered = true;
                }

                if (!marker.Driver.IsCinematicFinishComplete)
                {
                    allFinishMovesComplete = false;
                }
            }

            if (allFinishMovesComplete)
            {
                ReleaseRaceFinishPresentation();
            }
        }

        private void ReleaseRaceFinishPresentation()
        {
            raceFinishPresentationReleased = true;
            viewModel.SetRaceFinishPresentationReady(true);
        }

        private static int CompareRacePosition(RacePositionMarker left, RacePositionMarker right)
        {
            if (left.FinishedPosition > 0 || right.FinishedPosition > 0)
            {
                int leftPlace = left.FinishedPosition > 0 ? left.FinishedPosition : int.MaxValue;
                int rightPlace = right.FinishedPosition > 0 ? right.FinishedPosition : int.MaxValue;
                return leftPlace.CompareTo(rightPlace);
            }
            int progressComparison = GetRaceProgress(right.Driver).CompareTo(GetRaceProgress(left.Driver));
            return progressComparison != 0 ? progressComparison : left.GridPosition.CompareTo(right.GridPosition);
        }

        private static float GetRaceProgress(SplineEvaluateDriver driver)
        {
            return CalculateRaceProgress(driver.State.CompletedLaps, driver.State.T, driver.IsInStartingGrid);
        }

        private static float CalculateRaceProgress(int completedLaps, float normalizedProgress, bool isInStartingGrid)
        {
            float startingGridOffset = isInStartingGrid ? 1f : 0f;
            return completedLaps + normalizedProgress - startingGridOffset;
        }

        private static string GetRacePositionLabel(int position)
        {
            int moduloHundred = position % 100;
            string suffix = moduloHundred is >= 11 and <= 13
                ? "th"
                : (position % 10) switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th"
                };
            return $"{position}{suffix}";
        }

        private static Color32 GetRacePositionMarkerColor(bool isPlayer)
        {
            return isPlayer ? s_playerPositionMarkerColor : s_opponentPositionMarkerColor;
        }

        private static void SetRacePositionMarkerColor(RacePositionMarker marker, Color32 color)
        {
            marker.Label.color = color;
            if (marker.LeftArrowFill != null)
            {
                marker.LeftArrowFill.color = color;
            }
            if (marker.RightArrowFill != null)
            {
                marker.RightArrowFill.color = color;
            }
        }

        private static int GetDisplayedLap(int completedLaps, int totalLaps)
        {
            return Mathf.Clamp(completedLaps + 1, 1, totalLaps);
        }

        private static Color GetLapColor(int completedLaps, int totalLaps, Color normalColor)
        {
            if (completedLaps >= totalLaps)
            {
                return s_finishedLapColor;
            }

            return GetDisplayedLap(completedLaps, totalLaps) >= totalLaps
                ? s_finalLapColor
                : normalColor;
        }

        private static Vector2 GetGridPlacement(int position, float trackLength)
        {
            float distance = k_gridFrontDistance + ((position - 1) / 2) * k_gridRowSpacing;
            float lane = position % 2 == 1 ? -k_gridLaneOffset : k_gridLaneOffset;
            return new Vector2(-distance / Mathf.Max(trackLength, 1f), lane);
        }

        private int GetCarDrivingSeed(int gridPosition)
        {
            return raceDrivingSeed + gridPosition * 7919;
        }

        private static void ApplyClubSportCarLivery(GameObject car, int liveryIndex, bool isPlayer)
        {
            if (!car.TryGetComponent(out ClubSportCarLiveryView livery))
            {
                Debug.LogError("[ActiveRaceView] Race cars require ClubSportCarLiveryView.");
                return;
            }

            livery.Apply(liveryIndex, isPlayer);
        }

        private void SetOpponentsPaused(bool paused)
        {
            foreach (SplineEvaluateDriver driver in opponentDrivers)
            {
                driver.SetPaused(paused);
            }
        }

        private void StartRaceWithOpponents()
        {
            SetOpponentsPaused(false);
            viewModel.StartRaceAfterCarReady();
        }

        protected override void OnOpen(bool wasHidden)
        {
            base.OnOpen(wasHidden);
            if (wasHidden)
            {
                ConfigureRaceBackdrop();
            }

            if (clubSportHud != null && clubSportHud.State == null)
            {
                clubSportHud.Bind(viewModel.Track.Session, viewModel.DriftScore);
            }

            ConfigureTrackViewport();
            SetRaceSceneRootsActive(true);
            HideBoardCapacityChip();
            ApplyRaceBoardLayout();
            if (board != null)
            {
                board.Board.SetAllGearsRapidSpin(true);
            }
            PlayFrustumTransitionThenStartRace();
        }

        private void ApplyRaceBoardLayout()
        {
            if (board == null || board.Board == null)
            {
                return;
            }

            if (raceBoardRect == null)
            {
                raceBoardRect = board.Board.transform as RectTransform;
                if (raceBoardRect != null)
                {
                    boardAnchorMin = raceBoardRect.anchorMin;
                    boardAnchorMax = raceBoardRect.anchorMax;
                    raceBoardRect.anchorMin = new Vector2(boardAnchorMin.x, k_raceBoardAnchorMinY);
                    raceBoardRect.anchorMax = new Vector2(boardAnchorMax.x, k_raceBoardAnchorMaxY);
                }
            }

            ConfigureRaceBoardCanvas();
            if (clubSportHud != null)
            {
                clubSportBoard.Apply(board.Board, clubSportHud.Layout);
            }
        }

        private void ConfigureRaceBoardCanvas()
        {
            if (raceBoardCanvas == null)
            {
                raceBoardCanvas = board.GetComponent<Canvas>();
            }

            if (raceBoardCanvas == null)
            {
                Debug.LogError("[ActiveRaceView] BoardView requires a Canvas for race presentation.");
                return;
            }

            if (!hasBoardCanvasSnapshot)
            {
                boardCanvasRenderMode = raceBoardCanvas.renderMode;
                boardCanvasWorldCamera = raceBoardCanvas.worldCamera;
                boardCanvasSortingOrder = raceBoardCanvas.sortingOrder;
                hasBoardCanvasSnapshot = true;
            }

            ApplyRaceBoardCanvasState();
        }

        private void ApplyRaceBoardCanvasState()
        {
            if (!hasBoardCanvasSnapshot || raceBoardCanvas == null)
            {
                return;
            }

            raceBoardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            raceBoardCanvas.worldCamera = null;
            raceBoardCanvas.sortingOrder = k_raceBoardSortingOrder;
        }

        private void RestoreBoardLayout()
        {
            clubSportBoard.Restore();
            if (raceBoardRect != null)
            {
                raceBoardRect.anchorMin = boardAnchorMin;
                raceBoardRect.anchorMax = boardAnchorMax;
                raceBoardRect = null;
            }

            if (hasBoardCanvasSnapshot && raceBoardCanvas != null)
            {
                raceBoardCanvas.worldCamera = boardCanvasWorldCamera;
                raceBoardCanvas.renderMode = boardCanvasRenderMode;
                raceBoardCanvas.sortingOrder = boardCanvasSortingOrder;
            }

            raceBoardCanvas = null;
            hasBoardCanvasSnapshot = false;
        }

        /// <summary>
        /// Plays the FrustumFit open transition and, once the track is at its
        /// final screen-space position, starts the race (which triggers prop generation).
        /// </summary>
        private void PlayFrustumTransitionThenStartRace()
        {
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(
                this,
                clubSportHud != null ? new[] { transform.Find("TrackViewport").GetComponent<FrustumFitAnchor>() } : openTransitionAnchors,
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

            // Canvas scaling can settle during the opening transition. Refit before generating
            // world-sized props so a later responsive refresh cannot magnify an early tiny layout.
            if (clubSportHud != null)
            {
                Canvas.ForceUpdateCanvases();
                clubSportHud.Layout.Refresh(true);
                ConfigureTrackViewport();
                transform.Find("TrackViewport").GetComponent<FrustumFitAnchor>().Apply();
                ApplyRaceBoardLayout();
            }

            // Generate props now that the track is at its final position.
            if (track != null)
            {
                track.RefreshThemeEffects();
                track.GenerateProps();
            }

            if (startRaceSound.IsValid())
            {
                BroAudio.Play(startRaceSound).OnEnd(_ =>
                {
                    if (this != null)
                    {
                        if (board != null)
                        {
                            board.Board.SetAllGearsRapidSpin(false);
                        }

                        if (viewModel != null)
                        {
                            StartRaceWithOpponents();
                        }
                    }
                });
            }
            else
            {
                if (board != null)
                {
                    board.Board.SetAllGearsRapidSpin(false);
                }

                StartRaceWithOpponents();
            }
        }

        protected override void OnClose(bool hiding)
        {
            base.OnClose(hiding);
            clubSportHud?.Unbind();
            ResetGearPresentation();
            if (!hiding)
            {
                raceStartPending = false;
                DestroySpawnedCars();
            }
            SetRaceSceneRootsActive(false);
            RestoreBoardLayout();
            RestoreRaceBackdrop();
        }

        protected override void OnUnbind()
        {
            clubSportHud?.Unbind();
            ResetGearPresentation();
            RestoreBoardLayout();
            raceStartPending = false;
            DestroySpawnedCars();
            if (track != null)
            {
                track.Unbind();
            }

            if (board != null)
            {
                board.Board.SetAllGearsRapidSpin(false);
                board.Unbind();
            }

            HideBoardCapacityChip();
            SetRaceSceneRootsActive(false);
            RestoreRaceBackdrop();
            base.OnUnbind();
        }

        private void ResetGearPresentation()
        {
            if (currentGearText == null) { return; }
            currentGearText.transform.DOKill();
            currentGearText.transform.localScale = Vector3.one;
            currentGearText.transform.localRotation = Quaternion.identity;
        }

        private void DestroySpawnedCars()
        {
            DestroyPositionMarkers();

            foreach (SplineEvaluateDriver driver in opponentDrivers)
            {
                opponentRunner?.RemoveDriver(driver.CarEntity);
            }
            opponentDrivers.Clear();
            opponentRunner = null;

            foreach (CarView car in spawnedCars)
            {
                if (car != null)
                {
                    car.gameObject.SetActive(false);
                    DestroyRuntimeObject(car.gameObject);
                }
            }

            spawnedCars.Clear();
        }

        private void DestroyPositionMarkers()
        {
            foreach (RacePositionMarker marker in positionMarkers)
            {
                DestroyRuntimeObject(marker.Root);
            }

            positionMarkers.Clear();
            rankedPositionMarkers.Clear();
            raceFinishPresentationReleased = false;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void SetRaceSceneRootsActive(bool active)
        {
            if (track != null)
            {
                track.gameObject.SetActive(active);
            }

            if (board != null)
            {
                board.SetVisible(active);
            }

            if (telemetry != null)
            {
                telemetry.gameObject.SetActive(active);
            }
        }

        private void HideBoardCapacityChip()
        {
            Transform boardTransform = board?.Board?.transform;
            if (boardTransform == null)
            {
                return;
            }

            for (int i = 0; i < boardTransform.childCount; i++)
            {
                Transform child = boardTransform.GetChild(i);
                if (child.name == k_boardCapacityChipName)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private sealed class RacePositionMarker
        {
            public RacePositionMarker(
                GameObject root, Transform target, SplineEvaluateDriver driver,
                TextMeshProUGUI label, Image leftArrowFill, Image rightArrowFill,
                int gridPosition, bool isPlayer)
            {
                Root = root;
                Target = target;
                Driver = driver;
                Label = label;
                LeftArrowFill = leftArrowFill;
                RightArrowFill = rightArrowFill;
                Arrow = leftArrowFill != null ? leftArrowFill.transform.parent as RectTransform : null;
                GridPosition = gridPosition;
                IsPlayer = isPlayer;
            }

            public GameObject Root { get; }
            public Transform Target { get; }
            public SplineEvaluateDriver Driver { get; }
            public TextMeshProUGUI Label { get; }
            public Image LeftArrowFill { get; }
            public Image RightArrowFill { get; }
            public RectTransform Arrow { get; }
            public Vector2 CarAnchoredPosition { get; set; }
            public int GridPosition { get; }
            public bool IsPlayer { get; }
            public int FinishedPosition { get; set; }
            public bool IsVisible { get; set; }
            public bool FinishTriggered { get; set; }
        }
    }
}
