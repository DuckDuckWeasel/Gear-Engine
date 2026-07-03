using System;
using System.Collections.Generic;
using OM.Animora.Runtime;
using UnityEngine;

namespace OM.Animora.Modules
{
    [System.Serializable]
    [AnimoraCreate("Nested Player","Core/Nested Player")]
    [AnimoraDescription("Plays a nested player")]
    [AnimoraIcon("AnimationClip Icon")]
    [AnimoraKeywords("Nested Player")]
    public class AnimoraClipNestedPlayer : AnimoraClip
    {
        [OM_StartGroup("Nested Player Settings","Settings")]
        [SerializeField] private AnimoraPlayer nestedPlayer;
        public AnimoraPlayer NestedPlayer => nestedPlayer;
        
        public override void OnPreviewChanged(AnimoraPlayer animoraPlayer, bool isOn)
        {
            base.OnPreviewChanged(animoraPlayer, isOn);
            if (nestedPlayer != null)
            {
                nestedPlayer.OnPreviewStateChanged(isOn);
            }
        }

        public override bool IsNestedPlayerClip(AnimoraPlayer targetPlayer)
        {
            return nestedPlayer != null && nestedPlayer == targetPlayer;
        }

        public override void OnEvaluate(float time, float clipTime, float normalizedTime, bool isPreviewing)
        {
            base.OnEvaluate(time, clipTime, normalizedTime, isPreviewing);
            var speed = nestedPlayer.GetTimelineDuration() / GetDuration();
            AnimoraClipsPlayUtility.EvaluateForce(nestedPlayer.ClipsToPlay, clipTime * speed, isPreviewing);
        }

        public override void Enter()
        {
            base.Enter();
            if (!IsPreviewing)
            {
                nestedPlayer.StartPlayingAndStartFirstLoop(CurrentPlayDirection, false);
            }
        }

        public override void Exit()
        {
            base.Exit();

            if (!IsPreviewing)
            {
                nestedPlayer.CompleteLoop();
                nestedPlayer.CompletePlaying();
            }
        }

        public override void OnCompletePlaying()
        {
            base.OnCompletePlaying();
            
            // If the parent timeline completed but this clip never exited naturally 
            // (e.g., its end time matched exactly the timeline's duration), force complete.
            if (!IsPreviewing && HasEntered && !HasExited)
            {
                nestedPlayer.CompleteLoop();
                nestedPlayer.CompletePlaying();
            }
        }

        public override void OnStop()
        {
            base.OnStop();
            
            // Ensure nested player stops when parent stops
            if (!IsPreviewing && HasEntered && !HasExited)
            {
                nestedPlayer.StopAnimation();
            }
        }

        public override Type GetTargetType()
        {
            return typeof(AnimoraPlayer);
        }

        public override List<Component> GetTargets()
        {
            return new List<Component>() {nestedPlayer};
        }

        public override bool HasError(out string error)
        {
            error = string.Empty;
            if (nestedPlayer == null)
            {
                error = "Nested Player is not set";
                return true;
            }

            return false;
        }
    }
}