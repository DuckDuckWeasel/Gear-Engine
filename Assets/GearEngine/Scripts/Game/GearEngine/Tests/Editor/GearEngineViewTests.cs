using System;
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

        private sealed class FakeDragService : IDragService
        {
            public bool IsDragging { get; private set; }
            private object dragData;

            public event Action<object> OnDragStarted;
            public event Action OnDragEnded;

            public T GetDragData<T>() where T : class => dragData as T;

            public void StartDrag(object data)
            {
                dragData = data;
                IsDragging = true;
                OnDragStarted?.Invoke(data);
            }

            public void EndDrag()
            {
                dragData = null;
                IsDragging = false;
                OnDragEnded?.Invoke();
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
            boardConfig.MaxAllowedBoardGears = 10;

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
            boardVm.Initialize(new FakeEngine(), gridManager, nodeFactory, boardConfig, dragService: new FakeDragService());

            var inventoryService = new GearEngine.Services.Inventory.InventoryService();
            inventoryService.Initialize(10, () => false);

            var gearData = new GearConfigData { Id = "bridge", Category = GearCategory.Base };
            inventoryService.AddItem(gearData);

            Vector2Int pos = new Vector2Int(3, 3);
            bool placed = boardVm.HandleInventoryDrop(pos, gearData);
            Assert.IsTrue(placed);

            inventoryService.ConsumeSpecificItem(gearData);
            Assert.AreEqual(0, inventoryService.Model.AvailableItems.Count);

            UnityEngine.Object.DestroyImmediate(boardConfig);
        }
    }
}
