using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using Scaffold.MVVM;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryView : ViewComponent<GearInventoryViewModel>
    {
        [SerializeField]
        private RectTransform itemsContainer;
        [SerializeField]
        private TagSO gridBoardTag;

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

            ClearItemsContainer();
            foreach (GearConfigData gear in viewModel.InventoryModel.AvailableGears)
            {
                CreateInventorySlot(gear);
            }
        }

        private void ClearItemsContainer()
        {
            foreach (Transform child in itemsContainer)
            {
                Object.Destroy(child.gameObject);
            }
        }

        private void CreateInventorySlot(GearConfigData gear)
        {
            GameObject slotGroup = BuildSlotRoot(gear.Id);
            DragHandler dragger = ConfigureSlotDragHandler(slotGroup, gear);
            GearInventorySlotView slotView = slotGroup.AddComponent<GearInventorySlotView>();
            TryBindSlotVisual(gear, slotGroup.transform, dragger);
            slotView.Bind(gear, viewModel);
        }

        private GameObject BuildSlotRoot(string gearId)
        {
            GameObject slotGroup = new GameObject($"Slot_{gearId}");
            slotGroup.transform.SetParent(itemsContainer, false);

            RectTransform rect = slotGroup.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 120);

            UnityEngine.UI.Image img = slotGroup.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            return slotGroup;
        }

        private DragHandler ConfigureSlotDragHandler(GameObject slotGroup, GearConfigData gear)
        {
            DragHandler dragger = slotGroup.AddComponent<DragHandler>();
            if (gridBoardTag != null)
            {
                dragger.AddAcceptedTag(gridBoardTag);
            }

            dragger.GhostScaleMultiplier = gear.UIScaleMultiplier;
            return dragger;
        }

        private void TryBindSlotVisual(GearConfigData gear, Transform slotRoot, DragHandler dragger)
        {
            if (gear.VisualPrefab == null)
            {
                return;
            }

            GameObject visualObj = Instantiate(gear.VisualPrefab, slotRoot);
            visualObj.name = "VisualInstance";
            visualObj.transform.localPosition = new Vector3(0, 0, -10f);
            visualObj.transform.localScale = new Vector3(gear.UIScaleMultiplier, gear.UIScaleMultiplier, gear.UIScaleMultiplier);

            foreach (SpriteRenderer sr in visualObj.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.sortingOrder = 50;
            }

            CreateInventoryIconIfNeeded(gear, visualObj.transform);
            dragger.GhostPrefab = visualObj;
        }

        private void CreateInventoryIconIfNeeded(GearConfigData gear, Transform visualRoot)
        {
            if (gear.UIIcon == null)
            {
                return;
            }

            GameObject iconObj = new GameObject("InventoryUIIcon");
            iconObj.transform.SetParent(visualRoot, false);
            iconObj.transform.localPosition = new Vector3(0, 0, -1f);
            iconObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            SpriteRenderer iconRenderer = iconObj.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = gear.UIIcon;
            iconRenderer.sortingOrder = 55;
            BuildFillShaderMaterialForIcon(iconRenderer);
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

        private static void BuildFillShaderMaterialForIcon(SpriteRenderer iconRenderer)
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
    }
}
