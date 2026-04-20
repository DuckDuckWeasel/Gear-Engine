using System;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Cards;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Bootstrap.Cards;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using VContainer;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class CardsClientModuleTests
    {
        [Test]
        public async Task PurchaseAsync_WhenServerSucceeds_AppendsUnlockedAndUpdatesNextCost()
        {
            CardGameData cardGameData = BuildCardGameData(catalog: new[] { "c1", "c2" }, unlocked: Array.Empty<string>());
            CurrencyGameData currencyGameData = BuildCurrencyGameData(100);
            var fake = new FakeLiveOpsService
            {
                CardGameData = cardGameData,
                CurrencyGameData = currencyGameData,
                CallImpl = (_, _) => new PurchaseCardResponse
                {
                    Success = true,
                    UnlockedCardId = "c1",
                    Cost = 10,
                    NextCost = 60,
                },
            };

            IObjectResolver container = BuildContainer(fake);
            try
            {
                CardsClientModule module = container.Resolve<CardsClientModule>();
                await module.InitializeAsync(CancellationToken.None);

                PurchaseCardResponse response = await module.PurchaseAsync(CancellationToken.None);

                Assert.That(response.Success, Is.True);
                Assert.That(response.UnlockedCardId, Is.EqualTo("c1"));
                Assert.That(module.Unlocked, Does.Contain("c1"));
                Assert.That(module.NextCost, Is.EqualTo(60));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        [Test]
        public async Task PurchaseAsync_WhenServerFails_DoesNotAppendUnlocked()
        {
            CardGameData cardGameData = BuildCardGameData(catalog: new[] { "c1" }, unlocked: Array.Empty<string>());
            var fake = new FakeLiveOpsService
            {
                CardGameData = cardGameData,
                CurrencyGameData = BuildCurrencyGameData(0),
                CallImpl = (_, _) => new PurchaseCardResponse
                {
                    Success = false,
                    NextCost = 100,
                    Cost = 100,
                },
            };

            IObjectResolver container = BuildContainer(fake);
            try
            {
                CardsClientModule module = container.Resolve<CardsClientModule>();
                await module.InitializeAsync(CancellationToken.None);
                int countBefore = module.Unlocked.Count;

                PurchaseCardResponse response = await module.PurchaseAsync(CancellationToken.None);

                Assert.That(response.Success, Is.False);
                Assert.That(module.Unlocked.Count, Is.EqualTo(countBefore));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        private static CardGameData BuildCardGameData(string[] catalog, string[] unlocked)
        {
            var persistence = new CardPersistence();
            persistence.Unlocked.AddRange(unlocked);
            CardConfig config = JsonConvert.DeserializeObject<CardConfig>(
                $"{{\"catalog\":{JsonConvert.SerializeObject(catalog)},\"currencyId\":\"gold\",\"baseCost\":100,\"costPerPurchaseGrowth\":50}}");
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

        private static IObjectResolver BuildContainer(FakeLiveOpsService fake)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(fake);
            builder.Register<CardsClientModule>(Lifetime.Singleton);
            return builder.Build();
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public CardGameData CardGameData { get; set; }

            public CurrencyGameData CurrencyGameData { get; set; }

            public Func<object, CancellationToken, ModuleResponse> CallImpl { get; set; }

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
                if (CallImpl == null)
                {
                    throw new InvalidOperationException("CallImpl not set");
                }

                ModuleResponse result = CallImpl((object)request, cancellationToken);
                return Task.FromResult((TResponse)result);
            }
        }
    }
}
