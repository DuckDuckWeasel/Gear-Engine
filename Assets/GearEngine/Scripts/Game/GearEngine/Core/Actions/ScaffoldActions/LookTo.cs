using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Rotates a GameObject to look at a supplied Transform or Vector3 over time.
    /// </summary>
    [CommandInfo("iTween",
                 "Look To",
                 "Rotates a GameObject to look at a supplied Transform or Vector3 over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class LookTo : iTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will look at")]
        [SerializeField] protected TransformData toTransform;

        [Tooltip("Target world position that the GameObject will look at, if no From Transform is set")]
        [SerializeField] protected Vector3Data toPosition;

        [Tooltip("Restricts rotation to the supplied axis only")]
        [SerializeField] protected iTweenAxis axis;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (toTransform.Value == null)
            {
                tweenParams.Add("looktarget", toPosition.Value);
            }
            else
            {
                tweenParams.Add("looktarget", toTransform.Value);
            }
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
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.LookTo(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return toTransform.transformRef == variable || toPosition.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}