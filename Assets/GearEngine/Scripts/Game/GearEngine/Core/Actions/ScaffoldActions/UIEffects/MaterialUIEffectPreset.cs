using Coffee.UIEffects;
using GearEngine.Presentation.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    /// <summary>
    /// A UIEffect preset that can either apply native UIEffect settings or own an animated UGUI material.
    /// </summary>
    [CreateAssetMenu(menuName = "Gear/UI Effects/Material UI Effect Preset", fileName = "E_UIE_NewEffect")]
    public sealed class MaterialUIEffectPreset : UIEffectPreset, IUIEffectPresetExecutor
    {
        [Tooltip("An optional self-animated UGUI material. When assigned, it takes precedence over native settings.")]
        [SerializeField] private Material materialPreset;

        [Tooltip("Disable native UIEffect while this preset owns the UGUI material.")]
        [SerializeField] private bool disableNativeUiEffect = true;

        public Material MaterialPreset => materialPreset;

        [Tooltip("The Disable native ui effect")]
        public bool DisableNativeUiEffect => disableNativeUiEffect;

        public void Execute(UIEffect target, bool append)
        {
            if (target == null)
            {
                Debug.LogError("[MaterialUIEffectPreset] Cannot apply an effect without a UIEffect target.");
                return;
            }

            if (materialPreset != null)
            {
                ApplyMaterial(target);
                return;
            }

            RestoreNativePath(target);
            target.LoadPreset(this, append);
        }

        private void ApplyMaterial(UIEffect target)
        {
            if (!target.TryGetComponent(out Graphic graphic))
            {
                Debug.LogError($"[MaterialUIEffectPreset] '{target.name}' requires a UGUI Graphic.");
                return;
            }

            UILoopMaterialEffect materialEffect = graphic.GetComponent<UILoopMaterialEffect>();
            if (materialEffect == null)
            {
                materialEffect = graphic.gameObject.AddComponent<UILoopMaterialEffect>();
            }

            materialEffect.DisableNativeUiEffect = disableNativeUiEffect;
            materialEffect.SetMaterial(materialPreset);
        }

        private static void RestoreNativePath(UIEffect target)
        {
            if (target.TryGetComponent(out UILoopMaterialEffect materialEffect))
            {
                materialEffect.Clear();
            }
        }
    }
}
