using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using Scaffold.MVVM;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public class GearInventoryView : ViewComponent<GearInventoryViewModel>
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private TagSO gridBoardTag;

        private IObjectResolver container;

        public void SetObjectResolver(IObjectResolver resolver)
        {
            container = resolver;
        }

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

            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var gear in viewModel.InventoryModel.AvailableGears)
            {
                GameObject slotGroup = new GameObject($"Slot_{gear.Id}");
                slotGroup.transform.SetParent(itemsContainer, false);

                var rect = slotGroup.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(120, 120);

                var img = slotGroup.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

                var dragger = slotGroup.AddComponent<DragHandler>();
                var slotView = slotGroup.AddComponent<GearInventorySlotView>();

                if (container != null)
                {
                    container.Inject(slotView);
                    container.Inject(dragger);
                }

                if (gridBoardTag != null)
                {
                    dragger.AddAcceptedTag(gridBoardTag);
                }

                dragger.GhostScaleMultiplier = gear.UIScaleMultiplier;

                if (gear.VisualPrefab != null)
                {
                    GameObject visualObj = Instantiate(gear.VisualPrefab, slotGroup.transform);
                    visualObj.name = "VisualInstance";
                    visualObj.transform.localPosition = new Vector3(0, 0, -10f);

                    visualObj.transform.localScale = new Vector3(gear.UIScaleMultiplier, gear.UIScaleMultiplier, gear.UIScaleMultiplier);

                    var srs = visualObj.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (var sr in srs)
                    {
                        sr.sortingOrder = 50;
                    }

                    if (gear.UIIcon != null)
                    {
                        GameObject iconObj = new GameObject("InventoryUIIcon");
                        iconObj.transform.SetParent(visualObj.transform, false);
                        iconObj.transform.localPosition = new Vector3(0, 0, -1f);
                        iconObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                        SpriteRenderer iconRenderer = iconObj.AddComponent<SpriteRenderer>();
                        iconRenderer.sprite = gear.UIIcon;
                        iconRenderer.sortingOrder = 55;

                        Shader fillShader = Shader.Find("GearEngine/Sprites/SpriteFillGrayscale");
                        if (fillShader != null)
                        {
                            Material mat = new Material(fillShader);
                            mat.SetFloat("_FillAmount", 1f);
                            iconRenderer.material = mat;
                        }
                    }

                    dragger.GhostPrefab = visualObj;
                }

                slotView.Bind(gear, viewModel);
            }
        }

        private void OnDestroy()
        {
            if (viewModel?.InventoryModel?.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged -= OnInventoryCollectionChanged;
            }
        }

        [ContextMenu("Mock: UI Click First Available Gear")]
        public void MockClickFirstGear()
        {
            if (viewModel.InventoryModel.AvailableGears.Count > 0)
            {
                viewModel.SelectGearLocal(viewModel.InventoryModel.AvailableGears.First());
            }
        }
    }
}
