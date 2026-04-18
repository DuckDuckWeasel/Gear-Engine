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
        public void Initialize_ClearsExistingItemsThenLoadsStartingGears()
        {
            var service = new InventoryService();
            service.Initialize(10, Array.Empty<GearConfig>());
            service.AddItem(new GearConfigData { Id = "stale" });

            GearConfig extra = CreateGearConfig("fromInit");
            service.Initialize(10, new[] { extra });

            Assert.AreEqual(1, service.CurrentCount);
            Assert.AreEqual("fromInit", service.Model.AvailableItems[0].Id);
        }

        [Test]
        public void Initialize_SecondCallClearsAndUpdatesMaxSlots()
        {
            var service = new InventoryService();
            service.Initialize(5, Array.Empty<GearConfig>());
            service.AddItem(new GearConfigData { Id = "a" });
            service.Initialize(12, Array.Empty<GearConfig>());

            Assert.AreEqual(12, service.MaxSlots);
            Assert.AreEqual(0, service.CurrentCount);
        }
    }
}
