using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Randomly shakes a GameObject's rotation by a diminishing amount over time.
    /// </summary>
    [CommandInfo("iTween",
                 "Shake Rotation",
                 "Randomly shakes a GameObject's rotation by a diminishing amount over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ShakeRotation : ITweenCommand
    {
        [Tooltip("A rotation offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data amount;

        [Tooltip("Apply the transformation in either the world coordinate or local cordinate system")]
        [SerializeField] protected Space space = Space.Self;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", amount.Value);
            tweenParams.Add("space", space);
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            iTween.ShakeRotation(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return amount.vector3Ref == variable || base.HasReference(variable);
        }

        #endregion

    }
}