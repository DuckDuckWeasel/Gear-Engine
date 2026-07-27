using System;
using Coffee.UIEffects;
using GearEngine.Core.Actions;
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
        [Tooltip("The UIEffect component to modify. Takes precedence over Target GameObject.")]
        [SerializeField] protected UIEffect targetEffect;

        [Tooltip("A dynamic target. This enables use inside a For Each loop over GameObjects.")]
        [SerializeField] protected GameObjectData targetGameObject;

        protected bool TryResolveEffect(bool addIfMissing, out UIEffect effect)
        {
            effect = targetEffect;
            if (effect != null)
            {
                return true;
            }

            GameObject target = targetGameObject.Value;
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
            if (targetEffect != null)
            {
                return targetEffect.TryGetComponent(out graphic);
            }

            GameObject target = targetGameObject.Value;
            return target != null && target.TryGetComponent(out graphic);
        }

        protected string GetTargetDescription()
        {
            if (targetEffect != null)
            {
                return targetEffect.name;
            }

            if (targetGameObject.gameObjectRef != null)
            {
                return targetGameObject.GetDescription();
            }

            return targetGameObject.Value != null
                ? targetGameObject.Value.name
                : "Error: No UIEffect or target GameObject";
        }

        public override bool HasReference(Variable variable)
        {
            return targetGameObject.gameObjectRef == variable || base.HasReference(variable);
        }
    }
}
