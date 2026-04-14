using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public class GearInventoryViewModelTests
    {
        private sealed class FakeEngine : IGearEngineService
        {
            public bool IsRunning => false;
            public void Play()
            {
            }

            public void Stop()
            {
            }
        }

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
        public void LoadInventory_AddsAllStartingGears()
        {
            var vm = new GearInventoryViewModel();
            vm.Initialize(new FakeEngine(), maxInventorySlots: 10);
            GearConfig g1 = CreateGearConfig("g1");
            GearConfig g2 = CreateGearConfig("g2");

            vm.LoadInventory(new[] { g1, g2 });

            Assert.AreEqual(2, vm.InventoryModel.AvailableGears.Count);

            UnityEngine.Object.DestroyImmediate(g1);
            UnityEngine.Object.DestroyImmediate(g2);
        }

        [Test]
        public void LoadInventory_IgnoresNullEntries()
        {
            var vm = new GearInventoryViewModel();
            vm.Initialize(new FakeEngine(), maxInventorySlots: 10);
            GearConfig g1 = CreateGearConfig("g1");

            vm.LoadInventory(new GearConfig[] { g1, null });

            Assert.AreEqual(1, vm.InventoryModel.AvailableGears.Count);

            UnityEngine.Object.DestroyImmediate(g1);
        }

        [Test]
        public void LoadInventory_ThrowsWhenNullEnumerable()
        {
            var vm = new GearInventoryViewModel();
            vm.Initialize(new FakeEngine(), maxInventorySlots: 10);

            Assert.Throws<ArgumentNullException>(() => vm.LoadInventory(null));
        }

        [Test]
        public void NotifyGearDropped_RaisesOnGearDraggedToBoard()
        {
            var vm = new GearInventoryViewModel();
            vm.Initialize(new FakeEngine(), maxInventorySlots: 10);
            GearConfigData data = new GearConfigData { Id = "x", Category = GearCategory.Base };
            Vector3? receivedPos = null;
            GearConfigData receivedGear = null;
            vm.OnGearDraggedToBoard += (p, g) =>
            {
                receivedPos = p;
                receivedGear = g;
            };

            var world = new Vector3(1f, 2f, 3f);
            vm.NotifyGearDropped(world, data);

            Assert.IsTrue(receivedPos.HasValue);
            Assert.AreEqual(world, receivedPos.Value);
            Assert.AreSame(data, receivedGear);
        }

        [Test]
        public void ConsumeSpecificGear_RemovesMatchingGear()
        {
            var vm = new GearInventoryViewModel();
            vm.Initialize(new FakeEngine(), maxInventorySlots: 10);
            var data = new GearConfigData { Id = "rm", Category = GearCategory.Base };
            vm.AddGearToInventory(data);

            vm.ConsumeSpecificGear(data);

            Assert.AreEqual(0, vm.InventoryModel.AvailableGears.Count);
        }

        [Test]
        public void ConsumeSpecificGear_LogsErrorWhenGearNotFound()
        {
            var vm = new GearInventoryViewModel();
            vm.Initialize(new FakeEngine(), maxInventorySlots: 10);
            var missing = new GearConfigData { Id = "nope", Category = GearCategory.Base };

            LogAssert.Expect(LogType.Error, "[GearInventoryViewModel] ConsumeSpecificGear: gear not found in inventory.");

            vm.ConsumeSpecificGear(missing);
        }

        [Test]
        public void AddGearToInventory_RejectsWhenLimitReached()
        {
            var vm = new GearInventoryViewModel();
            vm.Initialize(new FakeEngine(), maxInventorySlots: 3);

            vm.AddGearToInventory(new GearConfigData { Id = "g1", Category = GearCategory.Base });
            vm.AddGearToInventory(new GearConfigData { Id = "g2", Category = GearCategory.Base });
            vm.AddGearToInventory(new GearConfigData { Id = "g3", Category = GearCategory.Base });

            // 4th should be rejected
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"Inventory full"));
            vm.AddGearToInventory(new GearConfigData { Id = "overflow", Category = GearCategory.Base });

            Assert.AreEqual(3, vm.InventoryModel.AvailableGears.Count, "Inventory should be capped at 3.");
            Assert.AreEqual(3, vm.CurrentCount);
            Assert.AreEqual(3, vm.MaxSlots);
        }
    }
}
