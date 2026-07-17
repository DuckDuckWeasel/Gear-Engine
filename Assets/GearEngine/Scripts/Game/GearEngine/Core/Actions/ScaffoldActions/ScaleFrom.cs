using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Changes a game object's scale to the specified value and back to its original scale over time.
    /// </summary>
    [CommandInfo("iTween", 
                 "Scale From", 
                 "Changes a game object's scale to the specified value and back to its original scale over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ScaleFrom : iTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will scale from")]
        [SerializeField] protected TransformData fromTransform;

        [Tooltip("Target scale that the GameObject will scale from, if no From Transform is set")]
        [SerializeField] protected Vector3Data fromScale;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (fromTransform.Value == null)
            {
                tweenParams.Add("scale", fromScale.Value);
            }
            else
            {
                tweenParams.Add("scale", fromTransform.Value);
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.ScaleFrom(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return fromTransform.transformRef == variable || fromScale.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("fromTransform")] public Transform fromTransformOLD;
        [HideInInspector] [FormerlySerializedAs("fromScale")] public Vector3 fromScaleOLD;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (fromTransformOLD != null)
            {
                fromTransform.Value = fromTransformOLD;
                fromTransformOLD = null;
            }

            if (fromScaleOLD != default(Vector3))
            {
                fromScale.Value = fromScaleOLD;
                fromScaleOLD = default(Vector3);
            }
        }

        #endregion
    }
}