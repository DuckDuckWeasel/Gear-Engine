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
        private RectTransform slotsContainer;

        [SerializeField]
        private TextMeshProUGUI slotRowTemplate;

        private readonly List<GameObject> slotRowInstances = new List<GameObject>();

        protected override void OnBind()
        {
            ValidateHierarchy();
            EnsureSlotsLayout();
            Bind<long, long>(() => viewModel.Gold, OnGoldChanged);
            Bind<int, int>(() => viewModel.InventoryRevision, OnInventoryRevisionChanged);
            OnGoldChanged(viewModel.Gold);
            RefreshSlotRows();
        }

        protected override void OnUnbind()
        {
            ClearSlotRows();
            base.OnUnbind();
        }

        private void ValidateHierarchy()
        {
            if (goldLabel == null)
            {
                throw new InvalidOperationException("[CardSampleView] Assign goldLabel.");
            }

            if (slotsContainer == null)
            {
                throw new InvalidOperationException("[CardSampleView] Assign slotsContainer.");
            }
        }

        private void EnsureSlotsLayout()
        {
            VerticalLayoutGroup layout = slotsContainer.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                return;
            }

            layout = slotsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
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

        private void OnInventoryRevisionChanged(int _)
        {
            RefreshSlotRows();
        }

        private void RefreshSlotRows()
        {
            ClearSlotRows();
            IReadOnlyList<CardSlotSnapshot> slots = viewModel.Slots;
            for (var i = 0; i < slots.Count; i++)
            {
                CreateSlotRow(i, slots[i]);
            }
        }

        private void CreateSlotRow(int index, CardSlotSnapshot slot)
        {
            var row = new GameObject($"SlotRow_{index}", typeof(RectTransform));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.SetParent(slotsContainer, false);
            rowRect.localScale = Vector3.one;

            var horizontal = row.AddComponent<HorizontalLayoutGroup>();
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.spacing = 12f;
            horizontal.childControlWidth = true;
            horizontal.childForceExpandWidth = true;

            var rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.minHeight = 40f;

            TextMeshProUGUI label = CreateLabelForRow(slot, index);
            label.rectTransform.SetParent(rowRect, false);

            if (slot.State == CardSlotState.Uncollected)
            {
                Button buy = CreateBuyButton(index);
                buy.transform.SetParent(rowRect, false);
            }

            slotRowInstances.Add(row);
        }

        private TextMeshProUGUI CreateLabelForRow(CardSlotSnapshot slot, int index)
        {
            string cardDisplay = string.IsNullOrEmpty(slot.CardId) ? "-" : slot.CardId;
            string text = $"Slot {index}: {slot.State} | Card: {cardDisplay}";

            if (slotRowTemplate != null)
            {
                TextMeshProUGUI instance = Instantiate(slotRowTemplate);
                instance.gameObject.SetActive(true);
                instance.text = text;
                return instance;
            }

            var go = new GameObject("Label", typeof(RectTransform));
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button CreateBuyButton(int slotIndex)
        {
            var go = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.45f, 0.85f, 1f);

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => HandlePurchaseClicked(slotIndex));

            var labelGo = new GameObject("Text", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "Buy";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.minWidth = 100f;
            layout.preferredWidth = 120f;
            layout.flexibleWidth = 0f;

            return btn;
        }

        private void HandlePurchaseClicked(int slotIndex)
        {
            try
            {
                viewModel?.TryPurchaseSlot(slotIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardSampleView] Purchase click failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ClearSlotRows()
        {
            for (var i = 0; i < slotRowInstances.Count; i++)
            {
                if (slotRowInstances[i] != null)
                {
                    Destroy(slotRowInstances[i]);
                }
            }

            slotRowInstances.Clear();
        }
    }
}
