using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Changes a game object's scale by a specified offset over time.
    /// </summary>
    [CommandInfo("iTween",
                 "Scale Add",
                 "Changes a game object's scale by a specified offset over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ScaleAdd : ITweenCommand
    {
        [Tooltip("A scale offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data offset;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", offset.Value);
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            iTween.ScaleAdd(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return offset.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}