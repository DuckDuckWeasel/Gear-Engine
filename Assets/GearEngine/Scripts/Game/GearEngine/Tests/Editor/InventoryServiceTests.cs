using System;
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
        public void LoadInventory_AddsAllStartingItems()
        {
            var service = new InventoryService();
            service.Initialize(10, () => false);
            GearConfig g1 = CreateGearConfig("g1");
            GearConfig g2 = CreateGearConfig("g2");

            service.LoadInventory(new[] { g1.CreateRuntimeData(), g2.CreateRuntimeData() });

            Assert.AreEqual(2, service.Model.AvailableItems.Count);

            UnityEngine.Object.DestroyImmediate(g1);
            UnityEngine.Object.DestroyImmediate(g2);
        }

        [Test]
        public void LoadInventory_IgnoresNullEntries()
        {
            var service = new InventoryService();
            service.Initialize(10, () => false);
            GearConfig g1 = CreateGearConfig("g1");

            service.LoadInventory(new IItem[] { g1.CreateRuntimeData(), null });

            Assert.AreEqual(1, service.Model.AvailableItems.Count);

            UnityEngine.Object.DestroyImmediate(g1);
        }

        [Test]
        public void LoadInventory_ThrowsWhenNullEnumerable()
        {
            var service = new InventoryService();
            service.Initialize(10, () => false);

            Assert.Throws<ArgumentNullException>(() => service.LoadInventory(null));
        }

        [Test]
        public void NotifyItemDropped_RaisesOnItemDraggedToBoard()
        {
            var service = new InventoryService();
            service.Initialize(10, () => false);
            GearConfigData data = new GearConfigData { Id = "x", Category = GearCategory.Base };
            Vector3? receivedPos = null;
            IItem receivedItem = null;
            /*service.OnItemDraggedToBoard += (p, i) =>
            {
                receivedPos = p;
                receivedItem = i;
            };*/

            var world = new Vector3(1f, 2f, 3f);
            //service.NotifyItemDropped(world, data);

            Assert.IsTrue(receivedPos.HasValue);
            Assert.AreEqual(world, receivedPos.Value);
            Assert.AreSame(data, receivedItem);
        }

        [Test]
        public void ConsumeSpecificItem_RemovesMatchingItem()
        {
            var service = new InventoryService();
            service.Initialize(10, () => false);
            var data = new GearConfigData { Id = "rm", Category = GearCategory.Base };
            service.AddItem(data);

            service.ConsumeSpecificItem(data);

            Assert.AreEqual(0, service.Model.AvailableItems.Count);
        }

        [Test]
        public void ConsumeSpecificItem_LogsErrorWhenItemNotFound()
        {
            var service = new InventoryService();
            service.Initialize(10, () => false);
            var missing = new GearConfigData { Id = "nope", Category = GearCategory.Base };

            LogAssert.Expect(LogType.Error, "[InventoryService] ConsumeSpecificItem: item not found in inventory.");

            service.ConsumeSpecificItem(missing);
        }

        [Test]
        public void AddItem_RejectsWhenLimitReached()
        {
            var service = new InventoryService();
            service.Initialize(3, () => false);

            service.AddItem(new GearConfigData { Id = "g1", Category = GearCategory.Base });
            service.AddItem(new GearConfigData { Id = "g2", Category = GearCategory.Base });
            service.AddItem(new GearConfigData { Id = "g3", Category = GearCategory.Base });

            // 4th should be rejected
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"Inventory full"));
            service.AddItem(new GearConfigData { Id = "overflow", Category = GearCategory.Base });

            Assert.AreEqual(3, service.Model.AvailableItems.Count, "Inventory should be capped at 3.");
            Assert.AreEqual(3, service.CurrentCount);
            Assert.AreEqual(3, service.MaxSlots);
        }
    }
}
