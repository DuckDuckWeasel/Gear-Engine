using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Deletes a saved value from permanent storage.
    /// </summary>
    [CommandInfo("Variable",
                 "Delete Save Key",
                 "Deletes a saved value from permanent storage.")]
    [Serializable]
    public class DeleteSaveKey : ActionBase
    {
        [Tooltip("Name of the saved value. Supports variable substition e.g. \"player_{$PlayerNumber}")]
        [SerializeField] protected string key = "";

        #region Public members

        public override void OnEnter()
        {
            if (key == "")
            {
                Continue();
                return;
            }

            Blackboard blackboard = GetBlackboard();

            // Prepend the current save profile (if any)
            string prefsKey = SetSaveProfile.SaveProfile + "_" + blackboard.SubstituteVariables(key);

            Blackboard.SaveService.DeleteKey(prefsKey);

            Continue();
        }

        public override string GetSummary()
        {
            if (key.Length == 0)
            {
                return "Error: No stored value key selected";
            }

            return key;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        #endregion

        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            Blackboard f = GetBlackboard();

            f.DetermineSubstituteVariables(key, referencedVariables);
        }
#endif
        #endregion Editor caches
    }
}