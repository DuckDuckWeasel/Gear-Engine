using System;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class CardOptionView : ViewComponent<CardOptionViewModel>
    {
        [SerializeField] private TextMeshProUGUI gearNameLabel;
        [SerializeField] private GameObject selectedHighlight;
        [SerializeField] private Button selectButton;

        protected override void OnBind()
        {
            base.OnBind();
            ApplyGearNameLabel();
            BindSelectionHighlight();
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

        private void ApplyGearNameLabel()
        {
            if (gearNameLabel != null)
            {
                gearNameLabel.text = viewModel.GearConfig.name;
            }
        }

        private void BindSelectionHighlight()
        {
            Bind<bool, bool>(() => viewModel.IsSelected, UpdateHighlight);
        }

        private void UpdateHighlight(bool isSelected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(isSelected);
            }
        }

        private void SubscribeSelectButton()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectClicked);
            }
        }

        private void OnSelectClicked()
        {
            try
            {
                viewModel?.Select();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardOptionView] OnSelectClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
