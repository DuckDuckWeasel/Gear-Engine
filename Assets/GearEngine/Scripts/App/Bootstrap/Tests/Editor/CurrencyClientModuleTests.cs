using System;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.ModuleRequests;
using GearEngine.Currency;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using VContainer;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class CurrencyClientModuleTests
    {
        [Test]
        public async Task AddAsync_UpdatesCachedWallet_FromResponse()
        {
            CurrencyGameData gameData = BuildGameData(gold: 5);
            var fake = new FakeLiveOpsService
            {
                ModuleData = gameData,
                CallImpl = (_, _) => new AddCurrencyResponse("gold", 35, 30),
            };

            IObjectResolver container = BuildContainer(fake);
            try
            {
                CurrencyClientModule module = container.Resolve<CurrencyClientModule>();
                await module.InitializeAsync(CancellationToken.None);

                AddCurrencyResponse response = await module.AddAsync("gold", 30, CancellationToken.None);

                Assert.That(response.NewAmount, Is.EqualTo(35));
                Assert.That(module.GetWallet("gold").Current, Is.EqualTo(35));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        [Test]
        public async Task TrySpendAsync_True_UpdatesWallet()
        {
            CurrencyGameData gameData = BuildGameData(gold: 20);
            var fake = new FakeLiveOpsService
            {
                ModuleData = gameData,
                CallImpl = (_, _) => new SpendCurrencyResponse("gold", 15, 5, true),
            };

            IObjectResolver container = BuildContainer(fake);
            try
            {
                CurrencyClientModule module = container.Resolve<CurrencyClientModule>();
                await module.InitializeAsync(CancellationToken.None);

                bool ok = await module.TrySpendAsync("gold", 5, CancellationToken.None);

                Assert.That(ok, Is.True);
                Assert.That(module.GetWallet("gold").Current, Is.EqualTo(15));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        [Test]
        public async Task TrySpendAsync_False_DoesNotChangeWallet()
        {
            CurrencyGameData gameData = BuildGameData(gold: 10);
            var fake = new FakeLiveOpsService
            {
                ModuleData = gameData,
                CallImpl = (_, _) => new SpendCurrencyResponse("gold", 10, 0, false),
            };

            IObjectResolver container = BuildContainer(fake);
            try
            {
                CurrencyClientModule module = container.Resolve<CurrencyClientModule>();
                await module.InitializeAsync(CancellationToken.None);

                bool ok = await module.TrySpendAsync("gold", 50, CancellationToken.None);

                Assert.That(ok, Is.False);
                Assert.That(module.GetWallet("gold").Current, Is.EqualTo(10));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        [Test]
        public async Task AddAsync_InvalidCurrencyId_Throws()
        {
            var fake = new FakeLiveOpsService { ModuleData = BuildGameData(0) };
            IObjectResolver container = BuildContainer(fake);
            try
            {
                CurrencyClientModule module = container.Resolve<CurrencyClientModule>();
                await module.InitializeAsync(CancellationToken.None);
                try
                {
                    await module.AddAsync("", 1, CancellationToken.None);
                    Assert.Fail("Expected ArgumentException");
                }
                catch (ArgumentException)
                {
                }
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        [Test]
        public async Task AddAsync_NonPositiveAmount_Throws()
        {
            var fake = new FakeLiveOpsService { ModuleData = BuildGameData(0) };
            IObjectResolver container = BuildContainer(fake);
            try
            {
                CurrencyClientModule module = container.Resolve<CurrencyClientModule>();
                await module.InitializeAsync(CancellationToken.None);
                try
                {
                    await module.AddAsync("gold", 0, CancellationToken.None);
                    Assert.Fail("Expected ArgumentOutOfRangeException");
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        private static CurrencyGameData BuildGameData(long gold)
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
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            return builder.Build();
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public CurrencyGameData ModuleData { get; set; }

            public Func<object, CancellationToken, ModuleResponse> CallImpl { get; set; }

            public T GetModuleData<T>()
                where T : class, IGameModuleData
            {
                return ModuleData as T;
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
