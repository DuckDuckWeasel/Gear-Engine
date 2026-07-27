using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Particles", "Play Particles", "Plays, Stops, or Pauses a ParticleSystem.")]
    [Serializable]
    [AddComponentMenu("")]
    public class PlayParticles : ActionBase
    {
        public enum ParticleAction { Play, Stop, Pause, Clear }

        [Tooltip("The ParticleSystem to control")]
        [SerializeField] protected ParticleSystem targetParticles;
        
        [Tooltip("Action to perform on the ParticleSystem")]
        [SerializeField] protected ParticleAction action = ParticleAction.Play;

        [Tooltip("If true, includes children ParticleSystems")]
        [SerializeField] protected bool withChildren = true;

        public override void OnEnter()
        {
            if (targetParticles != null)
            {
                switch (action)
                {
                    case ParticleAction.Play:
                        targetParticles.Play(withChildren);
                        break;
                    case ParticleAction.Stop:
                        targetParticles.Stop(withChildren, ParticleSystemStopBehavior.StopEmittingAndClear);
                        break;
                    case ParticleAction.Pause:
                        targetParticles.Pause(withChildren);
                        break;
                    case ParticleAction.Clear:
                        targetParticles.Clear(withChildren);
                        break;
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetParticles == null) return "Error: No Particles";
            return $"{action} {targetParticles.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(206, 206, 206, 255); }
    }
}
