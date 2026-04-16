using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace GearEngine.GearEngine.Tests.Editor
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

        private GridManager gridManager;
        private EventController eventController;
        private BoardConfigSO boardConfig;
        private GearNodeFactory nodeFactory;
        private sealed class FakeSwapService
        {
            public void SwapNodes(IGridNode a, IGridNode b)
            {
                Vector2Int posA = a.Position;
                a.SetPosition(b.Position);
                b.SetPosition(posA);
            }
        }

        private sealed class FakeMergeService : IGridMergeService
        {
            public bool TryMerge(Vector2Int posA, Vector2Int posB)
            {
                return false;
            }

            public IGridNode MergeNodes(IGridNode dragged, IGridNode occupant, Vector2Int dropPos)
            {
                // Simple stub: ignores actual rules and just uses occupant
                return occupant;
            }
        }

        private BoardViewModel boardVm;
        private FakeDragService fakeDragService;
        private FakeSwapService fakeSwapService;
        private FakeMergeService fakeMergeService;
        private int placedCount;
        private int removedCount;
        private readonly List<IGridNode> placedNodes = new List<IGridNode>();
        private readonly List<IGridNode> removedNodes = new List<IGridNode>();

        [SetUp]
        public void Setup()
        {
            gridManager = new GridManager();
            eventController = new EventController();
            boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;
            boardConfig.MaxAllowedBoardGears = 5;
            fakeDragService = new FakeDragService();
            fakeSwapService = new FakeSwapService();
            fakeMergeService = new FakeMergeService();

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
            /*boardVm.Initialize(
                new FakeEngine(),
                gridManager,
                nodeFactory,
                boardConfig,
                dragService: fakeDragService,
                swapService: fakeSwapService,
                mergeService: fakeMergeService);
*/
            placedCount = 0;
            removedCount = 0;
            placedNodes.Clear();
            removedNodes.Clear();
            boardVm.OnGearPlaced += n =>
            {
                placedCount++;
                placedNodes.Add(n);
            };

            boardVm.OnGearRemoved += n =>
            {
                removedCount++;
                removedNodes.Add(n);
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (boardConfig != null)
            {
                UnityEngine.Object.DestroyImmediate(boardConfig);
            }
        }

        [Test]
        public void GetCurrentNodes_ReturnsExistingGridNodes()
        {
            var d1 = new GearConfigData { Id = "a", Category = GearCategory.Base };
            var d2 = new GearConfigData { Id = "b", Category = GearCategory.Base };
            var n1 = new BaseGearNode(gridManager, eventController);
            n1.Initialize(new Vector2Int(0, 0), d1);
            var n2 = new BaseGearNode(gridManager, eventController);
            n2.Initialize(new Vector2Int(1, 0), d2);
            gridManager.AddNode(n1);
            gridManager.AddNode(n2);

            int count = 0;
            foreach (IGridNode _ in boardVm.GetCurrentNodes())
            {
                count++;
            }

            Assert.AreEqual(2, count);
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
            Assert.AreEqual(0, placedCount);
            Assert.AreEqual(0, removedCount);
        }

        [Test]
        public void OnGearPickedUp_FiresDragServiceStartDrag()
        {
            var data = new GearConfigData { Id = "drag_start_test", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(1, 1), data);
            gridManager.AddNode(node);

            boardVm.OnGearPickedUp(node, new Vector2Int(1, 1));

            Assert.IsTrue(fakeDragService.IsDragging, "DragService should be dragging after pickup.");
            Assert.AreSame(data, fakeDragService.GetDragData<GearConfigData>());
        }

        [Test]
        public void OnGearDropped_EmptySlot_AddsNodeAtNewPosition()
        {
            var data = new GearConfigData { Id = "move_test", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            boardVm.OnGearPickedUp(node, new Vector2Int(2, 2));
            boardVm.OnGearDropped(node, new Vector2Int(1, 1));

            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 2)));
            Assert.AreSame(node, gridManager.GetNode(new Vector2Int(1, 1)));
            Assert.AreEqual(0, placedCount);
            Assert.AreEqual(0, removedCount);
            Assert.IsFalse(fakeDragService.IsDragging, "DragService should not be dragging after drop.");
        }

        [Test]
        public void OnGearDropped_OutOfBounds_SnapsBackToOriginalPosition()
        {
            var data = new GearConfigData { Id = "bounds_test", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            boardVm.OnGearPickedUp(node, new Vector2Int(2, 2));
            boardVm.OnGearDropped(node, new Vector2Int(99, 99));

            Assert.AreSame(node, gridManager.GetNode(new Vector2Int(2, 2)));
            Assert.AreEqual(0, placedCount);
            Assert.AreEqual(0, removedCount);
        }

        [Test]
        public void HandleBoardGearReturnedOverUI_NullNode_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => boardVm.HandleBoardGearReturnedOverUI(null, null));
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

            var dragged = new BaseGearNode(gridManager, eventController);
            dragged.Initialize(new Vector2Int(2, 2), draggedData);
            gridManager.AddNode(dragged);

            boardVm.OnGearPickedUp(dragged, new Vector2Int(2, 2));
            boardVm.OnGearDropped(dragged, new Vector2Int(1, 1));

            Assert.IsNull(gridManager.GetNode(new Vector2Int(2, 2)));
            IGridNode atTarget = gridManager.GetNode(new Vector2Int(1, 1));
            Assert.IsNotNull(atTarget);
            Assert.AreNotSame(occupant, atTarget);
            Assert.AreNotSame(dragged, atTarget);
            // FakeMergeService just returns occupant
            Assert.AreEqual(2, removedCount);
            Assert.AreEqual(1, placedCount);
            Assert.Contains(occupant, removedNodes);
            Assert.Contains(dragged, removedNodes);
            Assert.Contains(atTarget, placedNodes);

            UnityEngine.Object.DestroyImmediate(nextLvl);
        }

        [Test]
        public void SwapNode_FiresLifecycleEvents()
        {
            var d1 = new GearConfigData { Id = "s1", Category = GearCategory.Base };
            var d2 = new GearConfigData { Id = "s2", Category = GearCategory.Base };
            var a = new BaseGearNode(gridManager, eventController);
            a.Initialize(new Vector2Int(0, 0), d1);
            var b = new BaseGearNode(gridManager, eventController);
            b.Initialize(new Vector2Int(1, 0), d2);
            gridManager.AddNode(a);
            gridManager.AddNode(b);

            boardVm.OnGearPickedUp(a, new Vector2Int(0, 0));
            boardVm.OnGearDropped(a, new Vector2Int(1, 0));

            // Swap fires: 1x OnGearRemoved (occupant), 2x OnGearPlaced (dragged + occupant)
            Assert.AreEqual(1, removedCount, "OnGearRemoved should fire once for the occupant.");
            Assert.AreEqual(2, placedCount, "OnGearPlaced should fire for both dragged and occupant.");
            Assert.Contains(b, removedNodes);
            Assert.Contains(a, placedNodes);
            Assert.Contains(b, placedNodes);
        }

        [Test]
        public void HandleInventoryDrop_ReturnsTrueAndPlacesGear()
        {
            var gear = new GearConfigData { Id = "inv1", Category = GearCategory.Base };
            Vector2Int pos = new Vector2Int(3, 3);
            bool ok = boardVm.HandleInventoryDrop(pos, gear);

            Assert.IsTrue(ok);
            Assert.IsNotNull(gridManager.GetNode(new Vector2Int(3, 3)));
            Assert.AreEqual(1, placedCount);
        }

        [Test]
        public void HandleInventoryDrop_ReturnsFalseForFullCell()
        {
            var occupantData = new GearConfigData { Id = "occ", Category = GearCategory.Base };
            var occ = new BaseGearNode(gridManager, eventController);
            occ.Initialize(new Vector2Int(2, 2), occupantData);
            gridManager.AddNode(occ);

            var dropData = new GearConfigData { Id = "other", Category = GearCategory.Base };
            Vector2Int pos = new Vector2Int(2, 2);
            bool ok = boardVm.HandleInventoryDrop(pos, dropData);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, placedCount);
        }

        [Test]
        public void HandleInventoryDrop_RejectsWhenBoardLimitReached()
        {
            // Place gears up to the MaxAllowedBoardGears limit (5)
            for (int i = 0; i < boardConfig.MaxAllowedBoardGears; i++)
            {
                var data = new GearConfigData { Id = $"fill_{i}", Category = GearCategory.Base };
                Vector2Int pos = new Vector2Int(i, 0);
                bool placed = boardVm.HandleInventoryDrop(pos, data);
                Assert.IsTrue(placed, $"Gear {i} should be placed successfully.");
            }

            // Next placement should be rejected
            var extraGear = new GearConfigData { Id = "overflow", Category = GearCategory.Base };
            Vector2Int overflowPos = new Vector2Int(0, 1);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"Board limit reached"));
            bool rejected = boardVm.HandleInventoryDrop(overflowPos, extraGear);

            Assert.IsFalse(rejected, "Placement should be rejected when board limit is reached.");
            Assert.IsNull(gridManager.GetNode(new Vector2Int(0, 1)), "No gear should be at the overflow position.");
        }

        [Test]
        public void LoadLayout_AddsNodesToGridWithoutLifecycleEvents()
        {
            var gc = ScriptableObject.CreateInstance<GearConfig>();
            var gcSo = new UnityEditor.SerializedObject(gc);
            var dp = gcSo.FindProperty("data");
            Assert.IsNotNull(dp);
            dp.FindPropertyRelative("Id").stringValue = "layout_gear";
            dp.FindPropertyRelative("Category").enumValueIndex = (int)GearCategory.Base;
            gcSo.ApplyModifiedProperties();

            var layout = new BoardLayoutData(new[]
            {
                new BoardGearPlacementData(new Vector2Int(2, 2), gc)
            });

            boardVm.LoadLayout(layout);

            Assert.IsNotNull(gridManager.GetNode(new Vector2Int(2, 2)));
            Assert.AreEqual(0, placedCount);
            Assert.AreEqual(0, removedCount);

            UnityEngine.Object.DestroyImmediate(gc);
        }

        [Test]
        public void LoadLayout_RejectsDuplicatePlacements()
        {
            var gc = ScriptableObject.CreateInstance<GearConfig>();
            var gcSo = new UnityEditor.SerializedObject(gc);
            var dp = gcSo.FindProperty("data");
            Assert.IsNotNull(dp);
            dp.FindPropertyRelative("Id").stringValue = "dup";
            dp.FindPropertyRelative("Category").enumValueIndex = (int)GearCategory.Base;
            gcSo.ApplyModifiedProperties();

            var layout = new BoardLayoutData(new[]
            {
                new BoardGearPlacementData(new Vector2Int(1, 1), gc),
                new BoardGearPlacementData(new Vector2Int(1, 1), gc)
            });

            LogAssert.Expect(LogType.Error, "[BoardViewModel] Duplicate starting gear at (1, 1).");
            boardVm.LoadLayout(layout);

            Assert.IsNotNull(gridManager.GetNode(new Vector2Int(1, 1)));
            Assert.AreEqual(1, gridManager.GetAllNodes().Count());

            UnityEngine.Object.DestroyImmediate(gc);
        }

        [Test]
        public void LoadLayout_RejectsOutOfBoundsPlacements()
        {
            var gc = ScriptableObject.CreateInstance<GearConfig>();
            var gcSo = new UnityEditor.SerializedObject(gc);
            var dp = gcSo.FindProperty("data");
            Assert.IsNotNull(dp);
            dp.FindPropertyRelative("Id").stringValue = "oob";
            dp.FindPropertyRelative("Category").enumValueIndex = (int)GearCategory.Base;
            gcSo.ApplyModifiedProperties();

            var layout = new BoardLayoutData(new[]
            {
                new BoardGearPlacementData(new Vector2Int(99, 99), gc)
            });

            LogAssert.Expect(LogType.Error, "[BoardViewModel] Ignoring out-of-bounds starting gear at (99, 99).");
            boardVm.LoadLayout(layout);

            Assert.AreEqual(0, gridManager.GetAllNodes().Count());

            UnityEngine.Object.DestroyImmediate(gc);
        }

        [Test]
        public void BoardView_Bind_SpawnsViewsForExistingNodes()
        {
            var data = new GearConfigData { Id = "view_bind", Category = GearCategory.Base };
            var node = new BaseGearNode(gridManager, eventController);
            node.Initialize(new Vector2Int(2, 2), data);
            gridManager.AddNode(node);

            var go = new GameObject("BoardViewBindTest");
            try
            {
                var boardView = go.AddComponent<BoardViewComponent>();
                var dragHandler = go.AddComponent<GearBoardDragHandler>();
                SerializedObject boardSo = new SerializedObject(boardView);
                boardSo.FindProperty("dragHandler").objectReferenceValue = dragHandler;
                boardSo.ApplyModifiedPropertiesWithoutUndo();
                boardVm.Interactable = false;
                boardView.Bind(boardVm);
                GearView[] views = go.GetComponentsInChildren<GearView>(true);
                Assert.AreEqual(1, views.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
