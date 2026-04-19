using System.Collections.Generic;
using System.Text;
using GearEngine.CarSimulation.Simulation;
using TMPro;
using UnityEngine;

namespace GearEngine.CarSimulation.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TrackTelemetryViewComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI isBrakingText;
        [SerializeField] private TextMeshProUGUI isDriftingText;
        [SerializeField] private TextMeshProUGUI isAcceleratingText;
        [SerializeField] private TextMeshProUGUI lapsText;
        [SerializeField] private TextMeshProUGUI accelerationText;
        [SerializeField] private TextMeshProUGUI timesText;

        public void UpdateTelemetry(float speed, float progress, bool isBraking, bool isDrifting, bool isAccelerating, int currentLap, int maxLaps, float currentAcceleration, float raceTime, IReadOnlyList<float> lapTimes)
        {
            UpdatePrimaryTelemetry(speed, progress, currentLap, maxLaps, currentAcceleration);
            UpdateStateLabels(isBraking, isDrifting, isAccelerating);
            UpdateTimingTelemetry(raceTime, lapTimes);
        }

        public void UpdateFrom(CarViewModel carVm)
        {
            if (carVm == null)
            {
                return;
            }

            RaceState session = carVm.Session;
            UpdateTelemetry(
                carVm.Speed,
                carVm.Progress,
                carVm.IsBraking,
                carVm.IsDrifting,
                carVm.IsAccelerating,
                session.CurrentLap,
                session.TotalLaps,
                carVm.CurrentAcceleration,
                session.RaceTime,
                session.LapTimes);
        }

        private void UpdatePrimaryTelemetry(float speed, float progress, int currentLap, int maxLaps, float currentAcceleration)
        {
            SetText(speedText, $"Speed: {Mathf.RoundToInt(speed)} km/h");
            SetText(progressText, $"Progress: {(progress * 100f):F1}%");
            SetText(lapsText, $"Lap: {currentLap} / {maxLaps}");
            SetText(accelerationText, $"Accel Ratio: {currentAcceleration:F2}");
        }

        private void UpdateStateLabels(bool isBraking, bool isDrifting, bool isAccelerating)
        {
            SetStateLabel(isBrakingText, isBraking, "BRAKING : ON", "BRAKING : OFF", Color.red, Color.gray);
            SetStateLabel(isDriftingText, isDrifting, "DRIFTING : ON", "DRIFTING : OFF", new Color(1f, 0.5f, 0f), Color.gray);
            SetStateLabel(isAcceleratingText, isAccelerating, "ACCEL : ON", "ACCEL : OFF", Color.green, Color.gray);
        }

        private void UpdateTimingTelemetry(float raceTime, IReadOnlyList<float> lapTimes)
        {
            if (timesText == null)
            {
                return;
            }

            timesText.text = BuildTimingText(raceTime, lapTimes);
        }

        private void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private void SetStateLabel(TMP_Text label, bool isActive, string activeText, string inactiveText, Color activeColor, Color inactiveColor)
        {
            if (label == null)
            {
                return;
            }

            label.text = isActive ? activeText : inactiveText;
            label.color = isActive ? activeColor : inactiveColor;
        }

        private static string BuildTimingText(float raceTime, IReadOnlyList<float> lapTimes)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Race Time: {raceTime:F2}s");

            if (lapTimes == null)
            {
                return builder.ToString();
            }

            for (int i = 0; i < lapTimes.Count; i++)
            {
                builder.AppendLine($"Lap {i + 1}: {lapTimes[i]:F2}s");
            }

            return builder.ToString();
        }
    }
}
