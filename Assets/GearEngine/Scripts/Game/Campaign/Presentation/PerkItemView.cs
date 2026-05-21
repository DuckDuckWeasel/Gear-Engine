using System;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>
    /// ViewComponent for a single perk slot in the TalentPerks scroll list.
    /// Attach to the <c>Perk_View</c> prefab instance.
    /// Shows the perk ID, owned count, and a Burn button to destroy one copy for gold.
    /// </summary>
    public sealed class PerkItemView : ViewComponent<PerkItemViewModel>
    {
        [SerializeField] private TextMeshProUGUI perkIdLabel;
        [SerializeField] private TextMeshProUGUI countLabel;
        [SerializeField] private Button burnButton;

        protected override void OnBind()
        {
            base.OnBind();
            ValidateHierarchy();
            ApplyLabels();
            Bind<int, int>(() => viewModel.Count, OnCountChanged);
            if (burnButton != null)
            {
                burnButton.onClick.AddListener(OnBurnClicked);
            }
        }

        protected override void OnUnbind()
        {
            if (burnButton != null)
            {
                burnButton.onClick.RemoveListener(OnBurnClicked);
            }

            base.OnUnbind();
        }

        private void ApplyLabels()
        {
            if (perkIdLabel != null)
            {
                perkIdLabel.text = viewModel.PerkId;
            }

            UpdateCountLabel(viewModel.Count);
        }

        private void OnCountChanged(int newCount)
        {
            UpdateCountLabel(newCount);
            if (burnButton != null)
            {
                burnButton.interactable = newCount > 0;
            }
        }

        private void UpdateCountLabel(int count)
        {
            if (countLabel != null)
            {
                countLabel.text = $"x{count}";
            }
        }

        private void OnBurnClicked()
        {
            try
            {
                viewModel?.Burn();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerkItemView] OnBurnClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            if (perkIdLabel == null)
            {
                throw new InvalidOperationException(
                    "[PerkItemView] perkIdLabel must be assigned on the prefab instance.");
            }

            if (countLabel == null)
            {
                throw new InvalidOperationException(
                    "[PerkItemView] countLabel must be assigned on the prefab instance.");
            }
        }
    }
}
