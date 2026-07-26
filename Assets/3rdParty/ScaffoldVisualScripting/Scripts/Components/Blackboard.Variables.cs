using UnityEngine;
using System.Collections.Generic;

namespace Scaffold
{
    public partial class Blackboard
    {
        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You will need to cast the returned variable to the correct sub-type.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = blackboard.GetVariable("MyBool") as BooleanVariable;
        /// boolVar.Value = false;
        /// </summary>
        public Variable GetVariable(string key)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                Variable variable = variables[i];
                if (variable != null && variable.Key == key)
                {
                    return variable;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = blackboard.GetVariable<BooleanVariable>("MyBool");
        /// boolVar.Value = false;
        /// </summary>
        public T GetVariable<T>(string key) where T : Variable
        {
            for (int i = 0; i < variables.Count; i++)
            {
                Variable variable = variables[i];
                if (variable != null && variable.Key == key)
                {
                    return variable as T;
                }
            }

            Debug.LogWarning("Variable " + key + " not found.");
            return null;
        }

        /// <summary>
        /// Returns a list of variables matching the specified type.
        /// </summary>
        public virtual List<T> GetVariables<T>() where T : Variable
        {
            List<T> varsFound = new List<T>();

            for (int i = 0; i < Variables.Count; i++)
            {
                Variable currentVar = Variables[i];
                if (currentVar is T)
                {
                    varsFound.Add(currentVar as T);
                }
            }

            return varsFound;
        }

        /// <summary>
        /// Register a new variable with the Blackboard at runtime.
        /// The variable should be added as a component on the Blackboard game object.
        /// </summary>
        public void SetVariable<T>(string key, T newvariable) where T : Variable
        {
            for (int i = 0; i < variables.Count; i++)
            {
                Variable v = variables[i];
                if (v != null && v.Key == key)
                {
                    T variable = v as T;
                    if (variable != null)
                    {
                        variable = newvariable;
                        return;
                    }
                }
            }

            Debug.LogWarning("Variable " + key + " not found.");
        }

        /// <summary>
        /// Checks if a given variable exists in the blackboard.
        /// </summary>
        public virtual bool HasVariable(string key)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                Variable v = variables[i];
                if (v != null && v.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the list of variable names in the Blackboard.
        /// </summary>
        public virtual string[] GetVariableNames()
        {
            string[] vList = new string[variables.Count];

            for (int i = 0; i < variables.Count; i++)
            {
                Variable v = variables[i];
                if (v != null)
                {
                    vList[i] = v.Key;
                }
            }
            return vList;
        }

        /// <summary>
        /// Gets a list of all variables with public scope in this Blackboard.
        /// </summary>
        public virtual List<Variable> GetPublicVariables()
        {
            List<Variable> publicVariables = new List<Variable>();
            for (int i = 0; i < variables.Count; i++)
            {
                Variable v = variables[i];
                if (v != null && v.Scope == VariableScope.Public)
                {
                    publicVariables.Add(v);
                }
            }

            return publicVariables;
        }

        /// <summary>
        /// Gets the value of a boolean variable.
        /// Returns false if the variable key does not exist.
        /// </summary>
        public virtual bool GetBooleanVariable(string key)
        {
            BooleanVariable variable = GetVariable<BooleanVariable>(key);
            if (variable != null)
            {
                return GetVariable<BooleanVariable>(key).Value;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the value of a boolean variable.
        /// The variable must already be added to the list of variables for this Blackboard.
        /// </summary>
        public virtual void SetBooleanVariable(string key, bool value)
        {
            BooleanVariable variable = GetVariable<BooleanVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of an integer variable.
        /// Returns 0 if the variable key does not exist.
        /// </summary>
        public virtual int GetIntegerVariable(string key)
        {
            IntegerVariable variable = GetVariable<IntegerVariable>(key);
            if (variable != null)
            {
                return GetVariable<IntegerVariable>(key).Value;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Sets the value of an integer variable.
        /// The variable must already be added to the list of variables for this Blackboard.
        /// </summary>
        public virtual void SetIntegerVariable(string key, int value)
        {
            IntegerVariable variable = GetVariable<IntegerVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a float variable.
        /// Returns 0 if the variable key does not exist.
        /// </summary>
        public virtual float GetFloatVariable(string key)
        {
            FloatVariable variable = GetVariable<FloatVariable>(key);
            if (variable != null)
            {
                return GetVariable<FloatVariable>(key).Value;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Sets the value of a float variable.
        /// The variable must already be added to the list of variables for this Blackboard.
        /// </summary>
        public virtual void SetFloatVariable(string key, float value)
        {
            FloatVariable variable = GetVariable<FloatVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a string variable.
        /// Returns the empty string if the variable key does not exist.
        /// </summary>
        public virtual string GetStringVariable(string key)
        {
            StringVariable variable = GetVariable<StringVariable>(key);
            if (variable != null)
            {
                return GetVariable<StringVariable>(key).Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Sets the value of a string variable.
        /// The variable must already be added to the list of variables for this Blackboard.
        /// </summary>
        public virtual void SetStringVariable(string key, string value)
        {
            StringVariable variable = GetVariable<StringVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a GameObject variable.
        /// Returns null if the variable key does not exist.
        /// </summary>
        public virtual GameObject GetGameObjectVariable(string key)
        {
            GameObjectVariable variable = GetVariable<GameObjectVariable>(key);

            if (variable != null)
            {
                return GetVariable<GameObjectVariable>(key).Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the value of a GameObject variable.
        /// The variable must already be added to the list of variables for this Blackboard.
        /// </summary>
        public virtual void SetGameObjectVariable(string key, GameObject value)
        {
            GameObjectVariable variable = GetVariable<GameObjectVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a Transform variable.
        /// Returns null if the variable key does not exist.
        /// </summary>
        public virtual Transform GetTransformVariable(string key)
        {
            TransformVariable variable = GetVariable<TransformVariable>(key);

            if (variable != null)
            {
                return GetVariable<TransformVariable>(key).Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the value of a Transform variable.
        /// The variable must already be added to the list of variables for this Blackboard.
        /// </summary>
        public virtual void SetTransformVariable(string key, Transform value)
        {
            TransformVariable variable = GetVariable<TransformVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

    }
}
