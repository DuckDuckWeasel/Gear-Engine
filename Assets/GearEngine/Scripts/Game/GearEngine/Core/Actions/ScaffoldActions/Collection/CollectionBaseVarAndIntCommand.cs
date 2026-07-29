using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Base class for all ScaffoldCollection commands that require a compatible variable and an integer
    /// </summary>
    [Serializable]
    public abstract class CollectionBaseVarAndIntCommand : CollectionBaseVarCommand
    {
        [SerializeField]
        [VariableProperty(typeof(IntegerVariable))]
        [Tooltip("The Integer")]
        protected IntegerVariable integer;

        public override void OnEnter()
        {
            if (collection.Value != null && variableToUse != null && integer != null)
            {
                OnEnterInner();
            }

            Continue();
        }

        public override bool HasReference(Variable variable)
        {
            return variable == integer || base.HasReference(variable);
        }

        public override string GetSummary()
        {
            if (collection.Value == null)
            {
                return "Error: no collection selected";
            }

            if (variableToUse == null)
            {
                return "Error: no variable selected";
            }

            if (integer == null)
            {
                return "Error: no integer selected";
            }

            return integer.Key + " on " + variableToUse.Key + " in " + collection.Value.Name;
        }
    }
}