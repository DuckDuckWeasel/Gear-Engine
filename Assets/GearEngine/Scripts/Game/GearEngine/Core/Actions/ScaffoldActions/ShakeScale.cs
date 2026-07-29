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
                 "Shake Scale",
                 "Randomly shakes a GameObject's rotation by a diminishing amount over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ShakeScale : ITweenCommand
    {
        [Tooltip("A scale offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data amount;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", amount.Value);
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            iTween.ShakeScale(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return amount.vector3Ref == variable || base.HasReference(variable);
        }

        #endregion

    }
}