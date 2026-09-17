using Scaffold.Navigation.Contracts;
using VContainer;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public class ToolbarController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button storeButton;
        [SerializeField] private Button raceButton;
        [SerializeField] private Button garageButton;

        [Header("Screen State")]
        [Tooltip("Defines which content the Items view displays.")]
        [SerializeField] private ItemsScreenState itemsState;

        private INavigation navigation;

        [Inject]
        public void Construct(INavigation navigation)
        {
            this.navigation = navigation;
        }

        private void OnEnable()
        {
            if (storeButton != null)
            {
                storeButton.onClick.AddListener(OnStoreClicked);
            }

            if (raceButton != null)
            {
                raceButton.onClick.AddListener(OnRaceClicked);
            }

            if (garageButton != null)
            {
                garageButton.onClick.AddListener(OnGarageClicked);
            }
        }

        private void OnDisable()
        {
            if (storeButton != null)
            {
                storeButton.onClick.RemoveListener(OnStoreClicked);
            }

            if (raceButton != null)
            {
                raceButton.onClick.RemoveListener(OnRaceClicked);
            }

            if (garageButton != null)
            {
                garageButton.onClick.RemoveListener(OnGarageClicked);
            }
        }

        private void EnsureItemsState()
        {
            if (itemsState == null)
            {
                itemsState = ScriptableObject.CreateInstance<ItemsScreenState>();
            }
        }

        private void OnStoreClicked()
        {
            UpdateRadioButtons(storeButton);
            OpenItemsView(ItemScreenType.Perks, true, "Storage", "MAX OUT YOUR GEAR");
        }

        private void OnRaceClicked()
        {
            UpdateRadioButtons(raceButton);
            OpenMainView();
        }

        private void OnGarageClicked()
        {
            UpdateRadioButtons(garageButton);
            OpenItemsView(ItemScreenType.Gears, false, "Garage", "FIX AND REPAIR");
        }

        private void UpdateRadioButtons(Button activeBtn)
        {
            if (storeButton != null)
            {
                storeButton.interactable = (storeButton != activeBtn);
            }

            if (raceButton != null)
            {
                raceButton.interactable = (raceButton != activeBtn);
            }

            if (garageButton != null)
            {
                garageButton.interactable = (garageButton != activeBtn);
            }
        }

        private void OpenItemsView(ItemScreenType screenType, bool showBuyButton, string title, string subtitle)
        {
            EnsureItemsState();
            bool showUnownedItems = screenType == ItemScreenType.Perks;

            if (navigation != null && navigation.CurrentController is ItemsViewModel currentVm)
            {
                if (currentVm.Config.TypeToDisplay == screenType)
                {
                    return;
                }

                currentVm.Config.TypeToDisplay = screenType;
                currentVm.Config.ShowBuyButton = showBuyButton;
                currentVm.Config.ShowUnownedItems = showUnownedItems;
                currentVm.Config.Title = title;
                currentVm.Config.Subtitle = subtitle;

                itemsState.TypeToDisplay = screenType;
                itemsState.ShowBuyButton = showBuyButton;
                itemsState.ShowUnownedItems = showUnownedItems;
                itemsState.Title = title;
                itemsState.Subtitle = subtitle;

                currentVm.Refresh();
                return;
            }

            itemsState.TypeToDisplay = screenType;
            itemsState.ShowBuyButton = showBuyButton;
            itemsState.ShowUnownedItems = showUnownedItems;
            itemsState.Title = title;
            itemsState.Subtitle = subtitle;

            if (navigation != null)
            {
                navigation.Open(new ItemsViewModel(itemsState), true, new NavigationOptions() { CloseAllViews = true });
            }
            else
            {
                Debug.LogError("[ToolbarController] Navigation is required to open the Items view.");
            }
        }

        public void OpenMainView()
        {
            if (navigation == null)
            {
                Debug.LogError("[ToolbarController] Navigation is required to open the Main view.");
                return;
            }

            if (!(navigation.CurrentController is MainViewModel))
            {
                navigation.Open(new MainViewModel(), true, new NavigationOptions() { CloseAllViews = true });
            }
        }
    }
}
