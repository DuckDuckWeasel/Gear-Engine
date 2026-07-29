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
    public class ScaleTo : ITweenCommand
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
            iTween.ScaleTo(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return toTransform.transformRef == variable || toScale.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}