using System;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Cards.Bootstrap
{
    public sealed class CardsBootstrap : MonoBehaviour, IInitializable
    {
        [Inject]
        private CardCatalogSO catalog;

        [Inject]
        private INavigation navigation;

        public void Initialize()
        {
            try
            {
                if (catalog == null)
                {
                    throw new InvalidOperationException("[CardsBootstrap] CardCatalogSO is missing.");
                }

                navigation.Open(new CardSampleViewModel(catalog));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardsBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
