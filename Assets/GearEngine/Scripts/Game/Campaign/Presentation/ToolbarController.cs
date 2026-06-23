using Scaffold.Navigation.Contracts;
using VContainer;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public class ToolbarController : MonoBehaviour
    {
        [Header("Botões")]
        [SerializeField] private Button storeButton;
        [SerializeField] private Button raceButton;
        [SerializeField] private Button garageButton;

        [Header("Configuração de Estado (Distinguir Telas)")]
        [Tooltip("ScriptableObject usado para definir o que o ItemsView vai exibir")]
        [SerializeField] private ItemsScreenState itemsState;

        private INavigation navigation;

        [Inject]
        public void Construct(INavigation navigation)
        {
            this.navigation = navigation;
        }

        private void OnEnable()
        {
            if (storeButton != null) storeButton.onClick.AddListener(OnStoreClicked);
            if (raceButton != null) raceButton.onClick.AddListener(OnRaceClicked);
            if (garageButton != null) garageButton.onClick.AddListener(OnGarageClicked);
        }

        private void OnDisable()
        {
            if (storeButton != null) storeButton.onClick.RemoveListener(OnStoreClicked);
            if (raceButton != null) raceButton.onClick.RemoveListener(OnRaceClicked);
            if (garageButton != null) garageButton.onClick.RemoveListener(OnGarageClicked);
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
            if (storeButton != null) storeButton.interactable = (storeButton != activeBtn);
            if (raceButton != null) raceButton.interactable = (raceButton != activeBtn);
            if (garageButton != null) garageButton.interactable = (garageButton != activeBtn);
        }

        private void OpenItemsView(ItemScreenType screenType, bool showBuyButton, string title, string subtitle)
        {
            EnsureItemsState();

            // Desativa a MainView manualmente sempre que formos para a ItemsView
            MainView mainView = FindObjectOfType<MainView>(true);
            if (mainView != null && mainView.gameObject.activeSelf) 
            {
                mainView.gameObject.SetActive(false);
            }

            // Se já estivermos na tela de itens, apenas atualizamos o estado e damos refresh
            if (navigation != null && navigation.CurrentController is ItemsViewModel currentVm)
            {
                if (currentVm.Config.TypeToDisplay == screenType)
                {
                    // Já está aberto neste exato estado, não fazemos nada
                    return;
                }

                // Muda o tipo da tela e atualiza a View sem abrir uma nova na pilha de navegação
                currentVm.Config.TypeToDisplay = screenType;
                currentVm.Config.ShowBuyButton = showBuyButton;
                currentVm.Config.Title = title;
                currentVm.Config.Subtitle = subtitle;
                
                // Mantém o estado interno do Toolbar atualizado também
                itemsState.TypeToDisplay = screenType;
                itemsState.ShowBuyButton = showBuyButton;
                itemsState.Title = title;
                itemsState.Subtitle = subtitle;

                currentVm.Refresh();
                return;
            }

            // Se não estiver aberta, configuramos e abrimos normalmente
            itemsState.TypeToDisplay = screenType;
            itemsState.ShowBuyButton = showBuyButton;
            itemsState.Title = title;
            itemsState.Subtitle = subtitle;

            // Destrói e cria o ItemsView usando o Navigation System do Scaffold
            if (navigation != null)
            {
                // Se a arquitetura usar um método diferente de Open (ex: Push, Pop), basta ajustar aqui.
                // O Navigation cuidará de instanciar o ItemsView associado a este ViewModel!
                navigation.Open(new ItemsViewModel(itemsState), true, new NavigationOptions() { CloseAllViews = false });
            }
            else
            {
                Debug.LogError("[ToolbarController] INavigation não foi injetado! O ItemsView não pôde ser criado.");
            }
        }

        public void OpenMainView()
        {
            this.gameObject.SetActive(true);

            // Ativa o MainView
            MainView mainView = FindObjectOfType<MainView>(true);
            if (mainView != null) 
            {
                mainView.gameObject.SetActive(true);
            }

            // Usa o sistema de navegação para retornar e limpar as views antigas
            if (navigation != null && !(navigation.CurrentController is MainViewModel))
            {
                navigation.Open(new MainViewModel(), true, new NavigationOptions() { CloseAllViews = true });
            }
        }
    }
}
