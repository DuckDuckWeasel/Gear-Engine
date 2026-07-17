using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Clears a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Clear",
                     "Clears a target collection")]
    [Serializable]
    public class CollectionCommandClear : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Clear();
            }

            Continue();
        }
    }
}