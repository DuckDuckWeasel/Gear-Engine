using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Applies a jolt of force to a GameObject's scale and wobbles it back to its initial scale.
    /// </summary>
    [CommandInfo("iTween",
                 "Punch Scale",
                 "Applies a jolt of force to a GameObject's scale and wobbles it back to its initial scale.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class PunchScale : iTweenCommand
    {
        [Tooltip("A scale offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data amount;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", amount.Value);
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.PunchScale(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return variable == amount.vector3Ref;
        }

        #endregion

    }
}