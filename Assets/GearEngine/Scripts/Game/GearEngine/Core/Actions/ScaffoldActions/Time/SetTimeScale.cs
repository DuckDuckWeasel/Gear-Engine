using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Time", "Set Time Scale", "Instantly changes the global time scale.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetTimeScale : ActionBase
    {
        [Tooltip("The new time scale to set (1 is normal speed, 0.5 is half speed)")]
        [SerializeField] protected FloatData targetTimeScale = new FloatData(1f);

        public override void OnEnter()
        {
            Time.timeScale = targetTimeScale.Value;
            Continue();
        }

        public override string GetSummary()
        {
            return $"TimeScale = {targetTimeScale.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(216, 228, 240, 255); }
    }
}
