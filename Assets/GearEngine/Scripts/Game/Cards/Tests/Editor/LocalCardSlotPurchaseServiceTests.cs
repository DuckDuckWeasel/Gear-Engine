using System;
using System.Collections.Generic;
using GearEngine.Cards;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Cards.Tests.Editor
{
    public sealed class LocalCardSlotPurchaseServiceTests
    {
        [Test]
        public void TryPurchaseSlot_WhenEnoughGold_CollectsAndDeductsCost()
        {
            var catalog = ScriptableObject.CreateInstance<CardCatalogSO>();
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            SetId(card, "card_a");
            SetCardsList(catalog, new List<CardDefinition> { card });

            var inventory = new PlayerCardInventoryState
            {
                Slots = new List<CardSlotSnapshot>
                {
                    new CardSlotSnapshot { SlotIndex = 0, State = CardSlotState.Uncollected, CardId = null },
                },
            };

            long gold = 500;
            var service = new LocalCardSlotPurchaseService(catalog, () => gold, v => gold = v);
            var rng = new System.Random(42);

            try
            {
                bool ok = service.TryPurchaseSlot(inventory, 0, rng, out string error);
                Assert.That(ok, Is.True);
                Assert.That(error, Is.Null);
                Assert.That(inventory.Slots[0].State, Is.EqualTo(CardSlotState.Collected));
                Assert.That(inventory.Slots[0].CardId, Is.EqualTo("card_a"));
                long expected = 500 - CardCostCurve.GoldCostForSlot(0);
                Assert.That(gold, Is.EqualTo(expected));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(card);
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void TryPurchaseSlot_WhenNotEnoughGold_FailsWithoutMutation()
        {
            var catalog = ScriptableObject.CreateInstance<CardCatalogSO>();
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            SetId(card, "card_a");
            SetCardsList(catalog, new List<CardDefinition> { card });

            var inventory = new PlayerCardInventoryState
            {
                Slots = new List<CardSlotSnapshot>
                {
                    new CardSlotSnapshot { SlotIndex = 0, State = CardSlotState.Uncollected, CardId = null },
                },
            };

            long gold = 10;
            var service = new LocalCardSlotPurchaseService(catalog, () => gold, v => gold = v);

            try
            {
                bool ok = service.TryPurchaseSlot(inventory, 0, new System.Random(1), out string error);
                Assert.That(ok, Is.False);
                Assert.That(error, Is.EqualTo("Not enough gold."));
                Assert.That(inventory.Slots[0].State, Is.EqualTo(CardSlotState.Uncollected));
                Assert.That(gold, Is.EqualTo(10));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(card);
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        private static void SetId(CardDefinition card, string id)
        {
            var so = new SerializedObject(card);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetCardsList(CardCatalogSO catalog, List<CardDefinition> list)
        {
            var so = new SerializedObject(catalog);
            SerializedProperty prop = so.FindProperty("cards");
            prop.ClearArray();
            for (var i = 0; i < list.Count; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
