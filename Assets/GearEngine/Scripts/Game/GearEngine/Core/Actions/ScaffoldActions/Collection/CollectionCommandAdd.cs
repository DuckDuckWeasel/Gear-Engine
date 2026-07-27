using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Add an item to a collection
    /// </summary>
    [CommandInfo("Collection",
                 "Add",
                     "Add an item to a collection")]
    [Serializable]
    public class CollectionCommandAdd : CollectionBaseVarCommand
    {
        [Tooltip("Only add if the item does not already exist in the collection")]
        [SerializeField]
        protected BooleanData onlyIfUnique = new BooleanData(false);

        protected override void OnEnterInner()
        {
            if (onlyIfUnique.Value)
                collection.Value.AddUnique(variableToUse);
            else
                collection.Value.Add(variableToUse);
        }

        public override bool HasReference(Variable variable)
        {
            return onlyIfUnique.booleanRef == variable || base.HasReference(variable);
        }

        public override string GetSummary()
        {
            return base.GetSummary() + (onlyIfUnique.Value ? " Unique" : "");
        }
    }
}