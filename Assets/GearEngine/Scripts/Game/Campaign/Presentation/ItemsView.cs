using System;
using System.Collections.Generic;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemsView : View<ItemsViewModel>
    {
        [Header("Perk List")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private ItemSlotView itemPrefab;

        [Header("Actions")]
        [SerializeField] private GameObject buyButtonContainer;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMPro.TMP_Text buyButtonText;
        [SerializeField] private Button backButton;

        private readonly List<ItemSlotView> spawnedItems = new List<ItemSlotView>();

        protected override void OnBind()
        {
            ValidateHierarchy();
            
            if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            if (viewModel.Config != null && !viewModel.Config.ShowBuyButton)
            {
                if (buyButtonContainer != null) buyButtonContainer.SetActive(false);
                else if (buyButton != null) buyButton.gameObject.SetActive(false);
            }
            else
            {
                if (buyButtonContainer != null) buyButtonContainer.SetActive(true);
                else if (buyButton != null) buyButton.gameObject.SetActive(true);
                
                Bind<bool, bool>(() => viewModel.IsBuying, _ => UpdateBuyButtonState());
                Bind<bool, bool>(() => viewModel.CanAffordBuy, _ => UpdateBuyButtonState());
                Bind<long, long>(() => viewModel.NextCost, _ => UpdateBuyButtonState());
            }

            Bind<int, int>(() => viewModel.ItemsRevision, _ => RebuildItemList());
            
            UpdateBuyButtonState();
        }

        protected override void OnUnbind()
        {
            if (buyButton != null) buyButton.onClick.RemoveListener(OnBuyClicked);
            if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
            
            ClearItems();
            DisposeViewModelIfNeeded();
            base.OnUnbind();
        }

        private void RebuildItemList()
        {
            ClearItems();

            IReadOnlyList<ItemSlotViewModel> slotItems = viewModel.Items;
            
            if (slotItems.Count == 0)
            {
                Debug.LogWarning("[ItemsView] O jogador não possui nenhum item no momento.");
            }

            for (int i = 0; i < slotItems.Count; i++)
            {
                ItemSlotView slot = Instantiate(itemPrefab, itemContainer);
                slot.Bind(slotItems[i]);
                spawnedItems.Add(slot);
            }
        }

        private void ClearItems()
        {
            for (int i = 0; i < spawnedItems.Count; i++)
            {
                if (spawnedItems[i] != null)
                {
                    Destroy(spawnedItems[i].gameObject);
                }
            }
            spawnedItems.Clear();

            if (itemContainer != null)
            {
                foreach (Transform child in itemContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void UpdateBuyButtonState()
        {
            if (viewModel.Config != null && !viewModel.Config.ShowBuyButton) return;

            if (buyButtonText != null)
            {
                string colorTag = viewModel.CanAffordBuy ? "green" : "red";
                buyButtonText.text = $"Buy\n<color={colorTag}>{viewModel.NextCost}</color>";
            }

            if (buyButton != null)
            {
                buyButton.interactable = viewModel.CanAffordBuy && !viewModel.IsBuying;
            }
        }

        private void OnBuyClicked()
        {
            try
            {
                viewModel?.BuyRandom();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemsView] OnBuyClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnBackClicked()
        {
            try
            {
                viewModel?.CloseMenu();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemsView] OnBackClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(itemContainer, nameof(itemContainer));
            RequireReference(itemPrefab, nameof(itemPrefab));
            // Buy button is optional now depending on Config, but if it's assigned in the prefab it's fine.
            // If it's required for perks, we just warn if missing.
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                Debug.LogWarning($"[ItemsView] {name} is not assigned on the scene instance. This might cause issues depending on config.");
            }
        }

        private void DisposeViewModelIfNeeded()
        {
            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
