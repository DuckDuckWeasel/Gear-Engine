using System;
using System.Collections.Generic;
using System.Reflection;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearInventoryViewComponentTests
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
            private readonly List<OwnedGear> owned = new List<OwnedGear>();

            public event Action InventoryChanged;

            public bool HasSavedInventory => owned.Count > 0;

            public IReadOnlyList<OwnedGear> Owned => owned;

            public OwnedGear Add(GearConfig gear)
            {
                if (gear == null)
                {
                    return null;
                }

                var o = new OwnedGear { InstanceId = Guid.NewGuid().ToString("N"), Config = gear };
                owned.Add(o);
                InventoryChanged?.Invoke();
                return o;
            }

            public bool Remove(OwnedGear gear)
            {
                if (gear == null || !owned.Remove(gear))
                {
                    return false;
                }

                InventoryChanged?.Invoke();
                return true;
            }

            public void Clear()
            {
                owned.Clear();
                InventoryChanged?.Invoke();
            }
        }

        [Test]
        public void RebuildListTwiceInOneFrame_KeepsOneSetOfSlots()
        {
            GearInventoryViewComponent component = CreateBoundInventoryView(out RectTransform container, out _);

            component.RebuildAndFit();
            component.RebuildAndFit();

            Assert.AreEqual(3, container.childCount);
        }

        [Test]
        public void Bind_DoesNotPopulateSlots_UntilRebuildAndFitIsCalled()
        {
            GearInventoryViewComponent component = CreateBoundInventoryView(out RectTransform container, out _);

            Assert.AreEqual(0, container.childCount);

            component.RebuildAndFit();

            Assert.AreEqual(3, container.childCount);
        }

        private static GearInventoryViewComponent CreateBoundInventoryView(
            out RectTransform itemsContainerOut,
            out GearInventoryViewModel viewModel)
        {
            var inventory = new ListInventoryService();
            var engine = new FakeEngine();
            var board = new TrayTestBoardService();

            GearConfig g0 = CreateGearConfig("g0");
            GearConfig g1 = CreateGearConfig("g1");
            GearConfig g2 = CreateGearConfig("g2");
            inventory.Add(g0);
            inventory.Add(g1);
            inventory.Add(g2);

            viewModel = new GearInventoryViewModel(engine, board, inventory);

            var root = new GameObject("InventoryRoot");
            var containerGo = new GameObject("ItemsContainer", typeof(RectTransform));
            containerGo.transform.SetParent(root.transform, false);
            var containerRect = containerGo.GetComponent<RectTransform>();

            var slotPrefab = new GameObject("SlotPrefab", typeof(RectTransform));
            slotPrefab.AddComponent<Draggable>();
            GearInventorySlotView slotViewComp = slotPrefab.AddComponent<GearInventorySlotView>();
            var visualContainer = new GameObject("VisualContainer", typeof(RectTransform));
            visualContainer.transform.SetParent(slotPrefab.transform, false);
            SetPrivateField(slotViewComp, "visualContainer", visualContainer.transform);

            var labelGo = new GameObject("LimitLabel", typeof(RectTransform));
            labelGo.transform.SetParent(root.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();

            GearInventoryViewComponent component = root.AddComponent<GearInventoryViewComponent>();
            SetPrivateField(component, "itemsContainer", containerRect);
            SetPrivateField(component, "slotPrefab", slotPrefab);
            SetPrivateField(component, "inventoryLimitLabel", label);

            component.Bind(viewModel);

            itemsContainerOut = containerRect;
            return component;
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

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{name}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
