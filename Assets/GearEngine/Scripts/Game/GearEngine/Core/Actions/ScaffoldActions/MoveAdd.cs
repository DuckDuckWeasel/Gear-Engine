using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Moves a game object by a specified offset over time.
    /// </summary>
    [CommandInfo("iTween", 
                 "Move Add", 
                 "Moves a game object by a specified offset over time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class MoveAdd : iTweenCommand
    {
        [Tooltip("A translation offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data offset;

        [Tooltip("Apply the transformation in either the world coordinate or local cordinate system")]
        [SerializeField] protected Space space = Space.Self;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", tweenName.Value);
            tweenParams.Add("amount", offset.Value);
            tweenParams.Add("space", space);
            tweenParams.Add("time", duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", host.gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.MoveAdd(targetObject.Value, tweenParams);
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