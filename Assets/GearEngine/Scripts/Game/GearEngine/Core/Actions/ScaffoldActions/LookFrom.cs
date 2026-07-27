using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Instantly rotates a GameObject to look at the supplied Vector3 then returns it to it's starting rotation over time.
    /// </summary>
    [CommandInfo("iTween",
                 "Look From",
                 "Instantly rotates a GameObject to look at the supplied Vector3 then returns it to it's starting rotation over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class LookFrom : ITweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will look at")]
        [SerializeField] protected TransformData fromTransform;

        [Tooltip("Target world position that the GameObject will look at, if no From Transform is set")]
        [SerializeField] protected Vector3Data fromPosition;

        [Tooltip("Restricts rotation to the supplied axis only")]
        [SerializeField] protected ITweenAxis axis;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (fromTransform.Value == null)
            {
                tweenParams.Add("looktarget", fromPosition.Value);
            }
            else
            {
                tweenParams.Add("looktarget", fromTransform.Value);
            }
            switch (axis)
            {
                case ITweenAxis.X:
                    tweenParams.Add("axis", "x");
                    break;
                case ITweenAxis.Y:
                    tweenParams.Add("axis", "y");
                    break;
                case ITweenAxis.Z:
                    tweenParams.Add("axis", "z");
                    break;
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            iTween.LookFrom(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return fromTransform.transformRef == variable || fromPosition.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}