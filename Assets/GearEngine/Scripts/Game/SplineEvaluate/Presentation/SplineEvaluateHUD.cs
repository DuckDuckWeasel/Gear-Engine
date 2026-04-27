using GearEngine.SplineEvaluate.Bootstrap;
using GearEngine.SplineEvaluate.Definitions;
using GearEngine.SplineEvaluate.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.SplineEvaluate.Presentation
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
        [SerializeField] private TextMeshProUGUI lapLabel;
        [SerializeField] private TextMeshProUGUI stateLabel;

        [Header("Stat Sliders")]
        [SerializeField] private Slider aggressionSlider;
        [SerializeField] private Slider driftTendencySlider;
        [SerializeField] private Slider lineWidthSlider;
        [SerializeField] private Slider consistencySlider;
        [SerializeField] private Slider riskSlider;

        [Header("Controls")]
        [SerializeField] private Button startStopButton;
        [SerializeField] private TextMeshProUGUI startStopLabel;

        private bool isRunning;

        private void Start()
        {
            SetupSliders();

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

            if (lapLabel != null)
            {
                lapLabel.text = $"Lap {state.CompletedLaps + 1} | {state.T * 100f:F1}%";
            }

            if (stateLabel != null)
            {
                string mode = state.IsDrifting ? "DRIFT" : state.IsBraking ? "BRAKE" : state.IsAccelerating ? "ACCEL" : "COAST";
                stateLabel.text = mode;
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

        private void SetupSliders()
        {
            SetupSlider(aggressionSlider, 5f);
            SetupSlider(driftTendencySlider, 5f);
            SetupSlider(lineWidthSlider, 5f);
            SetupSlider(consistencySlider, 5f);
            SetupSlider(riskSlider, 5f);

            if (aggressionSlider != null) aggressionSlider.onValueChanged.AddListener(_ => PushPersonality());
            if (driftTendencySlider != null) driftTendencySlider.onValueChanged.AddListener(_ => PushPersonality());
            if (lineWidthSlider != null) lineWidthSlider.onValueChanged.AddListener(_ => PushPersonality());
            if (consistencySlider != null) consistencySlider.onValueChanged.AddListener(_ => PushPersonality());
            if (riskSlider != null) riskSlider.onValueChanged.AddListener(_ => PushPersonality());
        }

        private static void SetupSlider(Slider slider, float defaultValue)
        {
            if (slider == null) return;
            slider.minValue = 0f;
            slider.maxValue = 10f;
            slider.value = defaultValue;
        }

        private void PushPersonality()
        {
            if (bootstrap == null) return;

            var p = new DriverPersonality
            {
                Aggression = aggressionSlider != null ? aggressionSlider.value : 5f,
                DriftTendency = driftTendencySlider != null ? driftTendencySlider.value : 5f,
                LineWidth = lineWidthSlider != null ? lineWidthSlider.value : 5f,
                Consistency = consistencySlider != null ? consistencySlider.value : 5f,
                Risk = riskSlider != null ? riskSlider.value : 5f
            };

            bootstrap.UpdatePersonality(p);
        }

        private void OnDestroy()
        {
            if (startStopButton != null) startStopButton.onClick.RemoveListener(ToggleRace);
            if (aggressionSlider != null) aggressionSlider.onValueChanged.RemoveAllListeners();
            if (driftTendencySlider != null) driftTendencySlider.onValueChanged.RemoveAllListeners();
            if (lineWidthSlider != null) lineWidthSlider.onValueChanged.RemoveAllListeners();
            if (consistencySlider != null) consistencySlider.onValueChanged.RemoveAllListeners();
            if (riskSlider != null) riskSlider.onValueChanged.RemoveAllListeners();
        }
    }
}
