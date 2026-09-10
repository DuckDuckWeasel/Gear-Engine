using System.Collections;
using System.Collections.Generic;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GearEngine.Presentation.UI.Effects
{
    /// <summary>
    /// Drives the sample transition gallery and its companion destination scene.
    /// </summary>
    [AddComponentMenu("Gear/Samples/UI Transition Demo Controller")]
    [DisallowMultipleComponent]
    public sealed class UITransitionDemoController : MonoBehaviour
    {
        private const float k_rateEpsilon = 0.0001f;

        [SerializeField] private UIEffect transitionEffect;
        [SerializeField] private UIEffectTweener transitionTweener;
        [SerializeField] private Text presetLabel;
        [SerializeField] private Text sceneLabel;
        [SerializeField] private List<Button> controls = new List<Button>();
        [SerializeField] private List<UIEffectPreset> presets = new List<UIEffectPreset>();
        [SerializeField] private string destinationSceneName;
        [SerializeField] private bool revealOnStart;
        [SerializeField, Min(0.05f)] private float transitionDuration = 0.75f;
        [SerializeField, Min(0f)] private float coveredHoldDuration = 0.2f;

        private static int s_selectedPresetIndex;
        private bool isBusy;

        private void Awake()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            ConfigureTweener();
            ApplyCurrentPreset();
            PrepareInitialState();
        }

        private void Start()
        {
            if (revealOnStart)
            {
                StartCoroutine(RevealOnStartRoutine());
            }
        }

        public void PreviousPreset()
        {
            if (isBusy)
            {
                return;
            }

            s_selectedPresetIndex = WrapIndex(s_selectedPresetIndex - 1);
            ApplyCurrentPreset();
            PlayPreview();
        }

        public void NextPreset()
        {
            if (isBusy)
            {
                return;
            }

            s_selectedPresetIndex = WrapIndex(s_selectedPresetIndex + 1);
            ApplyCurrentPreset();
            PlayPreview();
        }

        public void PlayPreview()
        {
            if (!isBusy)
            {
                StartCoroutine(PreviewRoutine());
            }
        }

        public void LoadDestination()
        {
            if (!isBusy)
            {
                StartCoroutine(LoadDestinationRoutine());
            }
        }

        private IEnumerator PreviewRoutine()
        {
            BeginInteraction();
            yield return PlayPhase(false);
            yield return new WaitForSecondsRealtime(coveredHoldDuration);
            yield return PlayPhase(true);
            EndInteraction();
        }

        private IEnumerator LoadDestinationRoutine()
        {
            BeginInteraction();
            yield return PlayPhase(false);
            TryLoadDestination();
        }

        private IEnumerator RevealOnStartRoutine()
        {
            BeginInteraction();
            yield return PlayPhase(true);
            EndInteraction();
        }

        private IEnumerator PlayPhase(bool reveal)
        {
            if (reveal)
            {
                transitionTweener.PlayForward(true);
            }
            else
            {
                transitionTweener.PlayReverse(true);
            }

            yield return new WaitForSecondsRealtime(transitionDuration);
        }

        private bool ValidateConfiguration()
        {
            bool valid = transitionEffect != null &&
                transitionTweener != null &&
                presetLabel != null &&
                sceneLabel != null &&
                presets.Count > 0;
            if (!valid)
            {
                Debug.LogError($"[{nameof(UITransitionDemoController)}] '{name}' has incomplete demo references.", this);
            }

            return valid;
        }

        private void ConfigureTweener()
        {
            transitionTweener.duration = transitionDuration;
            transitionTweener.delay = 0f;
            transitionTweener.interval = 0f;
            transitionTweener.wrapMode = UIEffectTweener.WrapMode.Once;
            transitionTweener.updateMode = UIEffectTweener.UpdateMode.Unscaled;
            transitionTweener.playOnEnable = UIEffectTweener.PlayOnEnable.None;
            transitionTweener.cullingMask = UIEffectTweener.CullingMask.Transition;
        }

        private void ApplyCurrentPreset()
        {
            s_selectedPresetIndex = WrapIndex(s_selectedPresetIndex);
            UIEffectPreset preset = presets[s_selectedPresetIndex];
            transitionEffect.LoadPreset(preset, false);
            presetLabel.text = preset.name;
            sceneLabel.text = SceneManager.GetActiveScene().name;
            PrepareRevealedState();
        }

        private void PrepareInitialState()
        {
            if (revealOnStart)
            {
                transitionTweener.SetTime(0f);
            }
            else
            {
                PrepareRevealedState();
            }
        }

        private void PrepareRevealedState()
        {
            float revealedTime = transitionTweener.totalTime - k_rateEpsilon;
            transitionTweener.SetTime(Mathf.Max(0f, revealedTime));
        }

        private void BeginInteraction()
        {
            isBusy = true;
            SetControlsInteractable(false);
        }

        private void EndInteraction()
        {
            isBusy = false;
            SetControlsInteractable(true);
        }

        private void SetControlsInteractable(bool value)
        {
            foreach (Button control in controls)
            {
                if (control != null)
                {
                    control.interactable = value;
                }
            }
        }

        private int WrapIndex(int index)
        {
            return (index % presets.Count + presets.Count) % presets.Count;
        }

        private void TryLoadDestination()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(destinationSceneName))
                {
                    throw new System.InvalidOperationException("Destination scene name is empty.");
                }

                SceneManager.LoadScene(destinationSceneName);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[{nameof(UITransitionDemoController)}] Failed to load '{destinationSceneName}': {exception}", this);
                EndInteraction();
            }
        }
    }
}
