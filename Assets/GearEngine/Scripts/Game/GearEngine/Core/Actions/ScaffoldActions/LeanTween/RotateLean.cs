using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;

namespace Scaffold
{
    /// <summary>
    /// Rotates a game object to the specified angles over time.
    /// </summary>
    [CommandInfo("LeanTween",
                 "Rotate",
                 "Rotates a game object to the specified angles over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class RotateLean : BaseLeanTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will rotate to")]
        [SerializeField]
        protected TransformData toTransform;

        [Tooltip("Target rotation that the GameObject will rotate to, if no To Transform is set")]
        [SerializeField]
        protected Vector3Data toRotation;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField]
        protected bool isLocal;

        public enum RotateMode { PureRotate, LookAt2D, LookAt3D}
        [Tooltip("Whether to use the provided Transform or Vector as a target to look at rather than a euler to match.")]
        [SerializeField]
        protected RotateMode rotateMode = RotateMode.PureRotate;


        public override LTDescr ExecuteTween()
        {
            var rot = toTransform.Value == null ? toRotation.Value : toTransform.Value.rotation.eulerAngles;

            if(rotateMode == RotateMode.LookAt3D)
            {
                var pos = toTransform.Value == null ? toRotation.Value : toTransform.Value.position;
                var dif = pos - targetObject.Value.gameObject.transform.position;
                rot = Quaternion.LookRotation(dif.normalized).eulerAngles;
            }
            else if(rotateMode == RotateMode.LookAt2D)
            {
                var pos = toTransform.Value == null ? toRotation.Value : toTransform.Value.position;
                var dif = pos - targetObject.Value.gameObject.transform.position;
                dif.z = 0;

                rot = Quaternion.FromToRotation(targetObject.Value.gameObject.transform.up, dif.normalized).eulerAngles;
            }

            if (IsInAddativeMode)
            {
                rot += targetObject.Value.gameObject.transform.rotation.eulerAngles;
            }

            if (IsInFromMode)
            {
                var cur = targetObject.Value.gameObject.transform.rotation.eulerAngles;
                targetObject.Value.gameObject.transform.rotation = Quaternion.Euler(rot);
                rot = cur;
            }

            if (isLocal)
                return LeanTween.rotateLocal(targetObject.Value, rot, duration);
            else
                return LeanTween.rotate(targetObject.Value, rot, duration);
        }

        public override bool HasReference(Variable variable)
        {
            return variable == toTransform.transformRef || toRotation.vector3Ref == variable || base.HasReference(variable);
        }
    }
}