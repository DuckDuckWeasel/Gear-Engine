using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Moves a game object from a specified position back to its starting position over time. The position can be defined by a host.transform in another object (using To Transform) or by setting an absolute position (using To Position, if To Transform is set to None).
    /// </summary>
    [CommandInfo("iTween", 
                 "Move From", 
                 "Moves a game object from a specified position back to its starting position over time. The position can be defined by a host.transform in another object (using To Transform) or by setting an absolute position (using To Position, if To Transform is set to None).")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class MoveFrom : iTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will move from")]
        [SerializeField] protected TransformData fromTransform;

        [Tooltip("Target world position that the GameObject will move from, if no From Transform is set")]
        [SerializeField] protected Vector3Data fromPosition;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField] protected bool isLocal;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (fromTransform.Value == null)
            {
                tweenParams.Add("position", fromPosition.Value);
            }
            else
            {
                tweenParams.Add("position", fromTransform.Value);
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("isLocal", isLocal);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.MoveFrom(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return fromTransform.transformRef == variable || fromPosition.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("fromTransform")] public Transform fromTransformOLD;
        [HideInInspector] [FormerlySerializedAs("fromPosition")] public Vector3 fromPositionOLD;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (fromTransformOLD != null)
            {
                fromTransform.Value = fromTransformOLD;
                fromTransformOLD = null;
            }

            if (fromPositionOLD != default(Vector3))
            {
                fromPosition.Value = fromPositionOLD;
                fromPositionOLD = default(Vector3);
            }
        }

        #endregion
    }
}