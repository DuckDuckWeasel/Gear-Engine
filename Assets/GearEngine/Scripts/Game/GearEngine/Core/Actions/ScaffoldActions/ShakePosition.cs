using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Randomly shakes a GameObject's position by a diminishing amount over time.
    /// </summary>
    [CommandInfo("iTween", 
                 "Shake Position", 
                 "Randomly shakes a GameObject's position by a diminishing amount over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ShakePosition : iTweenCommand
    {
        [Tooltip("A translation offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data amount;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField] protected bool isLocal;

        [Tooltip("Restricts rotation to the supplied axis only")]
        [SerializeField] protected iTweenAxis axis;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", amount.Value);
            switch (axis)
            {
            case iTweenAxis.X:
                tweenParams.Add("axis", "x");
                break;
            case iTweenAxis.Y:
                tweenParams.Add("axis", "y");
                break;
            case iTweenAxis.Z:
                tweenParams.Add("axis", "z");
                break;
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("isLocal", isLocal);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.ShakePosition(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return amount.vector3Ref == variable || base.HasReference(variable);
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