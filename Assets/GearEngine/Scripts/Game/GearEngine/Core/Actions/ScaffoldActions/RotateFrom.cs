using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Rotates a game object from the specified angles back to its starting orientation over time.
    /// </summary>
    [CommandInfo("iTween",
                 "Rotate From",
                 "Rotates a game object from the specified angles back to its starting orientation over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class RotateFrom : iTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will rotate from")]
        [SerializeField] protected TransformData fromTransform;

        [Tooltip("Target rotation that the GameObject will rotate from, if no From Transform is set")]
        [SerializeField] protected Vector3Data fromRotation;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField] protected bool isLocal;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (fromTransform.Value == null)
            {
                tweenParams.Add("rotation", fromRotation.Value);
            }
            else
            {
                tweenParams.Add("rotation", fromTransform.Value);
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("isLocal", isLocal);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.RotateFrom(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return fromTransform.transformRef == variable || fromRotation.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}