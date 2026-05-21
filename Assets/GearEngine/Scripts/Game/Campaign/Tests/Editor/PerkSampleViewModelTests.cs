using System;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.Campaign.Bootstrap.Perks;
using GearEngine.Perks;
using GearEngine.Currency;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;
using GearEngine.Perks.Config;
using PerkGameData = LiveOps.Modules.DTO.Perks.PerkGameData;
using PerkPersistence = LiveOps.Modules.DTO.Perks.PerkPersistence;

namespace GearEngine.Perks.Tests.Editor
{
    public sealed class PerkSampleViewModelTests
    {
        [Test]
        public void RefreshDisplay_UsesCurrencyWalletGold()
        {
            PerkCatalogSO catalog = ScriptableObject.CreateInstance<PerkCatalogSO>();
            PerkGameData perkGameData = BuildPerkGameData(Array.Empty<string>());
            CurrencyGameData currencyGameData = BuildCurrencyGameData(77);
            var fake = new FakeLiveOpsService
            {
                PerkGameData = perkGameData,
                CurrencyGameData = currencyGameData,
            };

            IObjectResolver container = BuildContainer(fake, catalog);
            try
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                PerksClientModule perks = container.Resolve<PerksClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
                perks.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                var viewModel = container.Resolve<PerkSampleViewModel>();
                viewModel.RefreshDisplay();

                Assert.That(viewModel.Gold, Is.EqualTo(77));
                Assert.That(viewModel.NextCost, Is.EqualTo(perkGameData.NextCost));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        private static PerkGameData BuildPerkGameData(string[] unlocked)
        {
            var persistence = new PerkPersistence();
            persistence.Unlocked.AddRange(unlocked);
            PerkConfig config = JsonConvert.DeserializeObject<PerkConfig>(
                "{\"catalog\":[\"a\"],\"baseCost\":10,\"costPerPurchaseGrowth\":5}");
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

        private static IObjectResolver BuildContainer(FakeLiveOpsService fake, PerkCatalogSO catalog)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(fake);
            builder.RegisterInstance(catalog);
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            builder.Register<PerksClientModule>(Lifetime.Singleton);
            builder.Register<PerkSampleViewModel>(Lifetime.Transient);
            return builder.Build();
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public PerkGameData PerkGameData { get; set; }

            public CurrencyGameData CurrencyGameData { get; set; }

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
                throw new InvalidOperationException("Not used in this test.");
            }
        }
    }
}
