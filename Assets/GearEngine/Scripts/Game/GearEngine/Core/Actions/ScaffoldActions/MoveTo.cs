using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Moves a game object to a specified position over time. The position can be defined by a host.transform in another object (using To Transform) or by setting an absolute position (using To Position, if To Transform is set to None).
    /// </summary>
    [CommandInfo("iTween",
                 "Move To",
                 "Moves a game object to a specified position over time. The position can be defined by a host.transform in another object (using To Transform) or by setting an absolute position (using To Position, if To Transform is set to None).")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class MoveTo : ITweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will move to")]
        [SerializeField] protected TransformData toTransform;

        [Tooltip("Target world position that the GameObject will move to, if no From Transform is set")]
        [SerializeField] protected Vector3Data toPosition;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField] protected bool isLocal;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (toTransform.Value == null)
            {
                tweenParams.Add("position", toPosition.Value);
            }
            else
            {
                tweenParams.Add("position", toTransform.Value);
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("isLocal", isLocal);
            iTween.MoveTo(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return toTransform.transformRef == variable || toPosition.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}