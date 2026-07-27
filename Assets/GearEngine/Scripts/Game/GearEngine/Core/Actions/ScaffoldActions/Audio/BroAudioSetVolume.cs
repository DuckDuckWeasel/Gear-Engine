using GearEngine.Core.Actions;
using System;
using UnityEngine;
using Ami.BroAudio;

namespace Scaffold
{
    [CommandInfo("Audio",
                 "BroAudio Set Volume",
                 "Changes or fades the volume of a BroAudio sound.")]
    [Serializable]
    [AddComponentMenu("")]
    public class BroAudioSetVolume : ActionBase
    {
        [Tooltip("The Sound ID to change volume for.")]
        [SerializeField] protected SoundID sound;

        [Tooltip("The BroAudio type to change volume for (e.g. BGM, UI, SFX). Only used if no SoundID is provided.")]
        [SerializeField] protected BroAudioType audioType = BroAudioType.None;

        [Range(0f, 1f)]
        [Tooltip("The target volume.")]
        [SerializeField] protected float targetVolume = 1f;

        [Tooltip("Fade duration in seconds. Set to 0 for instant change.")]
        [SerializeField] protected float fadeTime = 0f;

        public override void OnEnter()
        {
            if (sound.IsValid())
            {
                BroAudio.SetVolume(sound, targetVolume, fadeTime);
            }
            else if (audioType != BroAudioType.None)
            {
                BroAudio.SetVolume(audioType, targetVolume, fadeTime);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (sound.IsValid())
            {
                return "Set Volume of Sound";
            }

            if (audioType != BroAudioType.None)
            {
                return $"Set Volume of {audioType}";
            }

            return "Error: No target selected";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}
