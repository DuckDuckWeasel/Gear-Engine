using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Stops the currently playing game music.
    /// </summary>
    [CommandInfo("Audio", 
                 "Stop Ambiance", 
                 "Stops the currently playing game ambiance.")]
    [Serializable]
    public class StopAmbiance : ActionBase
    {
        #region Public members

        public override void OnEnter()
        {
            var musicManager = ScaffoldManager.Instance.MusicManager;

            musicManager.StopAmbiance();

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}