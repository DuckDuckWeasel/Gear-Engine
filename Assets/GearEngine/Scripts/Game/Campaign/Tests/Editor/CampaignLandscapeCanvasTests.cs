using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class CampaignLandscapeCanvasTests
    {
        [TestCase("ItemPopup View")]
        [TestCase("Campaign_RoguelikeView")]
        public void CampaignPopup_Landscape_UsesAspectSafeCanvasScaling(string prefabName)
        {
            string path = $"Assets/GearEngine/Prefabs/Campaign/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.That(prefab, Is.Not.Null, path);
            CanvasScaler scaler = prefab.GetComponent<CanvasScaler>();
            Assert.That(scaler, Is.Not.Null, path);
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.Expand),
                $"{prefabName} must fit its portrait canvas inside a 16:9 viewport.");
        }
    }
}
