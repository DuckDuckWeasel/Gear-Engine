using GearEngine.Campaign.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class CampaignScreenReferenceTests
    {
        [Test]
        public void RaceBoardLayout_ReservesHudSpaceAndRestoresSharedBoard()
        {
            GameObject root = new GameObject("RaceLayoutTest");
            root.SetActive(false);
            GameObject boardObject = new GameObject("Board", typeof(RectTransform),
                typeof(GearEngine.Presentation.UI.BoardViewComponent));
            boardObject.transform.SetParent(root.transform);
            GearEngine.Presentation.UI.BoardView shared = root.AddComponent<GearEngine.Presentation.UI.BoardView>();
            SerializedObject sharedData = new SerializedObject(shared);
            sharedData.FindProperty("board").objectReferenceValue =
                boardObject.GetComponent<GearEngine.Presentation.UI.BoardViewComponent>();
            sharedData.ApplyModifiedPropertiesWithoutUndo();
            Presentation.ActiveRaceView view = root.AddComponent<Presentation.ActiveRaceView>();
            SerializedObject viewData = new SerializedObject(view);
            viewData.FindProperty("board").objectReferenceValue = shared;
            viewData.ApplyModifiedPropertiesWithoutUndo();
            RectTransform rect = (RectTransform)boardObject.transform;
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.4f);
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic;
            try
            {
                typeof(Presentation.ActiveRaceView).GetMethod("ApplyRaceBoardLayout", flags).Invoke(view, null);
                Assert.That(rect.anchorMin.y, Is.GreaterThanOrEqualTo(0.2f));
                typeof(Presentation.ActiveRaceView).GetMethod("ApplyRaceBoardLayout", flags).Invoke(view, null);
                typeof(Presentation.ActiveRaceView).GetMethod("RestoreBoardLayout", flags).Invoke(view, null);
                Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(0.1f, 0.1f)));
                Assert.That(rect.anchorMax, Is.EqualTo(new Vector2(0.9f, 0.4f)));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase("Campaign_ResultPopupView", "ResultPopupView")]
        [TestCase("PFB_ReceivedRewardsView", "ReceivedRewardsView")]
        [TestCase("PFB_RaceProgressView", "RaceProgressView")]
        public void PostRacePrefab_HasWiredScreenReferences(string name, string viewType)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/GearEngine/Prefabs/Campaign/{name}.prefab");
            Component view = System.Array.Find(prefab.GetComponents<Component>(), c => c.GetType().Name == viewType);
            Assert.That(view, Is.Not.Null);
            SerializedObject data = new SerializedObject(view);
            SerializedProperty property = data.GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Assert.That(property.objectReferenceValue, Is.Not.Null, $"{name}: {property.propertyPath}");
                }
            }
        }

        [TestCase("Main View")]
        [TestCase("Setup View")]
        [TestCase("Race View")]
        [TestCase("Campaign_ResultPopupView")]
        [TestCase("PFB_ReceivedRewardsView")]
        [TestCase("PFB_RaceProgressView")]
        [TestCase("Campaign_RoguelikeView")]
        [TestCase("Items View")]
        [TestCase("ItemPopup View")]
        public void ScreenPrefab_HasNoMissingScripts(string name)
        {
            string path = $"Assets/GearEngine/Prefabs/Campaign/{name}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                Assert.That(missing, Is.Zero, $"{path}: {child.name} has missing scripts.");
            }
        }
    }
}
