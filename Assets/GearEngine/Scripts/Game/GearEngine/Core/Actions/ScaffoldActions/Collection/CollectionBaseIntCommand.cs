using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Base class for all ScaffoldCollection commands that use an intvar
    /// </summary>
    [Serializable]
    public abstract class CollectionBaseIntCommand : CollectionBaseCommand
    {
        [SerializeField]
        [Tooltip("The Integer")]
        protected IntegerData integer;

        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                OnEnterInner();
            }

            Continue();
        }

        protected abstract void OnEnterInner();

        public override bool HasReference(Variable variable)
        {
            return variable == integer.integerRef || base.HasReference(variable);
        }

        public override string GetSummary()
        {
            if (collection.Value == null)
            {
                return "Error: no collection selected";
            }

            return integer.Value.ToString() + " on " + collection.Value.name;
        }
    }
}