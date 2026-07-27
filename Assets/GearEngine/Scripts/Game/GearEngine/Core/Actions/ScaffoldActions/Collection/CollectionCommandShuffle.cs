using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Randomly reorders all elements of a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Shuffle",
                     "Randomly reorders all elements of a target collection")]
    [Serializable]
    public class CollectionCommandShuffle : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Shuffle();
            }

            Continue();
        }
    }
}