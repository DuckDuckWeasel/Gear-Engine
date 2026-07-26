using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Axis to apply the tween on.
    /// </summary>
    public enum iTweenAxis
    {
        /// <summary> Don't specify an axis. </summary>
        None,
        /// <summary> Apply the tween on the X axis only. </summary>
        X,
        /// <summary> Apply the tween on the Y axis only. </summary>
        Y,
        /// <summary> Apply the tween on the Z axis only. </summary>
        Z
    }

    /// <summary>
    /// Abstract base class for iTween commands.
    /// </summary>
    [ExecuteInEditMode]
    [Serializable]
    public abstract class iTweenCommand : ActionBase
    {
        [Tooltip("Target game object to apply the Tween to")]
        [SerializeField] protected GameObjectData targetObject;

        [Tooltip("An individual name useful for stopping iTweens by name")]
        [SerializeField] protected StringData tweenName;

        [Tooltip("The time in seconds the animation will take to complete")]
        [SerializeField] protected FloatData duration = new FloatData(1f);

        [Tooltip("The shape of the easing curve applied to the animation")]
        [SerializeField] protected iTween.EaseType easeType = iTween.EaseType.easeInOutQuad;

        [Tooltip("The type of loop to apply once the animation has completed")]
        [SerializeField] protected iTween.LoopType loopType = iTween.LoopType.none;

        [Tooltip("Stop any previously added iTweens on this object before adding this iTween")]
        [SerializeField] protected bool stopPreviousTweens = false;

        [Tooltip("Wait until the tween has finished before executing the next command")]
        [SerializeField] protected bool waitUntilFinished = true;

        protected virtual void OniTweenComplete(object param)
        {
            Command command = param as Command;
            if (command != null && command.Equals(this))
            {
                if (waitUntilFinished)
                {
                    Continue();
                }
            }
        }

        #region Public members

        public override void OnEnter()
        {
            if (targetObject.Value == null)
            {
                Continue();
                return;
            }

            if (stopPreviousTweens)
            {
                // Force any existing iTweens on this target object to complete immediately
                iTween[] tweens = targetObject.Value.GetComponents<iTween>();
                for (int i = 0; i < tweens.Length; i++)
                {
                    iTween tween = tweens[i];
                    tween.time = 0;
                    tween.SendMessage("Update");
                }
            }

            DoTween();

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public virtual void DoTween()
        { }

        public override string GetSummary()
        {
            if (targetObject.Value == null)
            {
                return "Error: No target object selected";
            }

            return targetObject.Value.name + " over " + duration.Value + " seconds";
        }

        public override Color GetButtonColor()
        {
            return new Color32(233, 163, 180, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetObject.gameObjectRef == variable || tweenName.stringRef == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}