using GameModuleDTO.Modules.Currency;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class CurrencyGameDataTests
    {
        [Test]
        public void Constructor_ProjectsWallets_FromPersistence()
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", 10);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0}]}");

            var data = new CurrencyGameData(persistence, config);

            Assert.That(data.Wallets.Count, Is.EqualTo(1));
            CurrencyWallet w = data.GetWallet("gold");
            Assert.That(w, Is.Not.Null);
            Assert.That(w.Current, Is.EqualTo(10));
            Assert.That(w.Min, Is.Null);
            Assert.That(w.Max, Is.Null);
        }

        [Test]
        public void Constructor_ReflectsUncappedStoredValue_NotClampedByGameData()
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gem", 999);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gem\",\"initial\":0,\"max\":100}]}");

            var data = new CurrencyGameData(persistence, config);

            Assert.That(data.GetWallet("gem").Current, Is.EqualTo(999));
        }

        [Test]
        public void Constructor_SkipsConfiguredId_WhenPersistenceEntryMissing()
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", 1);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0},{\"id\":\"xp\",\"initial\":0}]}");

            var data = new CurrencyGameData(persistence, config);

            Assert.That(data.GetWallet("gold"), Is.Not.Null);
            Assert.That(data.GetWallet("xp"), Is.Null);
        }

        [Test]
        public void Wallet_CanSpend_RespectsMinFloor()
        {
            var w = new CurrencyWallet { Current = 10, Min = 5 };
            Assert.That(w.CanSpend(5), Is.True);
            Assert.That(w.CanSpend(6), Is.False);
        }

        [Test]
        public void Wallet_Serialize_OmitsNullMinMax_AndNoInitial()
        {
            var w = new CurrencyWallet { Id = "gold", Current = 7 };
            string json = JsonConvert.SerializeObject(w);
            Assert.That(json.Contains("\"min\""), Is.False);
            Assert.That(json.Contains("\"max\""), Is.False);
            Assert.That(json.Contains("\"initial\""), Is.False);
        }

        [Test]
        public void Wallet_Serialize_IncludesMinWhenSet()
        {
            var w = new CurrencyWallet { Id = "gold", Current = 7, Min = 0L };
            string json = JsonConvert.SerializeObject(w);
            Assert.That(json.Contains("\"min\":0"), Is.True);
        }
    }
}
