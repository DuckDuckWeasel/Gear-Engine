using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Use the collection as a source of random items and turn it into a random bag. Drawing the 
    /// next random item until out of items and then reshuffling them.
    /// </summary>
    [CommandInfo("Collection",
                 "RandomBag",
                     "Use the collection as a source of random items and turn it into a random bag. " +
                         "Drawing the next random item until out of items and then reshuffling them.")]
    [Serializable]
    public class CollectionRandomBag : CollectionBaseVarCommand
    {
        [SerializeField]
        [Tooltip("Will add this many copies to the bag. If you want 5 of everything, you want 4 copies.")]
        protected IntegerData duplicatesToPutInBag = new IntegerData(0);

        [SerializeField]
        [Tooltip("The Current index")]
        protected IntegerData currentIndex = new IntegerData(int.MaxValue);

        [Tooltip("The Is init")]
        protected bool isInit = false;

        protected override void OnEnterInner()
        {
            if (!isInit)
            {
                Init();
            }

            currentIndex.Value++;

            if (currentIndex.Value >= collection.Value.Count)
            {
                Reshuffle();
            }

            collection.Value.Get(currentIndex.Value, ref variableToUse);
        }

        protected void Init()
        {
            int startingCount = collection.Value.Count;
            for (int i = 0; i < duplicatesToPutInBag.Value; i++)
            {
                for (int j = 0; j < startingCount; j++)
                {
                    collection.Value.Add(collection.Value.Get(j));
                }
            }

            //force invalid index
            currentIndex.Value = collection.Value.Count;

            isInit = true;
        }

        protected void Reshuffle()
        {
            currentIndex.Value = 0;
            collection.Value.Shuffle();
        }

        public override bool HasReference(Variable variable)
        {
            return base.HasReference(variable) ||
                duplicatesToPutInBag.integerRef == variable ||
                currentIndex.integerRef == variable;
        }

        public override string GetSummary()
        {
            return base.GetSummary() +
                (duplicatesToPutInBag.integerRef != null ? " " + duplicatesToPutInBag.integerRef.Key : "") +
            (currentIndex.integerRef != null ? " " + currentIndex.integerRef.Key : ""); ;
        }
    }
}
