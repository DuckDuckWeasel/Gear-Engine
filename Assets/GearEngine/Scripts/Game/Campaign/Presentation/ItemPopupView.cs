using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemPopupView : View<ItemPopupViewModel>
    {
        [Header("Item Viewer")]
        [SerializeField] private ItemSlotView itemView;
        [SerializeField] private TMPro.TMP_Text descriptionText;

        [Header("Navigation")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [Header("Actions")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMPro.TMP_Text actionButtonText;

        protected override void OnBind()
        {
            if (nextButton != null) nextButton.onClick.AddListener(viewModel.Next);
            if (previousButton != null) previousButton.onClick.AddListener(viewModel.Previous);
            if (closeButton != null) closeButton.onClick.AddListener(viewModel.Close);
            if (actionButton != null) actionButton.onClick.AddListener(viewModel.ExecuteAction);

            Bind<ItemSlotViewModel, ItemSlotViewModel>(() => viewModel.CurrentItem, UpdateItemView);
            Bind<bool, bool>(() => viewModel.CanExecuteAction, canExecute => 
            {
                if (actionButton != null)
                {
                    actionButton.gameObject.SetActive(canExecute);
                }
            });
            Bind<bool, bool>(() => viewModel.HasMultipleItems, hasMultiple => 
            {
                if (nextButton != null) nextButton.gameObject.SetActive(hasMultiple);
                if (previousButton != null) previousButton.gameObject.SetActive(hasMultiple);
            });
            
            if (actionButtonText != null && viewModel != null)
            {
                actionButtonText.text = viewModel.ActionName;
            }
        }

        protected override void OnUnbind()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(viewModel.Next);
            if (previousButton != null) previousButton.onClick.RemoveListener(viewModel.Previous);
            if (closeButton != null) closeButton.onClick.RemoveListener(viewModel.Close);
            if (actionButton != null) actionButton.onClick.RemoveListener(viewModel.ExecuteAction);

            base.OnUnbind();
        }

        private void UpdateItemView(ItemSlotViewModel itemVm)
        {
            if (itemView != null && itemVm != null)
            {
                itemView.Bind(itemVm);
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = itemVm?.Item?.Description ?? string.Empty;
            }
        }
    }
}
