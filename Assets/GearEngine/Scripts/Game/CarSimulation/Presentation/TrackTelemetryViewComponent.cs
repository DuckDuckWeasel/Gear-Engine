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

        public void UpdateTelemetry(float speed, float progress, bool isBraking, bool isDrifting, bool isAccelerating, int currentLap, int maxLaps, float currentAcceleration, float raceTime, IReadOnlyList<float> lapTimes)
        {
            UpdatePrimaryTelemetry(speed);
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

        private void UpdatePrimaryTelemetry(float speed)
        {
            SetText(speedText, $"Speed: {Mathf.RoundToInt(speed)} km/h");
        }

        private void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }
    }
}
