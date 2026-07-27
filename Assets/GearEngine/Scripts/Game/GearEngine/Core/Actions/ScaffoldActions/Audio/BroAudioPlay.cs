using GearEngine.Core.Actions;
using System;
using UnityEngine;
using Ami.BroAudio;

namespace Scaffold
{
    [CommandInfo("Audio",
                 "BroAudio Play",
                 "Plays an audio clip using the BroAudio system.")]
    [Serializable]
    [AddComponentMenu("")]
    public class BroAudioPlay : ActionBase
    {
        [Tooltip("The Sound ID to play from BroAudio library.")]
        [SerializeField] protected SoundID sound;

        public override void OnEnter()
        {
            if (sound.IsValid())
            {
                BroAudio.Play(sound);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (!sound.IsValid())
            {
                return "Error: No sound selected";
            }
            
            // Note: SoundID might not easily convert to string showing its name without editor extensions,
            // but we can just return a generic text for now.
            return "Play BroAudio Sound";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}
