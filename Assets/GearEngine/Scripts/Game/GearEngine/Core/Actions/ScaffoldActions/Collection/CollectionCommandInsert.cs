using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Add at a specific location in the collection
    /// </summary>
    [CommandInfo("Collection",
                 "Insert",
                     "Add at a specific location in the collection")]
    [Serializable]
    public class CollectionCommandInsert : CollectionBaseVarAndIntCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Insert(integer.Value, variableToUse);
        }
    }
}