using System;
using GearEngine.Core.Actions;

using UnityEngine;
using Ami.BroAudio;

namespace Scaffold
{
    /// <summary>
    /// Stops the currently playing game music.
    /// </summary>
    [CommandInfo("Audio",
                 "Stop Music",
                 "Stops the currently playing game music.")]
    [Serializable]
    public class StopMusic : ActionBase
    {
        #region Public members

        public override void OnEnter()
        {
            BroAudio.Stop(BroAudioType.Music);
            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}