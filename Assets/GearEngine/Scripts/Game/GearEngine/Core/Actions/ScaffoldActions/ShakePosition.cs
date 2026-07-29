using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Randomly shakes a GameObject's position by a diminishing amount over time.
    /// </summary>
    [CommandInfo("iTween",
                 "Shake Position",
                 "Randomly shakes a GameObject's position by a diminishing amount over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ShakePosition : ITweenCommand
    {
        [Tooltip("A translation offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data amount;

        [Tooltip("Whether to animate in world space or relative to the parent. False by default.")]
        [SerializeField] protected bool isLocal;

        [Tooltip("Restricts rotation to the supplied axis only")]
        [SerializeField] protected ITweenAxis axis;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", amount.Value);
            switch (axis)
            {
                case ITweenAxis.X:
                    tweenParams.Add("axis", "x");
                    break;
                case ITweenAxis.Y:
                    tweenParams.Add("axis", "y");
                    break;
                case ITweenAxis.Z:
                    tweenParams.Add("axis", "z");
                    break;
            }
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("isLocal", isLocal);
            iTween.ShakePosition(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return amount.vector3Ref == variable || base.HasReference(variable);
        }

        #endregion

    }
}