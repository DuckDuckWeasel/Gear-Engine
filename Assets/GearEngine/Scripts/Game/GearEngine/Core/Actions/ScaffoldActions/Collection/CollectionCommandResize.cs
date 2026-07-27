using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Resize will grow the collection to be the given size, will not remove items to shrink
    /// </summary>
    [CommandInfo("Collection",
                 "Resize",
                     "Resize will grow the collection to be the given size, will not remove items to shrink")]
    [Serializable]
    public class CollectionCommandResize : CollectionBaseIntCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Resize(integer.Value);
        }
    }
}