using System;
using Coffee.UIEffects;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Control Tweener", "Plays, pauses, stops, resets, or manually advances a UIEffectTweener.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ControlUIEffectTweener : ActionBase
    {
        public enum TweenerOperation
        {
            PlayForward,
            PlayReverse,
            Pause,
            Resume,
            Stop,
            Reset,
            SetTime,
            UpdateTime,
        }

        [Tooltip("The UIEffectTweener to control. Takes precedence over Target GameObject.")]
        [SerializeField] protected UIEffectTweener targetTweener;

        [Tooltip("A dynamic target. This enables use inside a For Each loop over GameObjects.")]
        [SerializeField] protected GameObjectData targetGameObject;

        [SerializeField] protected TweenerOperation operation = TweenerOperation.PlayForward;

        [Tooltip("Used by Set Time and Update Time.")]
        [SerializeField] protected FloatData timeSeconds = new FloatData(0f);

        public override void OnEnter()
        {
            UIEffectTweener tweener = ResolveTweener();
            if (tweener != null)
            {
                Execute(tweener);
            }

            Continue();
        }

        public override string GetSummary()
        {
            return $"{operation} {GetTargetDescription()}";
        }

        public override bool HasReference(Variable variable)
        {
            return targetGameObject.gameObjectRef == variable || timeSeconds.floatRef == variable || base.HasReference(variable);
        }

        private UIEffectTweener ResolveTweener()
        {
            if (targetTweener != null)
            {
                return targetTweener;
            }

            GameObject target = targetGameObject.Value;
            return target != null ? target.GetComponent<UIEffectTweener>() : null;
        }

        private void Execute(UIEffectTweener tweener)
        {
            switch (operation)
            {
                case TweenerOperation.PlayForward:
                    tweener.PlayForward();
                    break;
                case TweenerOperation.PlayReverse:
                    tweener.PlayReverse();
                    break;
                case TweenerOperation.Pause:
                    tweener.SetPause(true);
                    break;
                case TweenerOperation.Resume:
                    tweener.SetPause(false);
                    break;
                case TweenerOperation.Stop:
                    tweener.Stop();
                    break;
                case TweenerOperation.Reset:
                    tweener.ResetTime();
                    break;
                case TweenerOperation.SetTime:
                    tweener.SetTime(timeSeconds.Value);
                    break;
                case TweenerOperation.UpdateTime:
                    tweener.UpdateTime(timeSeconds.Value);
                    break;
            }
        }

        private string GetTargetDescription()
        {
            if (targetTweener != null)
            {
                return targetTweener.name;
            }

            return targetGameObject.Value != null
                ? targetGameObject.Value.name
                : "Error: No UIEffectTweener or target GameObject";
        }
    }
}
