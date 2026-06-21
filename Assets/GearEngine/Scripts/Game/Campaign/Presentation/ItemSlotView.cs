using System;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemSlotView : ViewComponent<ItemSlotViewModel>
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private Material grayscaleMaterial;

        protected override void OnBind()
        {
            base.OnBind();
            ApplyPerkData();
            SubscribeSelectButton();
        }

        protected override void OnUnbind()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(OnSelectClicked);
            }
            base.OnUnbind();
        }

        private void ApplyPerkData()
        {
            if (viewModel?.Item == null) return;

            if (nameLabel != null)
            {
                string colorHex = viewModel.Item.Rarity.GetColorHex();
                if (viewModel.Amount > 1)
                    nameLabel.text = $"<color=#{colorHex}>x{viewModel.Amount} {viewModel.Item.Name}</color>";
                else
                    nameLabel.text = $"<color=#{colorHex}>{viewModel.Item.Name}</color>";
            }

            if (descriptionLabel != null)
                descriptionLabel.text = viewModel.Item.Description;

            if (iconImage != null && viewModel.Item.Icon != null)
            {
                iconImage.sprite = viewModel.Item.Icon;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            Image bgImage = null;
            if (selectButton != null) bgImage = selectButton.image;
            if (bgImage == null) bgImage = GetComponent<Image>();

            if (bgImage != null)
            {
                if (viewModel.IsOwned)
                {
                    bgImage.material = null;
                    if (grayscaleMaterial == null) bgImage.color = Color.white;
                }
                else
                {
                    if (grayscaleMaterial != null)
                    {
                        bgImage.material = grayscaleMaterial;
                    }
                    else
                    {
                        bgImage.material = null;
                        bgImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
                }
            }
        }

        private void SubscribeSelectButton()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectClicked);
                Bind<bool, bool>(() => viewModel.CanPick, interactable => selectButton.interactable = interactable);
            }
        }

        private void OnSelectClicked()
        {
            try
            {
                viewModel?.Pick();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemSlotView] OnSelectClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
