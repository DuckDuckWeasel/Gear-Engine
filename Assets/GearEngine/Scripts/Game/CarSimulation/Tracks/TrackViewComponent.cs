using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tracks
{
    [DisallowMultipleComponent]
    public sealed class TrackViewComponent : ViewComponent<TrackViewModel>
    {
        private const string pathChildName = "Path";

        public SplineContainer SplineContainer => splineContainer;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private SplineExtrude splineExtrude;

        [Header("Telemetry UI")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI isBrakingText;
        [SerializeField] private TextMeshProUGUI isDriftingText;
        [SerializeField] private TextMeshProUGUI isAcceleratingText;
        [SerializeField] private TextMeshProUGUI lapsText;
        [SerializeField] private TextMeshProUGUI accelerationText;
        [SerializeField] private TextMeshProUGUI timesText;

        public new void Unbind()
        {
            base.Unbind();
        }

        public void UpdateTelemetryUI(float speed, float progress, bool isBraking, bool isDrifting, bool isAccelerating, int currentLap, int maxLaps, float currentAcceleration, float raceTime, System.Collections.Generic.IReadOnlyList<float> lapTimes)
        {
            if (speedText != null)
                speedText.text = $"Speed: {Mathf.RoundToInt(speed)} km/h";
            
            if (progressText != null)
                progressText.text = $"Progress: {(progress * 100f):F1}%";

            if (isBrakingText != null)
            {
                isBrakingText.text = isBraking ? "BRAKING : ON" : "BRAKING : OFF";
                isBrakingText.color = isBraking ? Color.red : Color.gray;
            }

            if (isDriftingText != null)
            {
                isDriftingText.text = isDrifting ? "DRIFTING : ON" : "DRIFTING : OFF";
                isDriftingText.color = isDrifting ? new Color(1f, 0.5f, 0f) : Color.gray;
            }

            if (isAcceleratingText != null)
            {
                isAcceleratingText.text = isAccelerating ? "ACCEL : ON" : "ACCEL : OFF";
                isAcceleratingText.color = isAccelerating ? Color.green : Color.gray;
            }

            if (lapsText != null)
            {
                lapsText.text = $"Lap: {currentLap} / {maxLaps}";
            }

            if (accelerationText != null)
            {
                accelerationText.text = $"Accel Ratio: {currentAcceleration:F2}";
            }

            if (timesText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"Race Time: {raceTime:F2}s");
                if (lapTimes != null && lapTimes.Count > 0)
                {
                    for (int i = 0; i < lapTimes.Count; i++)
                    {
                        sb.AppendLine($"Lap {i + 1}: {lapTimes[i]:F2}s");
                    }
                }
                timesText.text = sb.ToString();
            }
        }

        private void Awake()
        {
            EnsureSplineContainerReference();
            EnsureSplineExtrudeReference();
        }

        protected override void OnBind()
        {
            if (viewModel == null)
            {
                return;
            }

            InitializeTrack(viewModel.Track);
        }

        protected override void OnUnbind()
        {
            viewModel?.TearDown();
            base.OnUnbind();
        }

        private void InitializeTrack(TrackDefinition data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            ExecuteInitialize(data);
        }

        private void ExecuteInitialize(TrackDefinition data)
        {
            EnsureSplineContainerReference();
            EnsureSplineExtrudeReference();
            if (!HasSplineContainerOrLog() || !HasSplineDataOrLog(data))
            {
                return;
            }

            CopySplineIntoContainer(data.Spline, splineContainer);
            RebuildVisualSplineExtrude(data.Spline);
        }

        private bool HasSplineContainerOrLog()
        {
            if (splineContainer != null)
            {
                return true;
            }

            LogSplineContainerMissing();
            return false;
        }

        private bool HasSplineDataOrLog(TrackDefinition data)
        {
            if (data.Spline.Count > 0)
            {
                return true;
            }

            LogEmptyTrackDefinition(data.name);
            return false;
        }

        private void EnsureSplineContainerReference()
        {
            if (splineContainer == null)
            {
                splineContainer = GetComponent<SplineContainer>();
            }
        }

        private void EnsureSplineExtrudeReference()
        {
            if (splineExtrude != null)
            {
                return;
            }

            Transform path = transform.Find(pathChildName);
            if (path != null)
            {
                splineExtrude = path.GetComponent<SplineExtrude>();
            }

            if (splineExtrude == null)
            {
                splineExtrude = GetComponent<SplineExtrude>();
            }
        }

        private void LogSplineContainerMissing()
        {
            Debug.LogError("[Track] SplineContainer is missing; cannot Initialize.");
        }

        private void LogEmptyTrackDefinition(string definitionName)
        {
            Debug.LogError($"[Track] TrackDefinition '{definitionName}' has no spline knots.");
        }

        private void RebuildVisualSplineExtrude(Spline source)
        {
            if (splineExtrude == null)
            {
                return;
            }

            SyncVisualContainer(source);
            splineExtrude.Rebuild();
        }

        private void SyncVisualContainer(Spline source)
        {
            SplineContainer visualContainer = splineExtrude.Container;
            if (visualContainer == null)
            {
                splineExtrude.Container = splineContainer;
                return;
            }

            if (visualContainer != splineContainer)
            {
                CopySplineIntoContainer(source, visualContainer);
            }
        }

        private void CopySplineIntoContainer(Spline source, SplineContainer targetContainer)
        {
            Spline target = targetContainer.Spline;
            target.Knots = source.Knots;
            target.Closed = source.Closed;
        }
    }
}
