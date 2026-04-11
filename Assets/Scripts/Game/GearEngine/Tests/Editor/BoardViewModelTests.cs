using Game.GearEngine;
using Game.GearEngine.Presentation;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using UnityEngine;
using VContainer;

namespace Game.GearEngine.Tests
{
    [TestFixture]
    public class BoardViewModelTests
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

        private GridManager gridManager;
        private EventController eventController;
        private BoardConfigSO boardConfig;
        private GearInventoryViewModel inventory;
        private GearViewFactory viewFactory;
        private GearNodeFactory nodeFactory;
        private BoardViewModel boardVm;
        private GameObject boardRoot;

        [SetUp]
        public void Setup()
        {
            gridManager = new GridManager();
            eventController = new EventController();
            boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;

            inventory = new GearInventoryViewModel();
            inventory.Initialize(new FakeEngine());

            var builder = new ContainerBuilder();
            builder.RegisterInstance(gridManager).As<IGridManager>();
            builder.RegisterInstance((IEventBus)eventController);
            builder.RegisterInstance(boardConfig);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            builder.Register<GearViewFactory>(Lifetime.Singleton);
            var container = builder.Build();

            nodeFactory = container.Resolve<GearNodeFactory>();
            viewFactory = container.Resolve<GearViewFactory>();

            boardVm = new BoardViewModel();
            boardVm.Initialize(
                new FakeEngine(),
                gridManager,
                nodeFactory,
                viewFactory,
                inventory,
                boardConfig,
                eventController);

            boardRoot = new GameObject("BoardRoot_Test");
            boardVm.SetBoardVisualRoot(boardRoot.transform);
        }

        [TearDown]
        public void TearDown()
        {
            boardVm?.Dispose();
            if (boardRoot != null)
            {
                Object.DestroyImmediate(boardRoot);
            }

            if (boardConfig != null)
            {
                Object.DestroyImmediate(boardConfig);
            }
        }

        [Test]
        public void OnGearPickedUp_ExtractsNodeFromGrid()
        {
            var data = new GearConfigData { Id = "pickup_test", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            boardVm.OnGearPickedUp(node, new Vector2Int(2, 2));

            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 2)));
        }

        [Test]
        public void OnGearDropped_EmptySlot_AddsNodeAtNewPosition()
        {
            var data = new GearConfigData { Id = "move_test", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            boardVm.OnGearPickedUp(node, new Vector2Int(2, 2));
            boardVm.OnGearDropped(node, new Vector2Int(1, 1), false);

            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 2)));
            Assert.AreSame(node, gridManager.GetNode(new Vector2Int(1, 1)));
        }

        [Test]
        public void OnGearDropped_OutOfBounds_SnapsBackToOriginalPosition()
        {
            var data = new GearConfigData { Id = "bounds_test", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            boardVm.OnGearPickedUp(node, new Vector2Int(2, 2));
            boardVm.OnGearDropped(node, new Vector2Int(99, 99), false);

            Assert.AreSame(node, gridManager.GetNode(new Vector2Int(2, 2)));
        }

        [Test]
        public void OnGearDropped_OverUI_ReturnsGearToInventory()
        {
            var data = new GearConfigData { Id = "ui_return_test", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);
            viewFactory.CreateView(node, data, boardRoot.transform);

            boardVm.OnGearPickedUp(node, new Vector2Int(2, 2));
            boardVm.OnGearDropped(node, new Vector2Int(1, 1), true);

            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 2)));
            Assert.IsTrue(inventory.InventoryModel.AvailableGears.Contains(data));
        }

        [Test]
        public void OnGearDropped_OccupiedSlot_SameId_WithNextLevel_MergesGear()
        {
            var nextLvl = ScriptableObject.CreateInstance<GearConfig>();
            var nextData = new GearConfigData { Id = "merged_lvl2", Category = GearCategory.Base };
            var so = new UnityEditor.SerializedObject(nextLvl);
            var dataProp = so.FindProperty("data");
            Assert.IsNotNull(dataProp, "GearConfig.data is required for CreateRuntimeData in merge test.");
            dataProp.FindPropertyRelative("Id").stringValue = nextData.Id;
            dataProp.FindPropertyRelative("Category").enumValueIndex = (int)GearCategory.Base;
            so.ApplyModifiedProperties();

            var occupantData = new GearConfigData
            {
                Id = "merge_me",
                Category = GearCategory.Base,
                NextLevelConfig = nextLvl
            };

            var draggedData = new GearConfigData
            {
                Id = "merge_me",
                Category = GearCategory.Base
            };

            var occupant = new BaseGearNode(gridManager, eventController);
            occupant.Initialize(new Vector2Int(1, 1), occupantData);
            gridManager.AddNode(occupant);
            viewFactory.CreateView(occupant, occupantData, boardRoot.transform);

            var dragged = new BaseGearNode(gridManager, eventController);
            dragged.Initialize(new Vector2Int(2, 2), draggedData);
            gridManager.AddNode(dragged);
            viewFactory.CreateView(dragged, draggedData, boardRoot.transform);

            boardVm.OnGearPickedUp(dragged, new Vector2Int(2, 2));
            boardVm.OnGearDropped(dragged, new Vector2Int(1, 1), false);

            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 2)));
            IGridNode atTarget = gridManager.GetNode(new Vector2Int(1, 1));
            Assert.IsNotNull(atTarget);
            Assert.AreNotSame(occupant, atTarget);
            Assert.AreNotSame(dragged, atTarget);
            Assert.AreEqual("merged_lvl2", ((NodeBase)atTarget).ConfigData.Id);

            Object.DestroyImmediate(nextLvl);
        }
    }
}
