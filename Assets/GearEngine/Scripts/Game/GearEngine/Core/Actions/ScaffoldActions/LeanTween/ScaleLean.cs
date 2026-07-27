using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;

namespace Scaffold
{
    /// <summary>
    /// Changes a game object's scale to a specified value over time.
    /// </summary>
    [CommandInfo("LeanTween",
                 "Scale",
                 "Changes a game object's scale to a specified value over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ScaleLean : BaseLeanTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will scale to")]
        [SerializeField]
        protected TransformData toTransform;

        [Tooltip("Target scale that the GameObject will scale to, if no To Transform is set")]
        [SerializeField]
        protected Vector3Data toScale = new Vector3Data(Vector3.one);

        public override LTDescr ExecuteTween()
        {
            var sc = toTransform.Value == null ? toScale.Value : toTransform.Value.localScale;

            if (IsInAddativeMode)
            {
                sc += targetObject.Value.gameObject.transform.localScale;
            }

            if (IsInFromMode)
            {
                var cur = targetObject.Value.gameObject.transform.localScale;
                targetObject.Value.gameObject.transform.localScale = sc;
                sc = cur;
            }

            return LeanTween.scale(targetObject.Value, sc, duration);
        }
        
        public override bool HasReference(Variable variable)
        {
            return variable == toTransform.transformRef || toScale.vector3Ref == variable || base.HasReference(variable);
        }
    }
}