using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Use the collection as a source of random selection. Picking a random item each run.
    /// </summary>
    [CommandInfo("Collection",
                 "RandomItem",
                     "Use the collection as a source of random selection. Picking a random item each run.")]
    [Serializable]
    public class CollectionRandom : CollectionBaseVarCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Get(UnityEngine.Random.Range(0, collection.Value.Count - 1), ref variableToUse);
        }
    }
}