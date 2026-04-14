using System.Collections.Specialized;
using System.Linq;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI.Tags;
using UnityEngine;
using Scaffold.MVVM;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryView : ViewComponent<GearInventoryViewModel>
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private TagSO gridBoardTag;

        public event System.Action<GearConfigData> OnInventoryDragStarted;
        public event System.Action OnInventoryDragEnded;

        protected override void OnBind()
        {
            if (viewModel.InventoryModel.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged += OnInventoryCollectionChanged;
            }

            Bind<GearConfigData, GearConfigData>(() => viewModel.InventoryModel.SelectedGear, OnSelectionChanged);

            DrawInitialList();
        }

        private void DrawInitialList()
        {
            if (viewModel == null || viewModel.InventoryModel.AvailableGears == null)
            {
                return;
            }

            if (itemsContainer == null)
            {
                Debug.LogWarning($"<color=#ff5555>[GearInventoryView]</color> ItemsContainer is not assigned in the inspector!");
                return;
            }

            RebuildUIList();
        }

        private void OnSelectionChanged(GearConfigData newSelection)
        {
            if (newSelection != null)
            {
                Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Highlight overlay moved onto -> {newSelection.Id}");
            }
            else
            {
                Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Highlight overlay disabled (None selected).");
            }
        }

        private void OnInventoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildUIList();
        }

        private void RebuildUIList()
        {
            if (itemsContainer == null || viewModel == null)
            {
                return;
            }

            ClearItemSlots();

            foreach (GearConfigData gear in viewModel.InventoryModel.AvailableGears)
            {
                CreateSlotForGear(gear);
            }
        }

        private void ClearItemSlots()
        {
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateSlotForGear(GearConfigData gear)
        {
            GameObject slotGroup = new GameObject($"Slot_{gear.Id}");
            slotGroup.transform.SetParent(itemsContainer, false);
            RectTransform rect = slotGroup.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 120);
            UnityEngine.UI.Image img = slotGroup.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            DragHandler dragger = slotGroup.AddComponent<DragHandler>();
            GearInventorySlotView slotView = slotGroup.AddComponent<GearInventorySlotView>();
            ConfigureDraggerForGear(dragger, gear);
            TryAttachVisualPrefab(slotGroup, dragger, gear);
            slotView.Bind(gear, viewModel);
            WireDragLifecycle(dragger, gear);
        }

        private void ConfigureDraggerForGear(DragHandler dragger, GearConfigData gear)
        {
            if (gridBoardTag != null)
            {
                dragger.AddAcceptedTag(gridBoardTag);
            }

            dragger.GhostScaleMultiplier = gear.UIScaleMultiplier;
        }

        private void WireDragLifecycle(DragHandler dragger, GearConfigData gear)
        {
            GearConfigData capturedGear = gear;
            dragger.OnDragBegin += () => OnInventoryDragStarted?.Invoke(capturedGear);
            dragger.OnDragEnd += () => OnInventoryDragEnded?.Invoke();
        }

        private void TryAttachVisualPrefab(GameObject slotGroup, DragHandler dragger, GearConfigData gear)
        {
            if (gear.VisualPrefab == null)
            {
                return;
            }

            GameObject visualObj = Instantiate(gear.VisualPrefab, slotGroup.transform);
            visualObj.name = "VisualInstance";
            visualObj.transform.localPosition = new Vector3(0, 0, -10f);
            visualObj.transform.localScale = new Vector3(gear.UIScaleMultiplier, gear.UIScaleMultiplier, gear.UIScaleMultiplier);
            BuildBoostedSpriteRenderers(visualObj);
            BuildInventoryIconOverlay(visualObj, gear);
            dragger.GhostPrefab = visualObj;
        }

        private void BuildBoostedSpriteRenderers(GameObject visualObj)
        {
            SpriteRenderer[] srs = visualObj.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingOrder = 50;
            }
        }

        private void BuildInventoryIconOverlay(GameObject visualObj, GearConfigData gear)
        {
            if (gear.UIIcon == null)
            {
                return;
            }

            GameObject iconObj = new GameObject("InventoryUIIcon");
            iconObj.transform.SetParent(visualObj.transform, false);
            iconObj.transform.localPosition = new Vector3(0, 0, -1f);
            iconObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            SpriteRenderer iconRenderer = iconObj.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = gear.UIIcon;
            iconRenderer.sortingOrder = 55;
            BuildFillMaterialForIcon(iconRenderer);
        }

        private void BuildFillMaterialForIcon(SpriteRenderer iconRenderer)
        {
            Shader fillShader = Shader.Find("GearEngine/Sprites/SpriteFillGrayscale");
            if (fillShader == null)
            {
                return;
            }

            Material mat = new Material(fillShader);
            mat.SetFloat("_FillAmount", 1f);
            iconRenderer.material = mat;
        }

        [ContextMenu("Mock: UI Click First Available Gear")]
        public void MockClickFirstGear()
        {
            if (viewModel.InventoryModel.AvailableGears.Count > 0)
            {
                viewModel.SelectGearLocal(viewModel.InventoryModel.AvailableGears.First());
            }
        }

        private void OnDestroy()
        {
            if (viewModel?.InventoryModel?.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged -= OnInventoryCollectionChanged;
            }
        }
    }
}
