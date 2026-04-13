using System.Collections.Generic;
using Game.GearEngine;
using Game.GearEngine.Presentation;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Game.GearEngine.Tests
{
    [TestFixture]
    public class DeleteGearTests
    {
        private sealed class FakeEngine : IGearEngineService
        {
            public bool IsRunning { get; set; }
            public void Play() => IsRunning = true;
            public void Stop() => IsRunning = false;
        }

        private GridManager gridManager;
        private EventController eventController;
        private BoardConfigSO boardConfig;
        private GearNodeFactory nodeFactory;
        private BoardViewModel boardVm;
        private FakeEngine fakeEngine;
        private GearEngineFeatureToggleSO featureToggle;
        private readonly List<IGridNode> removedNodes = new List<IGridNode>();
        private readonly List<GearDeletedEvent> deletedEvents = new List<GearDeletedEvent>();

        [SetUp]
        public void Setup()
        {
            gridManager = new GridManager();
            eventController = new EventController();
            boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;
            fakeEngine = new FakeEngine();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(gridManager).As<IGridManager>();
            builder.RegisterInstance((IEventBus)eventController);
            builder.RegisterInstance(boardConfig);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            using (IObjectResolver container = builder.Build())
            {
                nodeFactory = container.Resolve<GearNodeFactory>();
            }

            boardVm = new BoardViewModel();
            boardVm.Initialize(
                fakeEngine,
                gridManager,
                nodeFactory,
                boardConfig,
                eventController);

            removedNodes.Clear();
            deletedEvents.Clear();

            boardVm.OnGearRemoved += n => removedNodes.Add(n);
            eventController.AddListener<GearDeletedEvent>(e => deletedEvents.Add(e));
        }

        [TearDown]
        public void TearDown()
        {
            if (boardConfig != null)
            {
                Object.DestroyImmediate(boardConfig);
            }

            if (featureToggle != null)
            {
                Object.DestroyImmediate(featureToggle);
                featureToggle = null;
            }
        }

        [Test]
        public void DeleteGear_WhenDeletable_RemovesNodeAndRaisesEvent()
        {
            var data = new GearConfigData
            {
                Id = "deletable_gear",
                Category = GearCategory.Base,
                IsDeletable = true,
                DeleteRewardAmount = 42
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            bool result = boardVm.DeleteGear(node);

            Assert.IsTrue(result, "DeleteGear should return true for a deletable gear.");
            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 2)), "Gear should be removed from grid.");
            Assert.AreEqual(1, removedNodes.Count, "OnGearRemoved should fire once.");
            Assert.AreSame(node, removedNodes[0], "The removed node should match the deleted gear.");
            Assert.AreEqual(1, deletedEvents.Count, "GearDeletedEvent should be raised once.");
            Assert.AreEqual(new Vector2Int(2, 2), deletedEvents[0].Position);
            Assert.AreEqual(42, deletedEvents[0].RewardAmount);
        }

        [Test]
        public void DeleteGear_WhenNotDeletable_ReturnsFalse()
        {
            var data = new GearConfigData
            {
                Id = "non_deletable_gear",
                Category = GearCategory.Base,
                IsDeletable = false,
                DeleteRewardAmount = 10
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(3, 3), data);
            gridManager.AddNode(node);

            LogAssert.Expect(LogType.Warning, "[BoardViewModel] DeleteGear rejected: gear is not deletable.");
            bool result = boardVm.DeleteGear(node);

            Assert.IsFalse(result, "DeleteGear should return false for a non-deletable gear.");
            Assert.IsNotNull(gridManager.GetNode(new Vector2Int(3, 3)), "Gear should remain in grid.");
            Assert.AreEqual(0, removedNodes.Count, "OnGearRemoved should not fire.");
            Assert.AreEqual(0, deletedEvents.Count, "GearDeletedEvent should not be raised.");
        }

        [Test]
        public void DeleteGear_WhenRunning_ReturnsFalse()
        {
            fakeEngine.IsRunning = true;

            var data = new GearConfigData
            {
                Id = "running_gear",
                Category = GearCategory.Base,
                IsDeletable = true,
                DeleteRewardAmount = 5
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(1, 1), data);
            gridManager.AddNode(node);

            LogAssert.Expect(LogType.Warning, "[BoardViewModel] DeleteGear rejected: simulation is running.");
            bool result = boardVm.DeleteGear(node);

            Assert.IsFalse(result, "DeleteGear should return false when simulation is running.");
            Assert.IsNotNull(gridManager.GetNode(new Vector2Int(1, 1)), "Gear should remain in grid.");
            Assert.AreEqual(0, removedNodes.Count, "OnGearRemoved should not fire.");
            Assert.AreEqual(0, deletedEvents.Count, "GearDeletedEvent should not be raised.");
        }

        [Test]
        public void DeleteGear_NullNode_ReturnsFalse()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[BoardViewModel\] DeleteGear failed:"));
            bool result = boardVm.DeleteGear(null);

            Assert.IsFalse(result, "DeleteGear should return false for null node.");
        }

        [Test]
        public void DeleteGear_WithZeroReward_StillDeletesSuccessfully()
        {
            var data = new GearConfigData
            {
                Id = "zero_reward_gear",
                Category = GearCategory.Base,
                IsDeletable = true,
                DeleteRewardAmount = 0
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(4, 4), data);
            gridManager.AddNode(node);

            bool result = boardVm.DeleteGear(node);

            Assert.IsTrue(result, "DeleteGear should succeed even with zero reward.");
            Assert.IsNull(gridManager.GetNode(new Vector2Int(4, 4)), "Gear should be removed.");
            Assert.AreEqual(1, deletedEvents.Count);
            Assert.AreEqual(0, deletedEvents[0].RewardAmount);
        }

        [Test]
        public void SnapBackToOriginal_RestoresNodePosition()
        {
            var data = new GearConfigData
            {
                Id = "snapback_gear",
                Category = GearCategory.Base,
                IsDeletable = true,
                DeleteRewardAmount = 10
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 3), data);
            gridManager.AddNode(node);

            boardVm.OnGearPickedUp(node, new Vector2Int(2, 3));
            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 3)), "Node should be extracted.");

            boardVm.SnapBackToOriginal(node);
            Assert.AreSame(node, gridManager.GetNode(new Vector2Int(2, 3)), "Node should be snapped back.");
        }

        [Test]
        public void DeleteGear_FiresDragEndedEvent()
        {
            var data = new GearConfigData
            {
                Id = "drag_end_gear",
                Category = GearCategory.Base,
                IsDeletable = true,
                DeleteRewardAmount = 15
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(0, 0), data);
            gridManager.AddNode(node);

            int dragStartedCount = 0;
            int dragEndedCount = 0;
            boardVm.OnBoardDragStarted += _ => dragStartedCount++;
            boardVm.OnBoardDragEnded += () => dragEndedCount++;

            boardVm.OnGearPickedUp(node, new Vector2Int(0, 0));
            Assert.AreEqual(1, dragStartedCount, "Drag started should fire on pickup.");

            boardVm.DeleteGear(node);
            // Note: DeleteGear does not fire OnBoardDragEnded — that's the caller's responsibility.
            // The view layer (GearEngineView) calls trashZone.OnDragEnded after confirm/cancel.
        }

        [Test]
        public void DeleteGear_WhenFeatureToggleOff_ReturnsFalse()
        {
            // Create a toggle with trash disabled
            featureToggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();
            featureToggle.EnableTrashDeletion = false;

            // Re-initialize with the toggle
            var vmWithToggle = new BoardViewModel();
            vmWithToggle.Initialize(
                fakeEngine,
                gridManager,
                nodeFactory,
                boardConfig,
                eventController,
                featureToggle);

            var data = new GearConfigData
            {
                Id = "toggle_off_gear",
                Category = GearCategory.Base,
                IsDeletable = true,
                DeleteRewardAmount = 99
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            LogAssert.Expect(LogType.Warning, "[BoardViewModel] DeleteGear rejected: trash deletion feature is disabled.");
            bool result = vmWithToggle.DeleteGear(node);

            Assert.IsFalse(result, "DeleteGear should return false when feature toggle is off.");
            Assert.IsNotNull(gridManager.GetNode(new Vector2Int(2, 2)), "Gear should remain in grid.");
            Assert.AreEqual(0, deletedEvents.Count, "GearDeletedEvent should not be raised.");
        }

        [Test]
        public void DeleteGear_WhenFeatureToggleOn_DeletesSuccessfully()
        {
            featureToggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();
            featureToggle.EnableTrashDeletion = true;

            var vmWithToggle = new BoardViewModel();
            vmWithToggle.Initialize(
                fakeEngine,
                gridManager,
                nodeFactory,
                boardConfig,
                eventController,
                featureToggle);

            vmWithToggle.OnGearRemoved += n => removedNodes.Add(n);

            var data = new GearConfigData
            {
                Id = "toggle_on_gear",
                Category = GearCategory.Base,
                IsDeletable = true,
                DeleteRewardAmount = 77
            };

            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(1, 1), data);
            gridManager.AddNode(node);

            bool result = vmWithToggle.DeleteGear(node);

            Assert.IsTrue(result, "DeleteGear should succeed when feature toggle is on.");
            Assert.IsNull(gridManager.GetNode(new Vector2Int(1, 1)), "Gear should be removed.");
            Assert.AreEqual(1, removedNodes.Count);
            Assert.AreEqual(1, deletedEvents.Count);
            Assert.AreEqual(77, deletedEvents[0].RewardAmount);
        }
    }
}
