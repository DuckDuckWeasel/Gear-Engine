using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Applies a jolt of force to a GameObject's rotation and wobbles it back to its initial rotation.
    /// </summary>
    [CommandInfo("iTween", 
                 "Punch Rotation", 
                 "Applies a jolt of force to a GameObject's rotation and wobbles it back to its initial rotation.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class PunchRotation : iTweenCommand
    {
        [Tooltip("A rotation offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data amount;

        [Tooltip("Apply the transformation in either the world coordinate or local cordinate system")]
        [SerializeField] protected Space space = Space.Self;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", amount.Value);
            tweenParams.Add("space", space);
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.PunchRotation(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return variable == amount.vector3Ref;
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("amount")] public Vector3 amountOLD;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (amountOLD != default(Vector3))
            {
                amount.Value = amountOLD;
                amountOLD = default(Vector3);
            }
        }

        #endregion
    }
}