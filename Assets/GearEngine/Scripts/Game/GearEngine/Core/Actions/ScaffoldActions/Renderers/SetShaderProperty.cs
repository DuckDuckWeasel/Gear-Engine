using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Renderers", "Set Shader Property", "Modifies a float or color property on a Material by its property name.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetShaderProperty : ActionBase
    {
        public enum PropertyType { Float, Color }

        [Tooltip("The Renderer to modify")]
        [SerializeField] protected Renderer targetRenderer;
        
        [Tooltip("Name of the shader property (e.g., _BaseColor, _EmissionColor)")]
        [SerializeField] protected StringData propertyName;

        [Tooltip("Type of property to set")]
        [SerializeField] protected PropertyType propertyType = PropertyType.Color;

        [SerializeField] protected FloatData floatValue;
        [SerializeField] protected ColorData colorValue;

        [Tooltip("Material index to modify")]
        [SerializeField] protected int materialIndex = 0;

        public override void OnEnter()
        {
            if (targetRenderer != null && !string.IsNullOrEmpty(propertyName.Value))
            {
                Material[] mats = targetRenderer.materials;
                if (materialIndex >= 0 && materialIndex < mats.Length)
                {
                    Material mat = mats[materialIndex];
                    if (propertyType == PropertyType.Float)
                    {
                        mat.SetFloat(propertyName.Value, floatValue.Value);
                    }
                    else if (propertyType == PropertyType.Color)
                    {
                        mat.SetColor(propertyName.Value, colorValue.Value);
                    }
                    targetRenderer.materials = mats; // Apply back
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetRenderer == null) return "Error: No Renderer";
            return $"Set {propertyName.Value} on {targetRenderer.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
