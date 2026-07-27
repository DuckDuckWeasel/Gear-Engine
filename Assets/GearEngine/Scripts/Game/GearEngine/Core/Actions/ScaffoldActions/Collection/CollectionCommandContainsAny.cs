using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Does target collection, contain any of the items in the rhs collection items
    /// </summary>
    [CommandInfo("Collection",
                 "Contains Any Of",
                     "Does target collection, contain any of the items in the rhs collection items")]
    [Serializable]
    public class CollectionCommandContainsAny : CollectionBaseTwoCollectionCommand
    {
        [VariableProperty(typeof(BooleanVariable))]
        [Tooltip("The Result")]
        [SerializeField] protected BooleanVariable result;

        protected override void OnEnterInner()
        {
            if (result == null)
            {
                Debug.LogWarning("No result var set");
            }
            else
            {
                result.Value = collection.Value.ContainsAnyOf(rhsCollection.Value);
            }
        }

        public override bool HasReference(Variable variable)
        {
            return result == variable || base.HasReference(variable);
        }
    }
}