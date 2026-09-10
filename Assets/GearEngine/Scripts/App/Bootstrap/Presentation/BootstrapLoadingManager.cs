using System;
using System.Collections;
using System.Runtime.InteropServices;
using Scaffold.AppFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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

        [Header("Loading Media")]
        [SerializeField]
        [Tooltip("Raw image that displays either the loading video or the fallback texture.")]
        private RawImage loadingVisual;

        [SerializeField]
        [Tooltip("Video player used for the loading-screen loop.")]
        private VideoPlayer loadingVideoPlayer;

        [SerializeField]
        [Tooltip("Image displayed until the first video frame arrives or whenever video playback fails.")]
        private Texture fallbackTexture;

        [SerializeField]
        [Tooltip("Video file copied to StreamingAssets for URL-based WebGL playback.")]
        private string loadingVideoFileName = "GearEngineLoadingLoop.mp4";

        [SerializeField]
        [Min(0.1f)]
        [Tooltip("Maximum time to wait for the first video frame before keeping the fallback image.")]
        private float videoStartupTimeoutSeconds = 4f;

        private Coroutine videoStartupTimeout;
        private bool hasVideoFrame;
        private bool isStoppingLoadingVideo;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void GearEngineLoadingReady();
#endif

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

            InitializeLoadingMedia();

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

            ReleaseLoadingMedia();
        }

        private void OnDisable()
        {
            ReleaseLoadingMedia();
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

                StartCoroutine(CompleteLoading(session.Outcome));
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

        private void InitializeLoadingMedia()
        {
            ShowFallback();
            if (loadingVideoPlayer == null)
            {
                Debug.LogWarning("[BootstrapLoadingManager] Loading VideoPlayer is not assigned. Using the fallback image.", this);
                return;
            }

            SubscribeToVideoEvents();
            try
            {
                ConfigureLoadingVideo();
                loadingVideoPlayer.Prepare();
                videoStartupTimeout = StartCoroutine(WaitForVideoStartup());
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BootstrapLoadingManager] Failed to prepare the loading video. {exception}", this);
                StopLoadingVideo();
            }
        }

        private void ConfigureLoadingVideo()
        {
            if (string.IsNullOrWhiteSpace(loadingVideoFileName))
            {
                throw new InvalidOperationException("Loading video file name cannot be empty.");
            }

            loadingVideoPlayer.source = VideoSource.Url;
            loadingVideoPlayer.url = BuildLoadingVideoUrl();
            loadingVideoPlayer.playOnAwake = false;
            loadingVideoPlayer.isLooping = true;
            loadingVideoPlayer.waitForFirstFrame = true;
            loadingVideoPlayer.skipOnDrop = true;
            loadingVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            loadingVideoPlayer.sendFrameReadyEvents = true;
        }

        private string BuildLoadingVideoUrl()
        {
            string basePath = Application.streamingAssetsPath.TrimEnd('/', '\\');
            string path = $"{basePath}/{loadingVideoFileName}";
            return Uri.TryCreate(path, UriKind.Absolute, out Uri uri) ? uri.AbsoluteUri : path;
        }

        private IEnumerator WaitForVideoStartup()
        {
            yield return new WaitForSecondsRealtime(videoStartupTimeoutSeconds);
            videoStartupTimeout = null;
            if (hasVideoFrame)
            {
                yield break;
            }

            Debug.LogWarning($"[BootstrapLoadingManager] Loading video produced no frame after {videoStartupTimeoutSeconds:0.0}s. Using the fallback image.", this);
            StopLoadingVideo();
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            try
            {
                source.Play();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BootstrapLoadingManager] Failed to start the loading video. {exception}", this);
                StopLoadingVideo();
            }
        }

        private void OnVideoFrameReady(VideoPlayer source, long frameIndex)
        {
            if (hasVideoFrame)
            {
                return;
            }

            Texture videoTexture = source.targetTexture != null ? source.targetTexture : source.texture;
            if (loadingVisual == null || videoTexture == null)
            {
                return;
            }

            hasVideoFrame = true;
            loadingVisual.texture = videoTexture;
            CancelVideoStartupTimeout();
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogError($"[BootstrapLoadingManager] Loading video failed: {message}", this);
            StopLoadingVideo();
        }

        private void ShowFallback()
        {
            hasVideoFrame = false;
            if (loadingVisual != null && fallbackTexture != null)
            {
                loadingVisual.texture = fallbackTexture;
            }
        }

        private void StopLoadingVideo()
        {
            if (isStoppingLoadingVideo)
            {
                return;
            }

            isStoppingLoadingVideo = true;
            try
            {
                ShowFallback();
                CancelVideoStartupTimeout();
                if (loadingVideoPlayer != null)
                {
                    loadingVideoPlayer.Stop();
                }
            }
            finally
            {
                isStoppingLoadingVideo = false;
            }
        }

        private void CancelVideoStartupTimeout()
        {
            if (videoStartupTimeout == null)
            {
                return;
            }

            StopCoroutine(videoStartupTimeout);
            videoStartupTimeout = null;
        }

        private void SubscribeToVideoEvents()
        {
            loadingVideoPlayer.prepareCompleted += OnVideoPrepared;
            loadingVideoPlayer.frameReady += OnVideoFrameReady;
            loadingVideoPlayer.errorReceived += OnVideoError;
        }

        private void ReleaseLoadingMedia()
        {
            CancelVideoStartupTimeout();
            if (loadingVideoPlayer == null)
            {
                return;
            }

            loadingVideoPlayer.prepareCompleted -= OnVideoPrepared;
            loadingVideoPlayer.frameReady -= OnVideoFrameReady;
            loadingVideoPlayer.errorReceived -= OnVideoError;
            loadingVideoPlayer.Stop();
        }

        private IEnumerator CompleteLoading(AppFlowOutcome? outcome)
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

            // Use Unity's frame-driven delay so the loading screen also completes on single-threaded WebGL.
            yield return new WaitForSecondsRealtime(0.25f);

            NotifyBrowserLoadingComplete();

            if (loadingScreenRoot != null)
            {
                loadingScreenRoot.SetActive(false);
            }

            Debug.Log("[BootstrapLoadingManager] Loading screen hidden.");
            gameObject.SetActive(false);
        }

        private static void NotifyBrowserLoadingComplete()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            GearEngineLoadingReady();
#endif
        }
    }
}
