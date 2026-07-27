using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Renderers", "Set Global Shader", "Sets a global shader float or color property (affects all materials using it).")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetGlobalShader : ActionBase
    {
        public enum PropertyType { Float, Color }

        [Tooltip("Name of the global shader property")]
        [SerializeField] protected StringData propertyName;

        [Tooltip("Type of property to set")]
        [SerializeField] protected PropertyType propertyType = PropertyType.Color;

        [SerializeField] protected FloatData floatValue;
        [SerializeField] protected ColorData colorValue;

        public override void OnEnter()
        {
            if (!string.IsNullOrEmpty(propertyName.Value))
            {
                if (propertyType == PropertyType.Float)
                {
                    Shader.SetGlobalFloat(propertyName.Value, floatValue.Value);
                }
                else if (propertyType == PropertyType.Color)
                {
                    Shader.SetGlobalColor(propertyName.Value, colorValue.Value);
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            return $"Global {propertyName.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
