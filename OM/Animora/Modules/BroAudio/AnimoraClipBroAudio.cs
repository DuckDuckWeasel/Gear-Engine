using System;
using System.Collections.Generic;
using OM.Animora.Runtime;
using UnityEngine;
using Ami.BroAudio;
using Ami.BroAudio.Runtime;

namespace OM.Animora.Modules
{
    [System.Serializable]
    [AnimoraCreate("BroAudio Play", "Audio/BroAudio Play")]
    [AnimoraDescription("Plays a sound using BroAudio")]
    [AnimoraIcon("AudioSource Icon")]
    [AnimoraKeywords("BroAudio", "Audio", "Sound", "Play")]
    public class AnimoraClipBroAudio : AnimoraClip
    {
        [OM_StartGroup("BroAudio Settings", "Settings")]
        [SerializeField] private SoundID soundID;
        [SerializeField] private bool stopOnExit = false;
        [SerializeField] private float fadeOut = -1f;

        public override void Enter()
        {
            base.Enter();
            if (!IsPreviewing)
            {
                BroAudio.Play(soundID);
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (!IsPreviewing && stopOnExit)
            {
                if (fadeOut >= 0)
                {
                    BroAudio.Stop(soundID, fadeOut);
                }
                else
                {
                    BroAudio.Stop(soundID);
                }
            }
        }

        public override Type GetTargetType()
        {
            return typeof(Transform);
        }

        public override List<Component> GetTargets()
        {
            return null;
        }

        public override bool HasError(out string error)
        {
            error = string.Empty;
            if (!soundID.IsValid())
            {
                error = "Sound is not selected";
                return true;
            }
            return false;
        }
    }
}
