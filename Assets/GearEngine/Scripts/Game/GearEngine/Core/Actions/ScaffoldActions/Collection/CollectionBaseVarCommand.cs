using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Base class for all ScaffoldCollection commands that require a compatible variable type
    /// </summary>
    [Serializable]
    public abstract class CollectionBaseVarCommand : CollectionBaseCommand, ICollectionCompatible
    {
        [SerializeField]
        [VariableProperty(compatibleVariableName = "collection")]
        [Tooltip("The Variable to use")]
        protected Variable variableToUse;

        public override void OnEnter()
        {
            if (collection.Value != null && variableToUse != null)
            {
                OnEnterInner();
            }

            Continue();
        }

        protected abstract void OnEnterInner();

        public override bool HasReference(Variable variable)
        {
            return variable == variableToUse || base.HasReference(variable);
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

            return variableToUse.Key + " to " + collection.Value.Name;
        }

        bool ICollectionCompatible.IsVarCompatibleWithCollection(Variable variable, string compatibleWith)
        {
            if (compatibleWith == "collection")
            {
                return collection.Value == null ? false : collection.Value.IsElementCompatible(variable);
            }
            else
            {
                return true;
            }
        }
    }
}