using System;
using Coffee.UIEffects;
using GearEngine.Core.Actions;
using GearEngine.Presentation.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    /// <summary>
    /// Resolves a UIEffect from either a directly assigned component or a GameObject variable.
    /// </summary>
    [Serializable]
    public abstract class UIEffectActionBase : ActionBase
    {
        protected abstract UIEffect TargetEffect { get; }

        protected abstract GameObjectData TargetGameObject { get; }

        protected bool TryResolveEffect(bool addIfMissing, out UIEffect effect)
        {
            effect = TargetEffect;
            if (effect != null)
            {
                return true;
            }

            GameObject target = TargetGameObject.Value;
            if (target == null)
            {
                return false;
            }

            effect = target.GetComponent<UIEffect>();
            if (effect == null && addIfMissing)
            {
                effect = target.AddComponent<UIEffect>();
            }

            return effect != null;
        }

        protected bool TryResolveGraphic(out Graphic graphic)
        {
            graphic = null;
            if (TargetEffect != null)
            {
                return TargetEffect.TryGetComponent(out graphic);
            }

            GameObject target = TargetGameObject.Value;
            return target != null && target.TryGetComponent(out graphic);
        }

        protected void ApplyPreset(
            UIEffect effect,
            UIEffectPreset preset,
            bool append)
        {
            if (preset is not IUIEffectPresetExecutor &&
                effect.TryGetComponent(
                    out UILoopMaterialEffect materialEffect))
            {
                materialEffect.Clear();
            }

            effect.ExecutePreset(preset, append);
        }

        protected string GetTargetDescription()
        {
            if (TargetEffect != null)
            {
                return TargetEffect.name;
            }

            if (TargetGameObject.gameObjectRef != null)
            {
                return TargetGameObject.GetDescription();
            }

            return TargetGameObject.Value != null
                ? TargetGameObject.Value.name
                : "Error: No UIEffect or target GameObject";
        }

        public override bool HasReference(Variable variable)
        {
            return TargetGameObject.gameObjectRef == variable ||
                base.HasReference(variable);
        }
    }
}
