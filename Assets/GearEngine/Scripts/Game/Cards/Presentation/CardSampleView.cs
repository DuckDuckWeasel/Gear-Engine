using System;
using System.Collections.Generic;
using GearEngine.Cards;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Cards.Presentation
{
    public sealed class CardSampleView : View<CardSampleViewModel>
    {
        [SerializeField]
        private TextMeshProUGUI goldLabel;

        [SerializeField]
        private TextMeshProUGUI nextCostLabel;

        [SerializeField]
        private Button purchaseButton;

        [SerializeField]
        private RectTransform unlockedListContainer;

        [SerializeField]
        private TextMeshProUGUI unlockedRowTemplate;

        private readonly List<GameObject> unlockedRowInstances = new List<GameObject>();

        protected override void OnBind()
        {
            ValidateHierarchy();
            EnsureListLayout();
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            Bind<long, long>(() => viewModel.Gold, OnGoldChanged);
            Bind<long, long>(() => viewModel.NextCost, OnNextCostChanged);
            Bind<int, int>(() => viewModel.CardsRevision, OnCardsRevisionChanged);
            OnGoldChanged(viewModel.Gold);
            OnNextCostChanged(viewModel.NextCost);
            RefreshUnlockedRows();
        }

        protected override void OnUnbind()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            }

            ClearUnlockedRows();
            base.OnUnbind();
        }

        private void ValidateHierarchy()
        {
            if (goldLabel == null)
            {
                throw new InvalidOperationException("[CardSampleView] Assign goldLabel.");
            }

            if (nextCostLabel == null)
            {
                throw new InvalidOperationException("[CardSampleView] Assign nextCostLabel.");
            }

            if (unlockedListContainer == null)
            {
                throw new InvalidOperationException("[CardSampleView] Assign unlockedListContainer.");
            }
        }

        private void EnsureListLayout()
        {
            VerticalLayoutGroup layout = unlockedListContainer.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                return;
            }

            layout = unlockedListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private void OnGoldChanged(long value)
        {
            goldLabel.text = $"Gold: {value}";
        }

        private void OnNextCostChanged(long value)
        {
            string cur = string.IsNullOrEmpty(viewModel.CurrencyId) ? "gold" : viewModel.CurrencyId;
            nextCostLabel.text = $"Next card ({cur}): {value}";
        }

        private void OnCardsRevisionChanged(int _)
        {
            RefreshUnlockedRows();
        }

        private void OnPurchaseClicked()
        {
            try
            {
                viewModel?.TryPurchaseRandomCard();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardSampleView] Purchase click failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RefreshUnlockedRows()
        {
            ClearUnlockedRows();
            IReadOnlyList<string> ids = viewModel.UnlockedCardIds;
            for (var i = 0; i < ids.Count; i++)
            {
                CreateUnlockedRow(i, ids[i]);
            }
        }

        private void CreateUnlockedRow(int index, string cardId)
        {
            var row = new GameObject($"UnlockedRow_{index}", typeof(RectTransform));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.SetParent(unlockedListContainer, false);
            rowRect.localScale = Vector3.one;

            var rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.minHeight = 32f;

            string label = viewModel.GetDisplayLabelForCard(cardId);
            TextMeshProUGUI tmp = CreateUnlockedLabel($"{index + 1}. {label}");
            tmp.rectTransform.SetParent(rowRect, false);
            unlockedRowInstances.Add(row);
        }

        private TextMeshProUGUI CreateUnlockedLabel(string text)
        {
            if (unlockedRowTemplate != null)
            {
                TextMeshProUGUI instance = Instantiate(unlockedRowTemplate);
                instance.gameObject.SetActive(true);
                instance.text = text;
                return instance;
            }

            var go = new GameObject("Label", typeof(RectTransform));
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 26;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private void ClearUnlockedRows()
        {
            for (var i = 0; i < unlockedRowInstances.Count; i++)
            {
                if (unlockedRowInstances[i] != null)
                {
                    Destroy(unlockedRowInstances[i]);
                }
            }

            unlockedRowInstances.Clear();
        }
    }
}
