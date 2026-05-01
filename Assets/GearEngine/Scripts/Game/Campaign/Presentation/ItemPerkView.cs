using System;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemPerkView : ViewComponent<ItemPerkViewModel>
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;

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
                Debug.LogError($"[ItemPerkView] OnSelectClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
