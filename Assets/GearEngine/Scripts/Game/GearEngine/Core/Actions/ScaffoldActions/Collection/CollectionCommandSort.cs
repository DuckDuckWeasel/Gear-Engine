using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Sort a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Sort",
                     "Sort a target collection")]
    [Serializable]
    public class CollectionCommandSort : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Sort();
            }

            Continue();
        }
    }
}