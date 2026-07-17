using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Sets the global pitch level for audio played with Play Music and Play Sound commands.
    /// </summary>
    [CommandInfo("Audio",
                 "Set Audio Pitch",
                 "Sets the global pitch level for audio played with Play Music and Play Sound commands.")]
    [Serializable]
    public class SetAudioPitch : ActionBase
    {
        [Range(0,1)]
        [Tooltip("Global pitch level for audio played using the Play Music and Play Sound commands")]
        [SerializeField] protected float pitch = 1;

        [Range(0,30)]
        [Tooltip("Time to fade between current pitch level and target pitch level.")]
        [SerializeField] protected float fadeDuration; 

        [Tooltip("Wait until the pitch change has finished before executing next command")]
        [SerializeField] protected bool waitUntilFinished = true;

        #region Public members

        public override void OnEnter()
        {
            System.Action onComplete = () => {
                if (waitUntilFinished)
                {
                    Continue();
                }
            };

            var musicManager = ScaffoldManager.Instance.MusicManager;

            musicManager.SetAudioPitch(pitch, fadeDuration, onComplete);

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            return "Set to " + pitch + " over " + fadeDuration + " seconds.";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}