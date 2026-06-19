using Scaffold.Navigation.Contracts;
using VContainer;
using UnityEngine;
using UnityEngine.UI;
using Scaffold.Navigation;

namespace GearEngine.Campaign.Presentation
{
    public class ToolbarController : MonoBehaviour
    {
        [Header("Botões")]
        [SerializeField] private Button storeButton;
        [SerializeField] private Button raceButton;
        [SerializeField] private Button garageButton;

        // [Header("Configuração de Estado (Distinguir Telas)")]
        // Criado dinamicamente no código agora!

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

        private void OnStoreClicked()
        {
            var itemsState = ScriptableObject.CreateInstance<ItemsScreenState>();
            itemsState.TypeToDisplay = ItemScreenType.Perks;
            itemsState.ShowBuyButton = true;
            itemsState.ShowUnownedItems = true;

            OpenItemsView(itemsState);
        }

        private void OnRaceClicked()
        {
            // Para o MainView, apenas ativamos (sem recriar pelo navigation)
            ActivateMainView();
        }

        private void OnGarageClicked()
        {
            var itemsState = ScriptableObject.CreateInstance<ItemsScreenState>();
            itemsState.TypeToDisplay = ItemScreenType.Gears;
            itemsState.ShowBuyButton = false; 
            itemsState.ShowUnownedItems = true;

            OpenItemsView(itemsState);
        }

        private void OpenItemsView(ItemsScreenState itemsState)
        {
            // Desativa a MainView manualmente (já que a regra é activate/deactivate)
            var mainView = FindObjectOfType<MainView>(true);
            if (mainView != null) 
            {
                mainView.gameObject.SetActive(false);
            }

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

        private void ActivateMainView()
        {
            // Destrói o ItemsView (se existir) porque a regra é destruí-lo ao sair
            var itemsView = FindObjectOfType<ItemsView>(true);
            if (itemsView != null)
            {
                // Se o Navigation System tiver um jeito próprio de fechar Views específicas, o ideal seria usar navigation.Close()
                // Mas Destroy funciona caso não importe para o sistema de histórico.
                Destroy(itemsView.gameObject);
            }

            // Ativa o MainView
            var mainView = FindObjectOfType<MainView>(true);
            if (mainView != null) 
            {
                mainView.gameObject.SetActive(true);
            }
        }
    }
}
