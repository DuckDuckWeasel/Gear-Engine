using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Clears target and then adds all of rhs to target.
    /// </summary>
    [CommandInfo("Collection",
                 "Copy",
                     "Clears target and then adds all of rhs to target.")]
    [Serializable]
    public class CollectionCommandCopy : CollectionBaseTwoCollectionCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.CopyFrom(rhsCollection.Value);
        }
    }
}