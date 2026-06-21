using GearEngine.Currency;
using GearEngine.Currency.Events;
using Scaffold.Events.Contracts;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Presentation
{
    public class CurrencyDisplayWidget : MonoBehaviour
    {
        [Tooltip("Texto onde o valor da moeda será exibido")]
        [SerializeField] private TMP_Text currencyText;
        
        [Tooltip("ID da moeda que queremos exibir e escutar (ex: gold)")]
        [SerializeField] private string currencyId = "gold";

        private CurrencyClientModule currencyClient;
        private IEventBus eventBus;
        private bool isConstructed;

        private void Awake()
        {
            // Se o VContainer não o injetou automaticamente (ex: instanciado dinamicamente ou sem estar no Scope)
            if (!isConstructed)
            {
                LifetimeScope scope = FindObjectOfType<LifetimeScope>();
                if (scope != null && scope.Container != null)
                {
                    scope.Container.Inject(this);
                }
                else
                {
                    Debug.LogWarning("[CurrencyDisplayWidget] Nenhum LifetimeScope encontrado na cena para auto-injeção!");
                }
            }
        }

        [Inject]
        public void Construct(CurrencyClientModule currencyClient, IEventBus eventBus)
        {
            if (isConstructed) return;
            
            this.currencyClient = currencyClient;
            this.eventBus = eventBus;
            this.isConstructed = true;
            
            UpdateText(this.currencyClient.GetWallet(currencyId)?.Current ?? 0);
            
            this.eventBus.AddListener<CurrencyUpdatedEvent>(OnCurrencyUpdated);
            Debug.Log($"[CurrencyDisplayWidget] Inicializado com sucesso! Moeda atual: {currencyId}");
        }

        private void OnDestroy()
        {
            if (eventBus != null)
            {
                eventBus.RemoveListener<CurrencyUpdatedEvent>(OnCurrencyUpdated);
            }
        }

        private void OnCurrencyUpdated(CurrencyUpdatedEvent evt)
        {
            if (evt.CurrencyId == currencyId)
            {
                UpdateText(evt.NewAmount);
            }
        }

        private void UpdateText(long amount)
        {
            if (currencyText != null)
            {
                currencyText.text = amount.ToString();
            }
        }
    }
}
