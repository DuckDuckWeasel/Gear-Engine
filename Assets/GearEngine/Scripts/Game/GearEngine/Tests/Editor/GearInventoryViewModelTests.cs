using GearEngine.GearEngine.Nodes;
using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services.Inventory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearInventoryViewModelTests
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

            public System.Collections.Generic.IEnumerable<IGridNode> GetAllNodes() => new System.Collections.Generic.List<IGridNode>();
            public void ResetGridSimulationState()
            {
            }
        }

        [Test]
        public void Constructor_BindsToExistingInventoryModel_FromService()
        {
            GearConfig cfg = CreateGearConfig("seed");
            var loadout = GearInventoryLoadoutData.FromGearConfigs(7, new[] { cfg });
            var inventory = new InventoryService(loadout);
            var engine = new FakeEngine();

            var vm = new GearInventoryViewModel(engine, inventory);

            Assert.AreEqual(7, vm.MaxSlots);
            Assert.AreSame(inventory.GetInventory(), vm.InventoryModel);
            Assert.AreEqual(1, vm.InventoryModel.Items.Count);

            UnityEngine.Object.DestroyImmediate(cfg);
        }

        [Test]
        public void RecreatingViewModel_DoesNotResetInventoryOwnedByService()
        {
            var loadout = GearInventoryLoadoutData.FromGearConfigs(10, Array.Empty<GearConfig>());
            var inventory = new InventoryService(loadout);
            var engine = new FakeEngine();

            inventory.TryAdd(new GearConfigData { Id = "persist" });

            _ = new GearInventoryViewModel(engine, inventory);

            var second = new GearInventoryViewModel(engine, inventory);

            Assert.AreEqual(1, second.InventoryModel.Items.Count);
            Assert.AreEqual("persist", second.InventoryModel.Items[0].Id);
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
    }
}
