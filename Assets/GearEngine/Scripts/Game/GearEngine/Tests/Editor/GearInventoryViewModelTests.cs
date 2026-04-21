using System;
using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using NUnit.Framework;
using UnityEngine;

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

            public IEnumerable<IGridNode> GetAllNodes() => new List<IGridNode>();

            public void ResetGridSimulationState()
            {
            }
        }

        private sealed class TrayTestBoardService : IBoardService
        {
            public event Action<IGridNode> GearPlaced;

            public event Action<IGridNode> GearRemoved;

            public event Action BoardLayoutChanged;

            public BoardModel GetBoard() => null;

            public BoardRulesSO BoardRules => null;

            public bool IsSimulationRunning => false;

            public int CurrentBoardGearCount => 0;

            public int MaxAllowedBoardGears => 99;

            public IGridNode GetNode(Vector2Int coord) => null;

            public IEnumerable<IGridNode> GetAllNodes() => Array.Empty<IGridNode>();

            public void ToggleSimulation()
            {
            }

            public void LoadLayout(BoardLayoutData layout)
            {
            }

            public bool TryMoveBoardGear(IGridNode node, Vector2Int toPos, Vector2Int fromPos) => false;

            public bool TryPlace(Vector2Int targetDropPos, GearConfigData gearData) => false;

            public bool TryRemoveBoardGear(IGridNode node) => false;

            public bool TryDeleteBoardGear(IGridNode node) => false;

            public void SnapNodeBackToOriginal(IGridNode node, Vector2Int originalPos)
            {
            }
        }

        private sealed class ListInventoryService : IInventoryService
        {
            private readonly List<GearConfig> owned = new List<GearConfig>();

            public event Action InventoryChanged;

            public bool HasSavedInventory => owned.Count > 0;

            public IReadOnlyList<GearConfig> Owned => owned;

            public void Seed(params GearConfig[] gears)
            {
                owned.AddRange(gears);
            }

            public bool TryAdd(GearConfig gear)
            {
                if (gear == null)
                {
                    return false;
                }

                owned.Add(gear);
                InventoryChanged?.Invoke();
                return true;
            }

            public bool TryRemove(GearConfig gear)
            {
                if (gear == null)
                {
                    return false;
                }

                int i = owned.FindIndex(g => g.Id == gear.Id);
                if (i < 0)
                {
                    return false;
                }

                owned.RemoveAt(i);
                InventoryChanged?.Invoke();
                return true;
            }
        }

        [Test]
        public void Constructor_BuildsTray_FromOwnedWhenBoardEmpty()
        {
            GearConfig cfg = CreateGearConfig("seed");
            var inventory = new ListInventoryService();
            inventory.Seed(cfg);
            var engine = new FakeEngine();
            var board = new TrayTestBoardService();

            var vm = new GearInventoryViewModel(engine, board, inventory);

            Assert.AreEqual(1, vm.TrayItems.Count);
            Assert.AreEqual("seed", vm.TrayItems[0].Id);

            UnityEngine.Object.DestroyImmediate(cfg);
        }

        [Test]
        public void RecreatingViewModel_DoesNotResetSharedInventory()
        {
            var inventory = new ListInventoryService();
            var engine = new FakeEngine();
            var board = new TrayTestBoardService();

            GearConfig persist = CreateGearConfig("persist");
            inventory.TryAdd(persist);

            _ = new GearInventoryViewModel(engine, board, inventory);

            var second = new GearInventoryViewModel(engine, board, inventory);

            Assert.AreEqual(1, second.TrayItems.Count);
            Assert.AreEqual("persist", second.TrayItems[0].Id);

            UnityEngine.Object.DestroyImmediate(persist);
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
