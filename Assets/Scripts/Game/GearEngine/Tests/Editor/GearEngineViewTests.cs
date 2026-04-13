using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public class GearEngineViewTests
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

        [Test]
        public void HandleGearDraggedToBoard_OnSuccessfulPlacement_ConsumesInventory()
        {
            var gridManager = new GridManager();
            var eventController = new EventController();
            BoardConfigSO boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;

            var builder = new ContainerBuilder();
            builder.RegisterInstance(gridManager).As<IGridManager>();
            builder.RegisterInstance((IEventBus)eventController);
            builder.RegisterInstance(boardConfig);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            GearNodeFactory nodeFactory;
            using (IObjectResolver container = builder.Build())
            {
                nodeFactory = container.Resolve<GearNodeFactory>();
            }

            var boardVm = new BoardViewModel();
            boardVm.Initialize(new FakeEngine(), gridManager, nodeFactory, boardConfig);

            var inventoryVm = new GearInventoryViewModel();
            inventoryVm.Initialize(new FakeEngine());

            var gearData = new GearConfigData { Id = "bridge", Category = GearCategory.Base };
            inventoryVm.AddGearToInventory(gearData);

            Vector3 world = boardConfig.GetWorldPosition(new Vector2Int(3, 3));
            bool placed = boardVm.HandleInventoryDrop(world, gearData);
            Assert.IsTrue(placed);

            inventoryVm.ConsumeSpecificGear(gearData);
            Assert.AreEqual(0, inventoryVm.InventoryModel.AvailableGears.Count);

            Object.DestroyImmediate(boardConfig);
        }
    }
}
