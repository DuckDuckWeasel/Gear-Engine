using System;
using System.Collections.Generic;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>
    /// View for the <c>TalentPerks View</c> prefab.
    /// Binds to <see cref="TalentPerksViewModel"/>, instantiates a <see cref="PerkOptionView"/> prefab
    /// for each owned perk group and wires the Buy button.
    /// </summary>
    public sealed class TalentPerksView : View<TalentPerksViewModel>
    {
        [Header("Perk List")]
        [SerializeField] private Transform perkContainer;
        [SerializeField] private ItemPerkView perkPrefab;

        [Header("Actions")]
        [SerializeField] private Button buyButton;
        [SerializeField] private TMPro.TMP_Text buyButtonText;
        [SerializeField] private Button backButton;

        private readonly List<ItemPerkView> spawnedPerks = new List<ItemPerkView>();

        protected override void OnBind()
        {
            ValidateHierarchy();
            buyButton.onClick.AddListener(OnBuyClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            // Bind buying state and affordability
            Bind<bool, bool>(() => viewModel.IsBuying, _ => UpdateBuyButtonState());
            Bind<bool, bool>(() => viewModel.CanAffordBuy, _ => UpdateBuyButtonState());
            Bind<long, long>(() => viewModel.NextCost, _ => UpdateBuyButtonState());

            Bind<int, int>(() => viewModel.ItemsRevision, _ => RebuildPerkList());
            
            // Explicitly set initial state
            UpdateBuyButtonState();
        }

        protected override void OnUnbind()
        {
            buyButton.onClick.RemoveListener(OnBuyClicked);
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
            }
            ClearPerks();
            DisposeViewModelIfNeeded();
            base.OnUnbind();
        }

        private void RebuildPerkList()
        {
            ClearPerks();

            IReadOnlyList<ItemPerkViewModel> perkItems = viewModel.Items;
            
            if (perkItems.Count == 0)
            {
                Debug.LogWarning("[TalentPerksView] O jogador não possui nenhuma carta no momento.");
            }

            for (int i = 0; i < perkItems.Count; i++)
            {
                ItemPerkView perk = Instantiate(perkPrefab, perkContainer);
                perk.Bind(perkItems[i]);
                spawnedPerks.Add(perk);
            }
        }

        private void ClearPerks()
        {
            for (int i = 0; i < spawnedPerks.Count; i++)
            {
                if (spawnedPerks[i] != null)
                {
                    Destroy(spawnedPerks[i].gameObject);
                }
            }
            spawnedPerks.Clear();

            // Destroy any remaining children (e.g. editor dummies)
            if (perkContainer != null)
            {
                foreach (Transform child in perkContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void UpdateBuyButtonState()
        {
            if (buyButtonText != null)
            {
                string colorTag = viewModel.CanAffordBuy ? "green" : "red";
                buyButtonText.text = $"Buy\n<color={colorTag}>{viewModel.NextCost}</color>";
            }

            buyButton.interactable = viewModel.CanAffordBuy && !viewModel.IsBuying;
        }

        private void OnBuyClicked()
        {
            try
            {
                viewModel?.BuyRandom();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TalentPerksView] OnBuyClicked failed: {ex.Message}\n{ex.StackTrace}");
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
                Debug.LogError($"[TalentPerksView] OnBackClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(perkContainer, nameof(perkContainer));
            RequireReference(perkPrefab, nameof(perkPrefab));
            RequireReference(buyButton, nameof(buyButton));
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"[TalentPerksView] {name} must be assigned on the scene instance.");
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
