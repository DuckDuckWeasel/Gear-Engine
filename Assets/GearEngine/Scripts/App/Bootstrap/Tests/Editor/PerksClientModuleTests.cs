using System;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.Campaign.Bootstrap.Perks;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using VContainer;
using PerkGameData = LiveOps.Modules.DTO.Perks.PerkGameData;
using PerkPersistence = LiveOps.Modules.DTO.Perks.PerkPersistence;
using PurchasePerkResponse = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkResponse;
using PurchasePerkRequest = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkRequest;
using BurnPerkResponse = LiveOps.Modules.DTO.ModuleRequests.BurnPerkResponse;
using BurnPerkRequest = LiveOps.Modules.DTO.ModuleRequests.BurnPerkRequest;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class PerksClientModuleTests
    {
        [Test]
        public async Task PurchaseAsync_WhenServerSucceeds_AppendsUnlockedAndUpdatesNextCost()
        {
            PerkGameData perkGameData = BuildPerkGameData(catalog: new[] { "c1", "c2" }, unlocked: Array.Empty<string>());
            CurrencyGameData currencyGameData = BuildCurrencyGameData(100);
            var fake = new FakeLiveOpsService
            {
                PerkGameData = perkGameData,
                CurrencyGameData = currencyGameData,
                CallImpl = (_, _) => new PurchasePerkResponse
                {
                    Success = true,
                    UnlockedPerkId = "c1",
                    Cost = 10,
                    NextCost = 60,
                },
            };

            IObjectResolver container = BuildContainer(fake);
            try
            {
                PerksClientModule module = container.Resolve<PerksClientModule>();
                await module.InitializeAsync(CancellationToken.None);

                PurchasePerkResponse response = await module.PurchaseAsync(CancellationToken.None);

                Assert.That(response.Success, Is.True);
                Assert.That(response.UnlockedPerkId, Is.EqualTo("c1"));
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
            PerkGameData perkGameData = BuildPerkGameData(catalog: new[] { "c1" }, unlocked: Array.Empty<string>());
            var fake = new FakeLiveOpsService
            {
                PerkGameData = perkGameData,
                CurrencyGameData = BuildCurrencyGameData(0),
                CallImpl = (_, _) => new PurchasePerkResponse
                {
                    Success = false,
                    NextCost = 100,
                    Cost = 100,
                },
            };

            IObjectResolver container = BuildContainer(fake);
            try
            {
                PerksClientModule module = container.Resolve<PerksClientModule>();
                await module.InitializeAsync(CancellationToken.None);
                int countBefore = module.Unlocked.Count;

                PurchasePerkResponse response = await module.PurchaseAsync(CancellationToken.None);

                Assert.That(response.Success, Is.False);
                Assert.That(module.Unlocked.Count, Is.EqualTo(countBefore));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        private static PerkGameData BuildPerkGameData(string[] catalog, string[] unlocked)
        {
            var persistence = new PerkPersistence();
            persistence.Unlocked.AddRange(unlocked);
            PerkConfig config = JsonConvert.DeserializeObject<PerkConfig>(
                $"{{\"catalog\":{JsonConvert.SerializeObject(catalog)},\"baseCost\":100,\"costPerPurchaseGrowth\":50}}");
            return new PerkGameData(persistence, config);
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
            builder.Register<PerksClientModule>(Lifetime.Singleton);
            return builder.Build();
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public PerkGameData PerkGameData { get; set; }

            public CurrencyGameData CurrencyGameData { get; set; }

            public Func<object, CancellationToken, ModuleResponse> CallImpl { get; set; }

            public T GetModuleData<T>()
                where T : class, IGameModuleData
            {
                if (typeof(T) == typeof(PerkGameData))
                {
                    return PerkGameData as T;
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
