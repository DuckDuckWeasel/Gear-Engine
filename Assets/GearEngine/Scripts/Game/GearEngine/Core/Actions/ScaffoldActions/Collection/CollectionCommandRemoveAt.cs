using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Remove item at given index
    /// </summary>
    [CommandInfo("Collection",
                 "Remove At",
                     "Remove item at given index")]
    [Serializable]
    public class CollectionCommandRemoveAt : CollectionBaseIntCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.RemoveAt(integer.Value);
        }
    }
}