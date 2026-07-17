using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Sets the global volume level for audio played with Play Music and Play Sound commands.
    /// </summary>
    [CommandInfo("Audio",
                 "Set Audio Volume",
                 "Sets the global volume level for audio played with Play Music and Play Sound commands.")]
    [Serializable]
    public class SetAudioVolume : ActionBase
    {
        [Range(0,1)]
        [Tooltip("Global volume level for audio played using Play Music and Play Sound")]
        [SerializeField] protected float volume = 1f;

        [Range(0,30)]
        [Tooltip("Time to fade between current volume level and target volume level.")]
        [SerializeField] protected float fadeDuration = 1f;

        [Tooltip("Wait until the volume fade has completed before continuing.")]
        [SerializeField] protected bool waitUntilFinished = true;

        #region Public members

        public override void OnEnter()
        {
            var musicManager = ScaffoldManager.Instance.MusicManager;

            musicManager.SetAudioVolume(volume, fadeDuration, () => {
                if (waitUntilFinished)
                {
                    Continue();
                }
            });

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            return "Set to " + volume + " over " + fadeDuration + " seconds.";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}