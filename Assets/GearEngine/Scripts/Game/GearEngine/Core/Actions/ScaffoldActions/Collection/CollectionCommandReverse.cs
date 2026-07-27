using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Reverse the current order of a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Reverse",
                     "Reverse the current order of a target collection")]
    [Serializable]
    public class CollectionCommandReverse : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Reverse();
            }

            Continue();
        }
    }
}