
#if UNITY_5_3_OR_NEWER

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Scaffold
{
    /// <summary>
    /// A singleton game object which displays a simple UI for the Narrative Log.
    /// </summary>
    public class NarrativeLogMenu : MonoBehaviour
    {
        [SerializeField] private NarrativeLog narrativeLog;

        [Tooltip("Contains the overall aesthetic of each entry.")]
        [SerializeField] protected NarrativeLogEntryDisplay entryDisplayPrefab;

        [Tooltip("Show the Narrative Log Menu")]
        [SerializeField] protected bool showLog = true;

        [Tooltip("Show previous lines instead of previous and current")]
        [SerializeField] protected bool previousLines = true;

        [Tooltip("A scrollable text field used for displaying conversation history.")]
        [SerializeField] protected ScrollRect narrativeLogView;

        [Tooltip("Limit characters to be shown in Narrative Log")]
        [SerializeField] protected int maxCharacters = 10000;

        protected TextAdapter narLogViewtextAdapter = new TextAdapter();

        [Tooltip("The CanvasGroup containing the save menu buttons")]
        [SerializeField] protected CanvasGroup narrativeLogMenuGroup;

        protected static bool s_narrativeLogActive = false;

        protected AudioSource clickAudioSource;

        protected LTDescr fadeTween;

        protected static NarrativeLogMenu s_instance;

        protected virtual void Awake()
        {
            if (showLog)
            {
                // Only one instance of NarrativeLogMenu may exist
                if (s_instance != null)
                {
                    Destroy(gameObject);
                    return;
                }

                s_instance = this;

                GameObject.DontDestroyOnLoad(this);

                clickAudioSource = GetComponent<AudioSource>();
            }
            else
            {
                GameObject logView = GameObject.Find("NarrativeLogView");
                logView.SetActive(false);
                this.enabled = false;
            }

            narLogViewtextAdapter.InitFromGameObject(narrativeLogView.gameObject, true);
        }

        protected virtual void Start()
        {
            if (!s_narrativeLogActive)
            {
                narrativeLogMenuGroup.alpha = 0f;
            }

            //Clear up the lorem ipsum
            UpdateNarrativeLogText();
        }

        protected virtual void OnEnable()
        {
            WriterSignals.OnWriterState += OnWriterState;
            NarrativeLog.OnNarrativeAdded += OnNarrativeAdded;
        }

        protected virtual void OnDisable()
        {
            WriterSignals.OnWriterState -= OnWriterState;
            NarrativeLog.OnNarrativeAdded -= OnNarrativeAdded;
        }

        protected virtual void OnNarrativeAdded(NarrativeLogEntry data)
        {
            UpdateNarrativeLogText();
        }

        protected virtual void OnWriterState(Writer writer, WriterState writerState)
        {
            if (writerState == WriterState.Start)
            {
                UpdateNarrativeLogText();
            }
        }

        protected void UpdateNarrativeLogText()
        {
            if (narrativeLogView.enabled && narrativeLog != null)
            {
                string prettyHistory = narrativeLog.GetPrettyHistory();

                if (prettyHistory.Length > maxCharacters)
                {
                    prettyHistory = "... " + prettyHistory.Substring(prettyHistory.Length - maxCharacters, maxCharacters);
                }
                narLogViewtextAdapter.Text = prettyHistory;

                Canvas.ForceUpdateCanvases();
                narrativeLogView.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
            }
        }

        protected void PlayClickSound()
        {
            if (clickAudioSource != null)
            {
                clickAudioSource.Play();
            }
        }

        #region Public methods

        public virtual void ToggleNarrativeLogView()
        {
            if (fadeTween != null)
            {
                LeanTween.cancel(fadeTween.id, true);
                fadeTween = null;
            }

            if (s_narrativeLogActive)
            {
                // Switch menu off
                LeanTween.value(narrativeLogMenuGroup.gameObject, narrativeLogMenuGroup.alpha, 0f, .2f)
                    .setEase(LeanTweenType.easeOutQuint)
                    .setOnUpdate((t) =>
                    {
                        narrativeLogMenuGroup.alpha = t;
                    }).setOnComplete(() =>
                    {
                        narrativeLogMenuGroup.alpha = 0f;
                    });

            }
            else
            {
                // Switch menu on
                LeanTween.value(narrativeLogMenuGroup.gameObject, narrativeLogMenuGroup.alpha, 1f, .2f)
                    .setEase(LeanTweenType.easeOutQuint)
                    .setOnUpdate((t) =>
                    {
                        narrativeLogMenuGroup.alpha = t;
                    }).setOnComplete(() =>
                    {
                        narrativeLogMenuGroup.alpha = 1f;
                    });

            }

            s_narrativeLogActive = !s_narrativeLogActive;
        }

        #endregion
    }
}

#endif
