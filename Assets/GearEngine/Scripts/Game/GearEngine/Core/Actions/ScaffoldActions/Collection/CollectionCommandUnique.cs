using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Removes all duplicates.
    /// </summary>
    [CommandInfo("Collection",
                 "Unique",
                     "Removes all duplicates.")]
    [Serializable]
    public class CollectionCommandUnique : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Unique();
            }

            Continue();
        }
    }
}