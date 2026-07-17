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
    public class ScaleAdd : iTweenCommand
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
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.ScaleAdd(targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return offset.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("offset")] public Vector3 offsetOLD;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (offsetOLD != default(Vector3))
            {
                offset.Value = offsetOLD;
                offsetOLD = default(Vector3);
            }
        }

        #endregion
    }
}