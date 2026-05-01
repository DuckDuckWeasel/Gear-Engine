using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class PerkPopupView : View<PerkPopupViewModel>
    {
        [Header("Perk Viewer")]
        [SerializeField] private ItemPerkView perkView;

        [Header("Navigation")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [Header("Actions")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button burnButton;

        protected override void OnBind()
        {
            if (nextButton != null) nextButton.onClick.AddListener(viewModel.Next);
            if (previousButton != null) previousButton.onClick.AddListener(viewModel.Previous);
            if (closeButton != null) closeButton.onClick.AddListener(viewModel.Close);
            if (burnButton != null) burnButton.onClick.AddListener(viewModel.Burn);

            Bind<ItemPerkViewModel, ItemPerkViewModel>(() => viewModel.CurrentPerk, UpdatePerkView);
            Bind<bool, bool>(() => viewModel.CanBurn, canBurn => 
            {
                if (burnButton != null)
                {
                    burnButton.gameObject.SetActive(canBurn);
                }
            });
            Bind<bool, bool>(() => viewModel.HasMultiplePerks, hasMultiple => 
            {
                if (nextButton != null) nextButton.gameObject.SetActive(hasMultiple);
                if (previousButton != null) previousButton.gameObject.SetActive(hasMultiple);
            });
        }

        protected override void OnUnbind()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(viewModel.Next);
            if (previousButton != null) previousButton.onClick.RemoveListener(viewModel.Previous);
            if (closeButton != null) closeButton.onClick.RemoveListener(viewModel.Close);
            if (burnButton != null) burnButton.onClick.RemoveListener(viewModel.Burn);

            base.OnUnbind();
        }

        private void UpdatePerkView(ItemPerkViewModel perkVm)
        {
            if (perkView != null && perkVm != null)
            {
                perkView.Bind(perkVm);
            }
        }
    }
}
