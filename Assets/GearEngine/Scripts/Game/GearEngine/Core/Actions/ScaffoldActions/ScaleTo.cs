using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Changes a game object's scale to a specified value over time.
    /// </summary>
    [CommandInfo("iTween", 
                 "Scale To", 
                 "Changes a game object's scale to a specified value over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ScaleTo : iTweenCommand
    {
        [Tooltip("Target host.transform that the GameObject will scale to")]
        [SerializeField] protected TransformData toTransform;

        [Tooltip("Target scale that the GameObject will scale to, if no To Transform is set")]
        [SerializeField] protected Vector3Data toScale = new Vector3Data(Vector3.one);

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            if (toTransform.Value == null)
            {
                tweenParams.Add("scale", toScale.Value);
            }
            else
            {
                tweenParams.Add("scale", toTransform.Value);
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.ScaleTo(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return toTransform.transformRef == variable || toScale.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("toTransform")] public Transform toTransformOLD;
        [HideInInspector] [FormerlySerializedAs("toScale")] public Vector3 toScaleOLD;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (toTransformOLD != null)
            {
                toTransform.Value = toTransformOLD;
                toTransformOLD = null;
            }

            if (toScaleOLD != default(Vector3))
            {
                toScale.Value = toScaleOLD;
                toScaleOLD = default(Vector3);
            }
        }

        #endregion
    }
}