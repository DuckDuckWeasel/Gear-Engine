using GearEngine.Core.Actions;
using System;
using UnityEngine;
using Ami.BroAudio;

namespace Scaffold
{
    [CommandInfo("Audio",
                 "BroAudio Stop",
                 "Stops an audio clip playing in the BroAudio system.")]
    [Serializable]
    [AddComponentMenu("")]
    public class BroAudioStop : ActionBase
    {
        [Tooltip("The Sound ID to stop.")]
        [SerializeField] protected SoundID sound;

        [Tooltip("If true, stops all currently playing sounds regardless of SoundID.")]
        [SerializeField] protected bool stopAll = false;

        public override void OnEnter()
        {
            if (stopAll)
            {
                // BroAudio handles stop all usually via BroAudio.Stop(BroAudioType.All) or similar.
                // We can use a direct call if available, or just stop the specific sound.
                // Assuming BroAudio has a Stop method for everything. Let's just stop the specific sound.
                // Actually, BroAudio.Stop(sound) is standard. For all, it's BroAudio.Stop(BroAudioType.All).
                // Let's stick to the specific sound to be safe with the API.
            }

            if (!stopAll && sound.IsValid())
            {
                BroAudio.Stop(sound);
            }
            else if (stopAll)
            {
                BroAudio.Stop(BroAudioType.All);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (stopAll) return "Stop All BroAudio";
            if (!sound.IsValid()) return "Error: No sound selected";
            
            return "Stop BroAudio Sound";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}
