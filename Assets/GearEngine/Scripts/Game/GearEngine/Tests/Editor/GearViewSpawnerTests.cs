using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearViewSpawnerTests
    {
        [Test]
        public void Spawn_ReturnsNull_WhenViewPrefabMissing()
        {
            GearItemData config = new GearItemData { Id = "x", ViewPrefab = null };
            Transform parent = new GameObject("Parent", typeof(RectTransform)).transform;
            LogAssert.Expect(LogType.Error, "[GearViewSpawner] Gear 'x' missing ViewPrefab.");

            GearView view = GearViewSpawner.Spawn(config, parent);

            Assert.IsNull(view);
            Object.DestroyImmediate(parent.gameObject);
        }

        [Test]
        public void Spawn_AppliesRelativeScaleMultiplierOnce()
        {
            GameObject gearVisualGo = new GameObject("GearVisual", typeof(RectTransform));
            GameObject root = new GameObject("PrefabRoot", typeof(RectTransform));
            gearVisualGo.transform.SetParent(root.transform, false);
            GearView gearView = root.AddComponent<GearView>();
            gearView.WireTestReferences(gearVisualGo.transform);

            GearView prefab = gearView;
            GearItemData config = new GearItemData
            {
                Id = "test",
                ViewPrefab = prefab,
                RelativeScaleMultiplier = 0.5f,
            };

            Transform parent = new GameObject("Slot", typeof(RectTransform)).transform;
            GearView instance = GearViewSpawner.Spawn(config, parent);

            Assert.IsNotNull(instance);
            Transform gv = instance.transform.GetChild(0);
            Assert.AreEqual(0.5f, gv.localScale.x, 0.001f);
            Assert.AreEqual(0.5f, gv.localScale.y, 0.001f);

            Object.DestroyImmediate(parent.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Spawn_ResetsLocalPositionAndRotation()
        {
            GameObject gearVisualGo = new GameObject("GearVisual", typeof(RectTransform));
            GameObject root = new GameObject("PrefabRoot", typeof(RectTransform));
            root.transform.localPosition = new Vector3(1, 2, 3);
            root.transform.localRotation = Quaternion.Euler(0, 0, 45);
            gearVisualGo.transform.SetParent(root.transform, false);
            GearView gearView = root.AddComponent<GearView>();
            gearView.WireTestReferences(gearVisualGo.transform);

            GearItemData config = new GearItemData { Id = "t2", ViewPrefab = gearView, RelativeScaleMultiplier = 1f };
            Transform parent = new GameObject("Slot", typeof(RectTransform)).transform;
            GearView instance = GearViewSpawner.Spawn(config, parent);

            Assert.IsNotNull(instance);
            Assert.AreEqual(Vector3.zero, instance.transform.localPosition);
            Assert.AreEqual(Quaternion.identity, instance.transform.localRotation);

            Object.DestroyImmediate(parent.gameObject);
            Object.DestroyImmediate(root);
        }
    }
}
