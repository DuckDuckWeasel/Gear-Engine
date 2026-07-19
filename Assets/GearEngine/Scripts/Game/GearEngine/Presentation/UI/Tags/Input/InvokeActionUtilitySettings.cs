using System;
using Scaffold;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    /// <summary>
    /// Stores the score and interruption policy used by a Utility Selector child action.
    /// </summary>
    [Serializable]
    public struct InvokeActionUtilitySettings
    {
        [Tooltip("Variable-backed utility score. The action with the highest value runs.")]
        [SerializeField] private FloatData utility;

        [Tooltip("Prevents a higher-utility action from interrupting this action while it is running.")]
        [SerializeField] private bool blockDuringExecution;

        [Tooltip("Relative selection weight used by Random order, from 0 to 100 percent.")]
        [Range(0f, 100f)]
        [SerializeField] private float weight;

        [SerializeField] private bool weightInitialized;

        [SerializeField] private bool weightOverride;

        public InvokeActionUtilitySettings(float utilityValue, bool blockDuringExecution)
        {
            utility = new FloatData(utilityValue);
            this.blockDuringExecution = blockDuringExecution;
            weight = 0f;
            weightInitialized = false;
            weightOverride = false;
        }

        public float Utility => utility.Value;

        public bool BlockDuringExecution => blockDuringExecution;

        public float Weight => weightInitialized ? Mathf.Clamp(weight, 0f, 100f) : 0f;

        public bool HasWeightOverride => weightOverride;

        public bool HasReference(Variable variable)
        {
            return utility.floatRef == variable;
        }

        public void SetUtility(float value)
        {
            utility.Value = value;
        }

        public void SetUtility(FloatData value)
        {
            utility = value;
        }

        public void SetBlockDuringExecution(bool shouldBlock)
        {
            blockDuringExecution = shouldBlock;
        }

        public void SetWeight(float value)
        {
            weight = Mathf.Clamp(value, 0f, 100f);
            weightInitialized = true;
            weightOverride = true;
        }

        public void ClearWeightOverride()
        {
            weight = 0f;
            weightInitialized = false;
            weightOverride = false;
        }

        public bool MigrateWeightOverride()
        {
            if (weightOverride || !weightInitialized || Mathf.Approximately(weight, 100f))
            {
                return false;
            }

            weightOverride = true;
            return true;
        }
    }
}
