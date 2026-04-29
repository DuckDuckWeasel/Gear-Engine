using GearEngine.SplineEvaluate.Bootstrap;
using GearEngine.SplineEvaluate.Definitions;
using GearEngine.SplineEvaluate.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeField] private TextMeshProUGUI curveModeLabel;

        [Header("Stat Sliders")]
        [FormerlySerializedAs("topSpeedSlider")]
        [SerializeField] private Slider speedCapabilitySlider;

        [FormerlySerializedAs("perfectCurveChanceSlider")]
        [SerializeField] private Slider corneringSkillSlider;

        [FormerlySerializedAs("tractionSlider")]
        [SerializeField] private Slider driftSlider;

        [FormerlySerializedAs("curveOffsetSlider")]
        [SerializeField] private Slider precisionSlider;

        [FormerlySerializedAs("recklessnessSlider")]
        [SerializeField] private Slider smoothnessSlider;

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

            if (curveModeLabel != null)
            {
                if (state.IsInCurveSequence)
                {
                    curveModeLabel.text = $"Curve Mode: {state.ActiveCurveMode}";
                }
                else
                {
                    curveModeLabel.text = "Curve Mode: None";
                }
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
            SetupSlider(speedCapabilitySlider, 5f, 0f, 10f);
            SetupSlider(corneringSkillSlider, 5f, 0f, 10f);
            SetupSlider(driftSlider, 5f, 0f, 10f);
            SetupSlider(precisionSlider, 5f, 0f, 10f);
            SetupSlider(smoothnessSlider, 5f, 0f, 10f);

            if (speedCapabilitySlider != null) speedCapabilitySlider.onValueChanged.AddListener(_ => PushPersonality());
            if (corneringSkillSlider != null) corneringSkillSlider.onValueChanged.AddListener(_ => PushPersonality());
            if (driftSlider != null) driftSlider.onValueChanged.AddListener(_ => PushPersonality());
            if (precisionSlider != null) precisionSlider.onValueChanged.AddListener(_ => PushPersonality());
            if (smoothnessSlider != null) smoothnessSlider.onValueChanged.AddListener(_ => PushPersonality());
        }

        private static void SetupSlider(Slider slider, float defaultValue, float min, float max)
        {
            if (slider == null) return;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = defaultValue;
        }

        private void PushPersonality()
        {
            if (bootstrap == null) return;

            var p = new DriverPersonality
            {
                SpeedCapability = speedCapabilitySlider != null ? speedCapabilitySlider.value : 5f,
                CorneringSkill = corneringSkillSlider != null ? corneringSkillSlider.value : 5f,
                Drift = driftSlider != null ? driftSlider.value : 5f,
                Precision = precisionSlider != null ? precisionSlider.value : 5f,
                Smoothness = smoothnessSlider != null ? smoothnessSlider.value : 5f
            };

            bootstrap.UpdatePersonality(p);
        }

        private void OnDestroy()
        {
            if (startStopButton != null) startStopButton.onClick.RemoveListener(ToggleRace);
            if (speedCapabilitySlider != null) speedCapabilitySlider.onValueChanged.RemoveAllListeners();
            if (corneringSkillSlider != null) corneringSkillSlider.onValueChanged.RemoveAllListeners();
            if (driftSlider != null) driftSlider.onValueChanged.RemoveAllListeners();
            if (precisionSlider != null) precisionSlider.onValueChanged.RemoveAllListeners();
            if (smoothnessSlider != null) smoothnessSlider.onValueChanged.RemoveAllListeners();
        }
    }
}
