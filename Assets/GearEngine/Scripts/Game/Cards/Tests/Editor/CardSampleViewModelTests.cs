using System;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Cards;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.ModuleRequests;
using GearEngine.App.Bootstrap.Cards;
using GearEngine.Cards;
using GearEngine.Currency;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Cards.Tests.Editor
{
    public sealed class CardSampleViewModelTests
    {
        [Test]
        public void RefreshDisplay_UsesCurrencyWalletGold()
        {
            CardCatalogSO catalog = ScriptableObject.CreateInstance<CardCatalogSO>();
            CardGameData cardGameData = BuildCardGameData(Array.Empty<string>());
            CurrencyGameData currencyGameData = BuildCurrencyGameData(77);
            var fake = new FakeLiveOpsService
            {
                CardGameData = cardGameData,
                CurrencyGameData = currencyGameData,
            };

            IObjectResolver container = BuildContainer(fake, catalog);
            try
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                CardsClientModule cards = container.Resolve<CardsClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
                cards.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                var viewModel = container.Resolve<CardSampleViewModel>();
                viewModel.RefreshDisplay();

                Assert.That(viewModel.Gold, Is.EqualTo(77));
                Assert.That(viewModel.NextCost, Is.EqualTo(cardGameData.NextCost));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        private static CardGameData BuildCardGameData(string[] unlocked)
        {
            var persistence = new CardPersistence();
            persistence.Unlocked.AddRange(unlocked);
            CardConfig config = JsonConvert.DeserializeObject<CardConfig>(
                "{\"catalog\":[\"a\"],\"currencyId\":\"gold\",\"baseCost\":10,\"costPerPurchaseGrowth\":5}");
            return new CardGameData(persistence, config);
        }

        private static CurrencyGameData BuildCurrencyGameData(long gold)
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", gold);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0}]}");
            return new CurrencyGameData(persistence, config);
        }

        private static IObjectResolver BuildContainer(FakeLiveOpsService fake, CardCatalogSO catalog)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(fake);
            builder.RegisterInstance(catalog);
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            builder.Register<CardsClientModule>(Lifetime.Singleton);
            builder.Register<CardSampleViewModel>(Lifetime.Transient);
            return builder.Build();
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public CardGameData CardGameData { get; set; }

            public CurrencyGameData CurrencyGameData { get; set; }

            public T GetModuleData<T>()
                where T : class, IGameModuleData
            {
                if (typeof(T) == typeof(CardGameData))
                {
                    return CardGameData as T;
                }

                if (typeof(T) == typeof(CurrencyGameData))
                {
                    return CurrencyGameData as T;
                }

                return null;
            }

            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                throw new InvalidOperationException("Not used in this test.");
            }
        }
    }
}
