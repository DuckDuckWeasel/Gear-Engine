using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Save an Boolean, Integer, Float or String variable to persistent storage using a string key.
    /// The value can be loaded again later using the Load Variable command. You can also 
    /// use the Set Save Profile command to manage separate save profiles for multiple players.
    /// </summary>
    [CommandInfo("Variable",
                 "Save Variable",
                 "Save an Boolean, Integer, Float or String variable to persistent storage using a string key. " +
                 "The value can be loaded again later using the Load Variable command. You can also " +
                 "use the Set Save Profile command to manage separate save profiles for multiple players.")]
    [Serializable]
    public class SaveVariable : ActionBase
    {
        [Tooltip("Name of the saved value. Supports variable substition e.g. \"player_{$PlayerNumber}")]
        [SerializeField] protected string key = "";

        [Tooltip("Variable to read the value from. Only Boolean, Integer, Float and String are supported.")]
        [VariableProperty(typeof(BooleanVariable),
                          typeof(IntegerVariable),
                          typeof(FloatVariable),
                          typeof(StringVariable))]

        [SerializeField] protected Variable variable;

        #region Public members

        public override void OnEnter()
        {
            if (key == "" ||
                variable == null)
            {
                Continue();
                return;
            }

            Blackboard blackboard = GetBlackboard();

            // Prepend the current save profile (if any)
            string prefsKey = SetSaveProfile.SaveProfile + "_" + blackboard.SubstituteVariables(key);

            switch (variable)
            {
                case BooleanVariable booleanVariable:
                    Blackboard.SaveService.SetInt(prefsKey, booleanVariable.Value ? 1 : 0);
                    break;
                case IntegerVariable integerVariable:
                    Blackboard.SaveService.SetInt(prefsKey, integerVariable.Value);
                    break;
                case FloatVariable floatVariable:
                    Blackboard.SaveService.SetFloat(prefsKey, floatVariable.Value);
                    break;
                case StringVariable stringVariable:
                    Blackboard.SaveService.SetString(prefsKey, stringVariable.Value);
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

            return variable.Key + " into '" + key + "'";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable in_variable)
        {
            return this.variable == in_variable || base.HasReference(in_variable);
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