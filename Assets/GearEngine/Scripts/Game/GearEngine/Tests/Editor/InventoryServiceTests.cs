using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public class InventoryServiceTests
    {
        private static GearConfig CreateGearConfig(string id)
        {
            var gc = ScriptableObject.CreateInstance<GearConfig>();
            var so = new UnityEditor.SerializedObject(gc);
            var dp = so.FindProperty("data");
            Assert.IsNotNull(dp);
            dp.FindPropertyRelative("Id").stringValue = id;
            dp.FindPropertyRelative("Category").enumValueIndex = (int)GearCategory.Base;
            so.ApplyModifiedProperties();
            return gc;
        }

        [Test]
        public void Constructor_SeedsItemsFromLoadout()
        {
            GearConfig a = CreateGearConfig("a");
            GearConfig b = CreateGearConfig("b");
            var loadout = GearInventoryLoadoutData.FromGearConfigs(10, new[] { a, b });

            var service = new InventoryService(loadout);

            Assert.AreEqual(2, service.GetInventory().Items.Count);
            Assert.AreEqual("a", service.GetInventory().Items[0].Id);
            Assert.AreEqual("b", service.GetInventory().Items[1].Id);
            Assert.AreEqual(10, service.GetInventory().MaxSlots);

            UnityEngine.Object.DestroyImmediate(a);
            UnityEngine.Object.DestroyImmediate(b);
        }

        [Test]
        public void TryConsume_RemovesReferenceFromInventory()
        {
            GearConfig cfg = CreateGearConfig("x");
            var loadout = GearInventoryLoadoutData.FromGearConfigs(5, new[] { cfg });
            var service = new InventoryService(loadout);
            IItem item = service.GetInventory().Items[0];

            Assert.IsTrue(service.TryConsume(item));
            Assert.AreEqual(0, service.GetInventory().Items.Count);

            UnityEngine.Object.DestroyImmediate(cfg);
        }

        [Test]
        public void TryAdd_RespectsMaxSlots()
        {
            var loadout = GearInventoryLoadoutData.Empty(maxSlots: 1);
            var service = new InventoryService(loadout);

            Assert.IsTrue(service.TryAdd(new GearConfigData { Id = "one" }));
            LogAssert.Expect(LogType.Warning, "[InventoryService] Inventory full (1/1). Cannot add item 'two'.");
            Assert.IsFalse(service.TryAdd(new GearConfigData { Id = "two" }));
        }
    }
}
