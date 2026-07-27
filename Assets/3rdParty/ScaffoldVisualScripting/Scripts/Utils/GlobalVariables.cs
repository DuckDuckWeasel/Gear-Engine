
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Scaffold
{
    /// <summary>
    /// Storage for a collection of scaffold variables that can then be accessed globally.
    /// </summary>
    public class GlobalVariables : MonoBehaviour
    {
        private Blackboard holder;
        private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

        void Awake()
        {
            holder = new GameObject("GlobalVariables").AddComponent<Blackboard>();
            holder.transform.parent = transform;
        }

		public Variable GetVariable(string variableKey)
		{
			Variable v = null;
			variables.TryGetValue(variableKey, out v);
			return v;
		}

        public VariableBase<T> GetOrAddVariable<T>(string variableKey, T defaultvalue, Type type)
        {
            Variable v = null;
            VariableBase<T> vAsT = null;
            bool res = variables.TryGetValue(variableKey, out v);

            if(res && v != null)
            {
                vAsT = v as VariableBase<T>;

                if (vAsT != null)
                {
                    return vAsT;
                }
                else
                {
                    Debug.LogError("A scaffold variable of name " + variableKey + " already exists, but of a different type");
                }
            }
            else
            {
                //create the variable
                vAsT = holder.gameObject.AddComponent(type) as VariableBase<T>;
                vAsT.Value = defaultvalue;
                vAsT.Key = variableKey;
                vAsT.Scope = VariableScope.Public;
                variables[variableKey] = vAsT;
                holder.Variables.Add(vAsT);
            }

            return vAsT;
        }
    }
}