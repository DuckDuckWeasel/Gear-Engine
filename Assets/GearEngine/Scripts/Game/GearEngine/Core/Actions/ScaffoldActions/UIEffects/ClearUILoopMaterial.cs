using System;
using GearEngine.Core.Actions;
using GearEngine.Presentation.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Clear Loop Material", "Restores the material that was active before a loop material was applied.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ClearUILoopMaterial : ActionBase
    {
        [Tooltip("The Graphic to modify. Takes precedence over Target GameObject.")]
        [SerializeField] protected Graphic targetGraphic;

        [Tooltip("A dynamic target. This enables use inside a For Each loop over GameObjects.")]
        [SerializeField] protected GameObjectData targetGameObject;

        public override void OnEnter()
        {
            if (TryResolveGraphic(out Graphic graphic) && graphic.TryGetComponent(out UILoopMaterialEffect effect))
            {
                effect.Restore();
            }

            Continue();
        }

        public override string GetSummary()
        {
            return $"Clear loop material on {GetTargetDescription()}";
        }

        public override bool HasReference(Variable variable)
        {
            return targetGameObject.gameObjectRef == variable || base.HasReference(variable);
        }

        private bool TryResolveGraphic(out Graphic graphic)
        {
            graphic = targetGraphic;
            if (graphic != null)
            {
                return true;
            }

            GameObject target = targetGameObject.Value;
            return target != null && target.TryGetComponent(out graphic);
        }

        private string GetTargetDescription()
        {
            if (targetGraphic != null)
            {
                return targetGraphic.name;
            }

            return targetGameObject.Value != null ? targetGameObject.Value.name : "Error: No UGUI Graphic or target GameObject";
        }
    }
}
