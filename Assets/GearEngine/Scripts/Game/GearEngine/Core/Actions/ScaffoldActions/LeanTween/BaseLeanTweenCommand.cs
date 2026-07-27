using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Abstract base class for LeanTween commands.
    /// </summary>
    [ExecuteInEditMode]
    [Serializable]
    public abstract class BaseLeanTweenCommand : ActionBase
    {
        [Tooltip("Target game object to apply the Tween to")]
        [SerializeField]
        protected GameObjectData targetObject;

        [Tooltip("The time in seconds the animation will take to complete")]
        [SerializeField]
        protected FloatData duration = new FloatData(1f);

        public enum ToFrom { To, From }
        [Tooltip("Does the tween act from current TO destination or is it reversed and act FROM destination to its current")]
        [SerializeField]
        protected ToFrom toFrom;
        public bool IsInFromMode { get { return toFrom == ToFrom.From; } }

        public enum AbsAdd { Absolute, Additive }
        [Tooltip("Does the tween use the value as a target or as a delta to be added to current.")]
        [SerializeField]
        protected AbsAdd absAdd;
        public bool IsInAddativeMode { get { return absAdd == AbsAdd.Additive; } }


        [Tooltip("The shape of the easing curve applied to the animation")]
        [SerializeField]
        protected LeanTweenType easeType = LeanTweenType.easeInOutQuad;
        
        [Tooltip("The type of loop to apply once the animation has completed")]
        [SerializeField]
        protected LeanTweenType loopType = LeanTweenType.once;

        [Tooltip("Number of times to repeat the tween, -1 is infinite.")]
        [SerializeField]
        protected int repeats = 0;

        [Tooltip("Stop any previously LeanTweens on this object before adding this one. Warning; expensive.")]
        [SerializeField]
        protected bool stopPreviousTweens = false;

        [Tooltip("Wait until the tween has finished before executing the next command")]
        [SerializeField]
        protected bool waitUntilFinished = true;


        [HideInInspector] protected LTDescr ourTween;


        protected virtual void OnTweenComplete()
        {
            Continue();
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
                LeanTween.cancel(targetObject.Value);
            }

            ourTween = ExecuteTween();

            ourTween.setEase(easeType)
                    .setRepeat(repeats)
                    .setLoopType(loopType);

            if (waitUntilFinished)
            {
                if(ourTween != null)
                {
                    ourTween.setOnComplete(OnTweenComplete);
                }
            }
            else
            {
                Continue();
            }
        }

        public abstract LTDescr ExecuteTween();

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
            return variable == targetObject.gameObjectRef || variable == duration.floatRef;
        }

        #endregion

    }
}