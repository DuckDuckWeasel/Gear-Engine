using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Waits for period of time before executing the next command in the block.
    /// </summary>
    [CommandInfo("Flow", 
                 "Wait", 
                 "Waits for period of time before executing the next command in the block.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class Wait : ActionBase
    {
        [Tooltip("Duration to wait for")]
        [SerializeField] protected FloatData duration = new FloatData(1);

        protected virtual void OnWaitComplete()
        {
            Continue();
        }

        #region Public members

        public override void OnEnter()
        {
            Invoke ("OnWaitComplete", duration.Value);
        }

        public override string GetSummary()
        {
            return duration.Value.ToString() + " seconds";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return duration.floatRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("duration")] public float durationOLD;

        protected virtual void OnEnable()
        {
            if (durationOLD != default(float))
            {
                duration.Value = durationOLD;
                durationOLD = default(float);
            }
        }

        #endregion
    }
}