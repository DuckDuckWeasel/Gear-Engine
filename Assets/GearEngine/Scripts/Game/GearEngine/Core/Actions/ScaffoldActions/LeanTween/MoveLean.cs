using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;

namespace Scaffold
{
    /// <summary>
    /// Moves a game object to a specified position over time. The position can be defined by a host.transform in another object (using To Transform) or by setting an absolute position (using To Position, if To Transform is set to None).
    /// </summary>
    [CommandInfo("LeanTween",
                 "Move",
                 "Moves a game object to a specified position over time. The position can be defined by a host.transform in another object (using To Transform) or by setting an absolute position (using To Position, if To Transform is set to None).")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class MoveLean : BaseLeanTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will move to")]
        [SerializeField]
        protected TransformData toTransform;

        [Tooltip("Target world position that the GameObject will move to, if no From Transform is set")]
        [SerializeField]
        protected Vector3Data toPosition;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField]
        protected bool isLocal;
        

        public override LTDescr ExecuteTween()
        {
            var loc = toTransform.Value == null ? toPosition.Value : toTransform.Value.position;

            if(IsInAddativeMode)
            {
                loc += targetObject.Value.gameObject.transform.position;
            }

            if(IsInFromMode)
            {
                var cur = targetObject.Value.gameObject.transform.position;
                targetObject.Value.gameObject.transform.position = loc;
                loc = cur;
            }

            if (isLocal)
                return LeanTween.moveLocal(targetObject.Value, loc, duration);
            else
                return LeanTween.move(targetObject.Value, loc, duration);
        }

        public override bool HasReference(Variable variable)
        {
            return toTransform.transformRef == variable || toPosition.vector3Ref == variable || base.HasReference(variable);
        }
    }
}