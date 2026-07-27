using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Renderers", "Set Material", "Swaps the material on a Renderer.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetMaterial : ActionBase
    {
        [Tooltip("The Renderer to modify")]
        [SerializeField] protected Renderer targetRenderer;
        
        [Tooltip("The new Material to apply")]
        [SerializeField] protected Material newMaterial;

        [Tooltip("The index of the material array to replace (usually 0)")]
        [SerializeField] protected int materialIndex = 0;

        public override void OnEnter()
        {
            if (targetRenderer != null && newMaterial != null)
            {
                Material[] mats = targetRenderer.materials;
                if (materialIndex >= 0 && materialIndex < mats.Length)
                {
                    mats[materialIndex] = newMaterial;
                    targetRenderer.materials = mats;
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetRenderer == null) return "Error: No Renderer";
            return $"Set material on {targetRenderer.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
