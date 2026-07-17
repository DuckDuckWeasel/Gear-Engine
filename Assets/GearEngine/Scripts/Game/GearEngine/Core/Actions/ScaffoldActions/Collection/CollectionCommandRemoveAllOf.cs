using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Remove all items in given rhs collection to target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Remove All Of",
                     "Remove all items in given rhs collection to target collection")]
    [Serializable]
    public class CollectionCommandRemoveAllOf : CollectionBaseTwoCollectionCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.RemoveAll(rhsCollection);
        }
    }
}