using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Offline;
using LiveOps.DTO.GameModule;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class OfflineLiveOpsServiceTests
    {
        [Test]
        public void GetModuleData_ReturnsRegisteredModule()
        {
            CurrencyGameData currency = BuildCurrency(gold: 250);
            var service = new OfflineLiveOpsService(new Dictionary<Type, IGameModuleData>
            {
                [typeof(CurrencyGameData)] = currency,
            });

            CurrencyGameData resolved = service.GetModuleData<CurrencyGameData>();

            Assert.That(resolved, Is.SameAs(currency));
            Assert.That(resolved.GetWallet("gold").Current, Is.EqualTo(250));
        }

        [Test]
        public void GetModuleData_UnknownType_ReturnsNull()
        {
            var service = new OfflineLiveOpsService(new Dictionary<Type, IGameModuleData>());

            Assert.That(service.GetModuleData<CurrencyGameData>(), Is.Null);
        }

        [Test]
        public async Task CallAsync_AddCurrency_AppliesMutationInMemory()
        {
            CurrencyGameData currency = BuildCurrency(gold: 100);
            var service = new OfflineLiveOpsService(new Dictionary<Type, IGameModuleData>
            {
                [typeof(CurrencyGameData)] = currency,
            });

            AddCurrencyResponse response = await service.CallAsync(new AddCurrencyRequest("gold", 75), CancellationToken.None);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.NewAmount, Is.EqualTo(175));
            Assert.That(response.Diff, Is.EqualTo(75));
            Assert.That(currency.GetWallet("gold").Current, Is.EqualTo(175), "Wallet should be mutated in-memory for the session.");
        }

        [Test]
        public async Task CallAsync_SpendCurrency_FailsWhenInsufficient()
        {
            CurrencyGameData currency = BuildCurrency(gold: 30);
            var service = new OfflineLiveOpsService(new Dictionary<Type, IGameModuleData>
            {
                [typeof(CurrencyGameData)] = currency,
            });

            SpendCurrencyResponse response = await service.CallAsync(new SpendCurrencyRequest("gold", 50), CancellationToken.None);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Succeeded, Is.False);
            Assert.That(response.NewAmount, Is.EqualTo(30), "Wallet stays untouched when spend cannot succeed.");
            Assert.That(currency.GetWallet("gold").Current, Is.EqualTo(30));
        }

        [Test]
        public async Task CallAsync_UnknownRequest_ReturnsDefaultResponse()
        {
            var service = new OfflineLiveOpsService(new Dictionary<Type, IGameModuleData>());

            SetInventoryResponse response = await service.CallAsync(new SetInventoryRequest(), CancellationToken.None);

            // No InventoryGameData registered, but the response should still come back so callers don't NRE.
            Assert.That(response, Is.Not.Null);
        }

        private static CurrencyGameData BuildCurrency(long gold)
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", gold);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0}]}");
            return new CurrencyGameData(persistence, config);
        }
    }
}
