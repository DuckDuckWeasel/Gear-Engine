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
    public class Wait : ActionBase, IActionProgressProvider
    {
        [Tooltip("Duration to wait for")]
        [SerializeField] protected FloatData duration = new FloatData(1);

        private float waitStartedAt;
        [Tooltip("The Active wait duration")]
        private float activeWaitDuration;
        [Tooltip("The Is waiting")]
        private bool isWaiting;

        protected virtual void OnWaitComplete()
        {
            isWaiting = false;
            Continue();
        }

        #region Public members

        public override void OnEnter()
        {
            activeWaitDuration = Mathf.Max(0f, duration.Value);
            waitStartedAt = Time.time;
            isWaiting = activeWaitDuration > 0f;
            Invoke("OnWaitComplete", activeWaitDuration);
        }

        public bool TryGetExecutionProgress(out float progress)
        {
            if (!isWaiting || activeWaitDuration <= 0f)
            {
                progress = 0f;
                return false;
            }

            progress = Mathf.Clamp01((Time.time - waitStartedAt) / activeWaitDuration);
            return true;
        }

        public override void OnStopExecuting()
        {
            isWaiting = false;
            base.OnStopExecuting();
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

    }
}
