using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearViewSpawnerTests
    {
        [Test]
        public void Spawn_ReturnsNull_WhenViewPrefabMissing()
        {
            var config = new GearConfigData { Id = "x", ViewPrefab = null };
            var parent = new GameObject("Parent").transform;

            GearView view = GearViewSpawner.Spawn(config, parent);

            Assert.IsNull(view);
            Object.DestroyImmediate(parent.gameObject);
        }

        [Test]
        public void Spawn_AppliesRelativeScaleMultiplierOnce()
        {
            var gearVisualGo = new GameObject("GearVisual");
            var root = new GameObject("PrefabRoot");
            gearVisualGo.transform.SetParent(root.transform, false);
            var gearView = root.AddComponent<GearView>();
            gearView.WireTestReferences(gearVisualGo.transform);

            var prefab = gearView;
            var config = new GearConfigData
            {
                Id = "test",
                ViewPrefab = prefab,
                RelativeScaleMultiplier = 0.5f,
            };

            var parent = new GameObject("Slot").transform;
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
            var gearVisualGo = new GameObject("GearVisual");
            var root = new GameObject("PrefabRoot");
            root.transform.localPosition = new Vector3(1, 2, 3);
            root.transform.localRotation = Quaternion.Euler(0, 0, 45);
            gearVisualGo.transform.SetParent(root.transform, false);
            var gearView = root.AddComponent<GearView>();
            gearView.WireTestReferences(gearVisualGo.transform);

            var config = new GearConfigData { Id = "t2", ViewPrefab = gearView, RelativeScaleMultiplier = 1f };
            var parent = new GameObject("Slot").transform;
            GearView instance = GearViewSpawner.Spawn(config, parent);

            Assert.IsNotNull(instance);
            Assert.AreEqual(Vector3.zero, instance.transform.localPosition);
            Assert.AreEqual(Quaternion.identity, instance.transform.localRotation);

            Object.DestroyImmediate(parent.gameObject);
            Object.DestroyImmediate(root);
        }
    }
}
