using System;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemSlotView : ViewComponent<ItemSlotViewModel>
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Image rarityBackgroundImage;

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

            RarityConfigSO visualConfig = viewModel.Item.RarityConfig;

            if (nameLabel != null)
            {
                if (visualConfig != null)
                {
                    nameLabel.color = visualConfig.TextColor;
                }

                if (viewModel.Amount > 1)
                    nameLabel.text = $"x{viewModel.Amount} {viewModel.Item.Name}";
                else
                    nameLabel.text = viewModel.Item.Name;
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

            if (rarityBackgroundImage != null)
            {
                if (visualConfig != null && visualConfig.CardSprite != null)
                {
                    rarityBackgroundImage.sprite = visualConfig.CardSprite;
                }
                rarityBackgroundImage.color = Color.white;
            }

            Image[] allImages = GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (viewModel.IsOwned)
                {
                    img.material = null;
                }
                else if (grayscaleMaterial != null)
                {
                    img.material = grayscaleMaterial;
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
