using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Reserve space for given number of items in the collection
    /// </summary>
    [CommandInfo("Collection",
                 "Reserve",
                     "Reserve space for given number of items in the collection")]
    [Serializable]
    public class CollectionCommandReserve : CollectionBaseIntCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Reserve(integer.Value);
        }
    }
}