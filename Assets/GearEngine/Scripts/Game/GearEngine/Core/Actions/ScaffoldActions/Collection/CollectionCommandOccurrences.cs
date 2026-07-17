using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// How many occurrences of a given variable exist in a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Occurrences",
                     "How many occurrences of a given variable exist in a target collection")]
    [Serializable]
    public class CollectionCommandOccurrences : CollectionBaseVarAndIntCommand
    {
        protected override void OnEnterInner()
        {
            integer.Value = collection.Value.Occurrences(variableToUse);
        }
    }
}