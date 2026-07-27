using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Remove all items from collection that aren't also in RHS, similar to an overlap.
    /// </summary>
    [CommandInfo("Collection",
                 "Intersection",
                     "Remove all items from collection that aren't also in RHS, similar to an overlap.")]
    [Serializable]
    public class CollectionCommandIntersection : CollectionBaseTwoCollectionCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Intersection(rhsCollection.Value);
        }
    }
}