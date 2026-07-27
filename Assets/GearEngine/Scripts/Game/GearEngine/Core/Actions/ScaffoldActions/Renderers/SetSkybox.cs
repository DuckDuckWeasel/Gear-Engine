using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Renderers", "Set Skybox", "Changes the global Skybox material in RenderSettings.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetSkybox : ActionBase
    {
        [Tooltip("The new Skybox Material")]
        [SerializeField] protected Material skyboxMaterial;

        public override void OnEnter()
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (skyboxMaterial == null) return "Error: No Material";
            return $"Set Skybox to {skyboxMaterial.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
