using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Loads a saved value and stores it in a Boolean, Integer, Float or String variable. If the key is not found then the variable is not modified.
    /// </summary>
    [CommandInfo("Variable",
                 "Load Variable",
                 "Loads a saved value and stores it in a Boolean, Integer, Float or String variable. If the key is not found then the variable is not modified.")]
    [Serializable]
    public class LoadVariable : ActionBase
    {
        [Tooltip("Name of the saved value. Supports variable substition e.g. \"player_{$PlayerNumber}\"")]
        [SerializeField] protected string key = "";

        [Tooltip("Variable to store the value in.")]
        [VariableProperty(typeof(BooleanVariable),
                          typeof(IntegerVariable),
                          typeof(FloatVariable),
                          typeof(StringVariable))]

        [SerializeField] protected Variable variable;

        #region Public members

        public override void OnEnter()
        {
            Blackboard blackboard = GetBlackboard();

            // Prepend the current save profile (if any) and make sure all inputs are valid
            string prefsKey = SetSaveProfile.SaveProfile + "_" + blackboard.SubstituteVariables(key);
            bool validKey = key != "" && Blackboard.SaveService.HasKey(prefsKey);
            bool validVariable = variable != null;

            if (!validKey || !validVariable)
            {
                Continue();
                return;
            }

            switch (variable)
            {
                case BooleanVariable booleanVariable:
                    booleanVariable.Value = Blackboard.SaveService.GetInt(prefsKey) == 1;
                    break;
                case IntegerVariable integerVariable:
                    integerVariable.Value = Blackboard.SaveService.GetInt(prefsKey);
                    break;
                case FloatVariable floatVariable:
                    floatVariable.Value = Blackboard.SaveService.GetFloat(prefsKey);
                    break;
                case StringVariable stringVariable:
                    stringVariable.Value = Blackboard.SaveService.GetString(prefsKey);
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (key.Length == 0)
            {
                return "Error: No stored value key selected";
            }

            if (variable == null)
            {
                return "Error: No variable selected";
            }

            return "'" + key + "' into " + variable.Key;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable in_variable)
        {
            return this.variable == in_variable ||
                base.HasReference(in_variable);
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