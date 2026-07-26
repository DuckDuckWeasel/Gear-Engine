using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Stops an active iTween by name.
    /// </summary>
    [CommandInfo("iTween",
                 "Stop Tween",
                 "Stops an active iTween by name.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class StopTween : ActionBase
    {
        [Tooltip("Stop and destroy any Tweens in current scene with the supplied name")]
        [SerializeField] protected StringData tweenName;

        #region Public members

        public override void OnEnter()
        {
            iTween.StopByName(tweenName.Value);
            Continue();
        }

        public override bool HasReference(Variable variable)
        {
            return tweenName.stringRef == variable || base.HasReference(variable);
        }

        #endregion

    }
}