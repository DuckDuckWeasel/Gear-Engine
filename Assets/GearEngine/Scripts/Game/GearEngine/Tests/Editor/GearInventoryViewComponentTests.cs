using System.Reflection;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services.Inventory;
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

            public void ResetGridSimulationState()
            {
            }
        }

        [Test]
        public void RebuildListTwiceInOneFrame_KeepsOneSetOfSlots()
        {
            GearInventoryViewComponent component = CreateBoundInventoryView(out RectTransform container, out _);

            MethodInfo rebuild = typeof(GearInventoryViewComponent).GetMethod(
                "RebuildUIList",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(rebuild);
            rebuild.Invoke(component, null);
            rebuild.Invoke(component, null);

            Assert.AreEqual(3, container.childCount);
        }

        private static GearInventoryViewComponent CreateBoundInventoryView(
            out RectTransform itemsContainerOut,
            out GearInventoryViewModel viewModel)
        {
            var loadout = GearInventoryLoadoutData.FromGearConfigs(10, System.Array.Empty<GearConfig>());
            var inventory = new InventoryService(loadout);
            var engine = new FakeEngine();
            inventory.TryAdd(new GearConfigData { Id = "g0" });
            inventory.TryAdd(new GearConfigData { Id = "g1" });
            inventory.TryAdd(new GearConfigData { Id = "g2" });

            viewModel = new GearInventoryViewModel(engine, inventory, dragService: null);

            var root = new GameObject("InventoryRoot");
            var containerGo = new GameObject("ItemsContainer", typeof(RectTransform));
            containerGo.transform.SetParent(root.transform, false);
            var containerRect = containerGo.GetComponent<RectTransform>();

            var slotPrefab = new GameObject("SlotPrefab", typeof(RectTransform));
            slotPrefab.AddComponent<DragHandler>();
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

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{name}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
