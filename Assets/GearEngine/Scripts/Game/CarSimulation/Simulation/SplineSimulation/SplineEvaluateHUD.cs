using GearEngine.CarSimulation.SplineSimulation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GearEngine.CarSimulation.SplineSimulation
{
    /// <summary>
    /// Simple debug/test HUD for the spline-evaluate scene. Shows speed, lap,
    /// progress, and provides 5 sliders for real-time stat tuning plus
    /// start/pause control.
    /// </summary>
    public sealed class SplineEvaluateHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SplineEvaluateBootstrap bootstrap;

        [Header("Telemetry Labels")]
        [SerializeField] private TextMeshProUGUI speedLabel;

        [Header("Controls")]
        [SerializeField] private Button startStopButton;
        [SerializeField] private TextMeshProUGUI startStopLabel;

        private bool isRunning;

        private void Start()
        {

            if (startStopButton != null)
            {
                startStopButton.onClick.AddListener(ToggleRace);
            }
        }

        private void Update()
        {
            if (bootstrap == null || bootstrap.ActiveDriver == null) return;

            SplineMotionState state = bootstrap.ActiveDriver.State;

            if (speedLabel != null)
            {
                speedLabel.text = $"{state.Speed * 3.6f:F0} km/h";
            }
        }

        private void ToggleRace()
        {
            if (bootstrap == null) return;

            isRunning = !isRunning;
            if (isRunning)
            {
                bootstrap.StartRace();
            }
            else
            {
                bootstrap.PauseRace();
            }

            if (startStopLabel != null)
            {
                startStopLabel.text = isRunning ? "Pause" : "Start";
            }
        }



        private void OnDestroy()
        {
            if (startStopButton != null) startStopButton.onClick.RemoveListener(ToggleRace);
        }
    }
}
