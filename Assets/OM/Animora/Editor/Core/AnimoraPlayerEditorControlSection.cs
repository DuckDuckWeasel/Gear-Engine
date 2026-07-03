using OM.Editor;
using OM.Animora.Runtime;
using OM.TimelineCreator.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OM.Animora.Editor
{
    /// <summary>
    /// Handles the playback control logic (Play, Pause, Stop, Replay) for the Animora timeline inside the Unity Editor.
    /// Connects UI buttons with <see cref="AnimoraPlayer"/> play state logic.
    /// </summary>
    public class AnimoraPlayerEditorControlSection
    {
        private readonly AnimoraPlayerEditor AnimoraPlayerEditor;
        private readonly AnimoraPlayer AnimoraPlayer;
        private readonly OM_HeaderButton _playButton, _stopButton, _replayButton;

        private double _lastEditorTime;
        private bool _isPlayingInEditor;
        private bool _previewLoopEnabled = false;
        private OM_PlayState _editorPlayState = OM_PlayState.Stopped;

        /// <summary>
        /// Initializes the control section and binds UI controls to the player's state.
        /// </summary>
        /// <param name="playerEditor">The parent <see cref="AnimoraPlayerEditor"/> instance.</param>
        public AnimoraPlayerEditorControlSection(AnimoraPlayerEditor playerEditor)
        {
            AnimoraPlayerEditor = playerEditor;
            AnimoraPlayer = playerEditor.Player;

            _replayButton = playerEditor.AnimoraTimeline.Header.ReplayButton;
            _playButton = playerEditor.AnimoraTimeline.Header.PlayButton;
            _stopButton = playerEditor.AnimoraTimeline.Header.StopButton;

            playerEditor.AnimoraTimeline.Header.OnReplayButtonClicked += TogglePreviewLoop;
            playerEditor.AnimoraTimeline.Header.OnPlayButtonClicked += OnPlayButtonClicked;
            playerEditor.AnimoraTimeline.Header.OnStopButtonClicked += Stop;

            if (!Application.isPlaying)
            {
                AnimoraPlayer.SetPlayState(OM_PlayState.Stopped);
            }

            AnimoraPlayer.OnPlayStateChanged += OnPlayStateChanged;
            playerEditor.AnimoraTimeline.OnPreviewStateChangedCallback += OnPreviewStateChanged;
            OnPlayStateChanged(Application.isPlaying ? AnimoraPlayer.PlayState : _editorPlayState);
        }

        private void OnPreviewStateChanged(bool isPreviewing)
        {
            if (_editorPlayState == OM_PlayState.Stopped)
            {
                _stopButton.SetEnabled(isPreviewing);
            }
        }

        /// <summary>
        /// Enables playmode state listeners and sets initial UI state.
        /// </summary>
        public void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnApplicationPlayModeStateChanged;
            EditorApplication.update += EditorUpdate;
            
            _playButton.SetEnabled(true);
            _stopButton.SetEnabled(true);
            _replayButton.SetEnabled(true);
        }

        /// <summary>
        /// Cleans up playmode state listeners.
        /// </summary>
        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnApplicationPlayModeStateChanged;
            EditorApplication.update -= EditorUpdate;
            if (AnimoraPlayerEditor != null && AnimoraPlayerEditor.AnimoraTimeline != null)
            {
                AnimoraPlayerEditor.AnimoraTimeline.OnPreviewStateChangedCallback -= OnPreviewStateChanged;
            }
        }

        private void EditorUpdate()
        {
            if (Application.isPlaying || !_isPlayingInEditor || AnimoraPlayer == null) return;
            
            if (!AnimoraPlayerEditor.AnimoraTimeline.IsPreviewing)
            {
                AnimoraPlayerEditor.AnimoraTimeline.StartPreview();
            }

            float deltaTime = (float)(EditorApplication.timeSinceStartup - _lastEditorTime);
            _lastEditorTime = EditorApplication.timeSinceStartup;
            
            float playbackSpeed = AnimoraPlayerEditor.serializedObject.FindProperty("playbackSpeed").floatValue;
            int loopType = AnimoraPlayerEditor.serializedObject.FindProperty("playLoopType").intValue;
            
            float timeIncrement = deltaTime * playbackSpeed;
            float duration = AnimoraPlayer.GetTimelineDuration();

            float newTime = AnimoraPlayer.ElapsedTime + timeIncrement;

            if (duration > 0 && newTime >= duration)
            {
                if (!_previewLoopEnabled && loopType == (int)AnimoraPlayLoopType.Once)
                {
                    newTime = duration;
                    _isPlayingInEditor = false;
                    _editorPlayState = OM_PlayState.Stopped;
                    OnPlayStateChanged(_editorPlayState);
                }
                else
                {
                    newTime = newTime % duration;
                }
            }

            AnimoraPlayerEditor.AnimoraTimeline.SetCursorTime(newTime);
            AnimoraPlayerEditor.AnimoraTimeline.UpdatePreviewElapsedTime(newTime);
            
            // Sync CurrentPreviewTime via reflection since it has a private setter
            var prop = typeof(OM_TimelineHeader<AnimoraClip, AnimoraTrack>).GetProperty("CurrentPreviewTime", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(AnimoraPlayerEditor.AnimoraTimeline.Header, newTime);
            }
        }

        /// <summary>
        /// Pauses the animation if in play mode.
        /// </summary>
        private void Pause()
        {
            if (Application.isPlaying)
                AnimoraPlayer.PauseAnimation();
            else
            {
                _isPlayingInEditor = false;
                _editorPlayState = OM_PlayState.Paused;
                OnPlayStateChanged(_editorPlayState);
            }
        }

        /// <summary>
        /// Resumes the animation if in play mode.
        /// </summary>
        private void Resume()
        {
            if (Application.isPlaying)
                AnimoraPlayer.ResumeAnimation();
            else
            {
                _isPlayingInEditor = true;
                _lastEditorTime = EditorApplication.timeSinceStartup;
                _editorPlayState = OM_PlayState.Playing;
                OnPlayStateChanged(_editorPlayState);
            }
        }

        /// <summary>
        /// Starts the animation if in play mode.
        /// </summary>
        private void Play()
        {
            if (Application.isPlaying)
                AnimoraPlayer.PlayAnimation();
            else
            {
                _isPlayingInEditor = true;
                _lastEditorTime = EditorApplication.timeSinceStartup;
                _editorPlayState = OM_PlayState.Playing;
                
                if (!AnimoraPlayerEditor.AnimoraTimeline.IsPreviewing)
                    AnimoraPlayerEditor.AnimoraTimeline.StartPreview();

                AnimoraPlayerEditor.AnimoraTimeline.SetCursorTime(0);
                AnimoraPlayerEditor.AnimoraTimeline.UpdatePreviewElapsedTime(0);
                
                OnPlayStateChanged(_editorPlayState);
            }
        }

        public void StopEditorPlayback()
        {
            _isPlayingInEditor = false;
            _editorPlayState = OM_PlayState.Stopped;
            OnPlayStateChanged(_editorPlayState);
        }

        /// <summary>
        /// Stops the animation if in play mode.
        /// </summary>
        public void Stop()
        {
            if (Application.isPlaying)
                AnimoraPlayer.StopAnimation();
            else
            {
                StopEditorPlayback();
                
                if (AnimoraPlayerEditor.AnimoraTimeline.IsPreviewing)
                    AnimoraPlayerEditor.AnimoraTimeline.StopPreview();
            }
        }

        /// <summary>
        /// Handles logic when the play button is clicked.
        /// Switches between Play, Pause, and Resume.
        /// </summary>
        private void OnPlayButtonClicked()
        {
            OM_PlayState state = Application.isPlaying ? AnimoraPlayer.PlayState : _editorPlayState;
            switch (state)
            {
                case OM_PlayState.Playing:
                    Pause();
                    break;
                case OM_PlayState.Paused:
                    Resume();
                    break;
                case OM_PlayState.Stopped:
                    Play();
                    break;
            }
        }

        private void TogglePreviewLoop()
        {
            _previewLoopEnabled = !_previewLoopEnabled;
            UpdateReplayButtonIcon();
        }

        private void UpdateReplayButtonIcon()
        {
            _replayButton.Icon.SetBackgroundFromIconContent("preAudioAutoPlayOff@2x");
            
            if (_previewLoopEnabled)
            {
                _replayButton.style.backgroundColor = new StyleColor(new Color(0.25f, 0.45f, 0.7f, 1f));
            }
            else
            {
                _replayButton.style.backgroundColor = new StyleColor(StyleKeyword.Null);
            }
        }

        /// <summary>
        /// Updates the button states based on Unity's play mode state.
        /// </summary>
        private void OnApplicationPlayModeStateChanged(PlayModeStateChange playModeState)
        {
            _playButton.SetEnabled(true);
            _stopButton.SetEnabled(true);
            _replayButton.SetEnabled(true);
            
            if (playModeState == PlayModeStateChange.ExitingEditMode)
            {
                _isPlayingInEditor = false;
                _editorPlayState = OM_PlayState.Stopped;
                OnPlayStateChanged(_editorPlayState);
            }
        }

        /// <summary>
        /// Updates button states and icons based on the current <see cref="OM_PlayState"/>.
        /// </summary>
        private void OnPlayStateChanged(OM_PlayState newState)
        {
            switch (newState)
            {
                case OM_PlayState.Playing:
                    _stopButton.SetEnabled(true);
                    _playButton.Icon.SetBackgroundFromIconContent("PauseButton@2x");
                    break;

                case OM_PlayState.Paused:
                    _stopButton.SetEnabled(true);
                    _playButton.Icon.SetBackgroundFromIconContent("PlayButton@2x");
                    break;

                case OM_PlayState.Stopped:
                    _stopButton.SetEnabled(AnimoraPlayerEditor.AnimoraTimeline != null && AnimoraPlayerEditor.AnimoraTimeline.IsPreviewing);
                    _playButton.Icon.SetBackgroundFromIconContent("PlayButton@2x");
                    break;
            }
        }
    }
}
