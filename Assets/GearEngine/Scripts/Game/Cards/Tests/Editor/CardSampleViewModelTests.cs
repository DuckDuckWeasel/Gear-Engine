using System.Collections.Generic;
using GearEngine.Cards;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Cards.Tests.Editor
{
    public sealed class CardSampleViewModelTests
    {
        [Test]
        public void TryPurchaseSlot_WhenSlotUncollected_CollectsCardAndDeductsGold()
        {
            var catalog = ScriptableObject.CreateInstance<CardCatalogSO>();
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            SetId(card, "card_sample");
            SetCardsList(catalog, new List<CardDefinition> { card });

            try
            {
                var viewModel = new CardSampleViewModel(catalog);
                viewModel.TryPurchaseSlot(0);

                Assert.That(viewModel.Slots[0].State, Is.EqualTo(CardSlotState.Collected));
                Assert.That(viewModel.Slots[0].CardId, Is.EqualTo("card_sample"));
                long expectedGold = 1000 - CardCostCurve.GoldCostForSlot(0);
                Assert.That(viewModel.Gold, Is.EqualTo(expectedGold));
            }
            finally
            {
                Object.DestroyImmediate(card);
                Object.DestroyImmediate(catalog);
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
