using System.Threading.Tasks;
using Scaffold.AppFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.App.Bootstrap.Presentation
{
    /// <summary>
    /// Manages the loading screen UI during the application bootstrap phase.
    /// Listens to AppFlowRoot progress to update a slider and phase text.
    /// </summary>
    public sealed class BootstrapLoadingManager : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField]
        [Tooltip("The root object that orchestrates the AppFlow layers and reports progress.")]
        private AppFlowRoot appFlowRoot;

        [Header("UI Elements")]
        [SerializeField]
        [Tooltip("Slider used as the progress bar.")]
        private Slider progressBar;

        [SerializeField]
        [Tooltip("Text element to display the current loading phase/layer.")]
        private TextMeshProUGUI phaseText;

        [SerializeField]
        [Tooltip("Text element to display the loading percentage.")]
        private TextMeshProUGUI percentageText;

        [SerializeField]
        [Tooltip("Root visual element to hide/show the loading screen.")]
        private GameObject loadingScreenRoot;

        private void Awake()
        {
            if (appFlowRoot == null)
            {
                Debug.LogWarning("[BootstrapLoadingManager] AppFlowRoot is not assigned.", this);
                return;
            }

            // Show loading screen initially
            if (loadingScreenRoot != null)
            {
                loadingScreenRoot.SetActive(true);
            }

            // Apply initial progress
            ApplyProgress(appFlowRoot.Progress.Current);

            // Subscribe to progress changes
            appFlowRoot.Progress.Changed += OnProgressChanged;
        }

        private void OnDestroy()
        {
            if (appFlowRoot != null)
            {
                appFlowRoot.Progress.Changed -= OnProgressChanged;
            }
        }

        private void OnProgressChanged(AppFlowSession session)
        {
            ApplyProgress(session);

            if (session.IsComplete)
            {
                // Unsubscribe to avoid multiple triggers
                if (appFlowRoot != null)
                {
                    appFlowRoot.Progress.Changed -= OnProgressChanged;
                }
                
                _ = CompleteLoadingAsync(session.Outcome);
            }
        }

        private void ApplyProgress(AppFlowSession session)
        {
            float total = Mathf.Max(1, session.TotalLayers);
            float current = session.Current.HasValue ? session.Current.Value.SubProgress : 0f;
            float normalized = Mathf.Clamp01((session.CompletedLayers + current) / total);

            if (progressBar != null)
            {
                progressBar.value = normalized;
            }

            if (percentageText != null)
            {
                percentageText.text = $"{(normalized * 100f):0}%";
            }

            if (phaseText != null && session.Current.HasValue)
            {
                phaseText.text = $"Loading Phase:\n{session.Current.Value.LayerName}";
            }
        }

        private async Task CompleteLoadingAsync(AppFlowOutcome? outcome)
        {
            if (progressBar != null)
            {
                progressBar.value = 1f;
            }

            if (percentageText != null)
            {
                percentageText.text = "100%";
            }
            
            if (phaseText != null)
            {
                bool succeeded = outcome.HasValue && outcome.Value.Succeeded;
                phaseText.text = succeeded ? "Loading Complete" : "Loading Failed";
                Debug.Log($"[BootstrapLoadingManager] Startup outcome succeeded = {succeeded}");
            }

            // Small delay to ensure the user sees the 100% completion before hiding
            await Task.Delay(250);

            if (loadingScreenRoot != null)
            {
                loadingScreenRoot.SetActive(false);
            }
            gameObject.SetActive(false);
        }
    }
}
