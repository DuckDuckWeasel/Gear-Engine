using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Rotates a game object to the specified angles over time.
    /// </summary>
    [CommandInfo("iTween", 
                 "Rotate To", 
                 "Rotates a game object to the specified angles over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class RotateTo : iTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will rotate to")]
        [SerializeField] protected TransformData toTransform;

        [Tooltip("Target rotation that the GameObject will rotate to, if no To Transform is set")]
        [SerializeField] protected Vector3Data toRotation;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField] protected bool isLocal;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (toTransform.Value == null)
            {
                tweenParams.Add("rotation", toRotation.Value);
            }
            else
            {
                tweenParams.Add("rotation", toTransform.Value);
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("isLocal", isLocal);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.RotateTo(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return toTransform.transformRef == variable || toRotation.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("toTransform")] public Transform toTransformOLD;
        [HideInInspector] [FormerlySerializedAs("toRotation")] public Vector3 toRotationOLD;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (toTransformOLD != null)
            {
                toTransform.Value = toTransformOLD;
                toTransformOLD = null;
            }

            if (toRotationOLD != default(Vector3))
            {
                toRotation.Value = toRotationOLD;
                toRotationOLD = default(Vector3);
            }
        }

        #endregion
    }
}