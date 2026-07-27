using GearEngine.Core.Actions;
using System;
using UnityEngine;
using UnityEngine.Video;

namespace Scaffold
{
    [CommandInfo("Renderers", "Play Video", "Controls a VideoPlayer component.")]
    [Serializable]
    [AddComponentMenu("")]
    public class PlayVideo : ActionBase
    {
        public enum VideoAction { Play, Pause, Stop }
        
        [Tooltip("The VideoPlayer to control")]
        [SerializeField] protected VideoPlayer targetVideoPlayer;
        
        [Tooltip("Action to perform")]
        [SerializeField] protected VideoAction action = VideoAction.Play;

        public override void OnEnter()
        {
            if (targetVideoPlayer != null)
            {
                switch (action)
                {
                    case VideoAction.Play:
                        targetVideoPlayer.Play();
                        break;
                    case VideoAction.Pause:
                        targetVideoPlayer.Pause();
                        break;
                    case VideoAction.Stop:
                        targetVideoPlayer.Stop();
                        break;
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetVideoPlayer == null) return "Error: No VideoPlayer";
            return $"{action} {targetVideoPlayer.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
