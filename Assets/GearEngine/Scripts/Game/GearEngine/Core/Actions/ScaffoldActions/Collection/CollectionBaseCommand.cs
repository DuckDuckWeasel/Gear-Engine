using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Base class for all ScaffoldCollection commands
    /// </summary>
    [Serializable]
    public abstract class CollectionBaseCommand : ActionBase
    {
        [SerializeField]
        [Tooltip("The Collection")]
        protected CollectionData collection;

        public override Color GetButtonColor()
        {
            return new Color32(191, 217, 235, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return variable == collection.collectionRef;
        }

        public override string GetSummary()
        {
            if (collection.Value == null)
            {
                return "Error: no collection selected";
            }

            return collection.Value.Name;
        }
    }
}