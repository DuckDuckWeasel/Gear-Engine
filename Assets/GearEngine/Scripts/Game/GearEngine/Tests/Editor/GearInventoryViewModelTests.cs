using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services.Inventory;
using NUnit.Framework;

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

            public void ResetGridSimulationState()
            {
            }
        }

        [Test]
        public void Constructor_PassesMaxSlotsToInventoryService()
        {
            var inventory = new InventoryService();
            var engine = new FakeEngine();
            var vm = new GearInventoryViewModel(7, Array.Empty<GearConfig>(), engine, inventory, dragService: null);

            Assert.AreEqual(7, inventory.MaxSlots);
            Assert.AreEqual(7, vm.MaxSlots);
        }
    }
}
