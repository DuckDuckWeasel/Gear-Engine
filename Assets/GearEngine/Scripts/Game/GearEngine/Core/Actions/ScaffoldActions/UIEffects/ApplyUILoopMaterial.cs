using System;
using GearEngine.Core.Actions;
using GearEngine.Presentation.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Apply UI Loop Material", "Applies a self-animated material to a UGUI Graphic.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ApplyUILoopMaterial : ActionBase
    {
        [Tooltip("The Graphic to modify. Takes precedence over Target GameObject.")]
        [SerializeField] protected Graphic targetGraphic;

        [Tooltip("A dynamic target. This enables use inside a For Each loop over GameObjects.")]
        [SerializeField] protected GameObjectData targetGameObject;

        [Tooltip("The self-animated UI material to apply.")]
        [SerializeField] protected Material materialPreset;

        [Tooltip("Disable a native UIEffect component so it does not replace this material.")]
        [SerializeField] protected BooleanData disableNativeUiEffect = new BooleanData(true);

        public override void OnEnter()
        {
            if (materialPreset != null && TryResolveGraphic(out Graphic graphic))
            {
                UILoopMaterialEffect effect = graphic.GetComponent<UILoopMaterialEffect>();
                if (effect == null)
                {
                    effect = graphic.gameObject.AddComponent<UILoopMaterialEffect>();
                }

                effect.DisableNativeUiEffect = disableNativeUiEffect.Value;
                effect.SetMaterial(materialPreset);
            }

            Continue();
        }

        public override string GetSummary()
        {
            return materialPreset == null ? "Error: No UI material" : $"{materialPreset.name} on {GetTargetDescription()}";
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
