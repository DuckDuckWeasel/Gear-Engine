using GameModuleDTO.Modules.Currency;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class CurrencyPersistenceSeederTests
    {
        [Test]
        public void SeedAndClampInPlace_MissingId_SeedsInitial_AndReturnsDirty()
        {
            var persistence = new CurrencyPersistence();
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":25}]}");

            bool dirty = CurrencyPersistenceSeeder.SeedAndClampInPlace(persistence, config);

            Assert.That(dirty, Is.True);
            Assert.That(persistence.TryGet("gold", out long v), Is.True);
            Assert.That(v, Is.EqualTo(25));
        }

        [Test]
        public void SeedAndClampInPlace_AboveMax_Clamps_AndReturnsDirty()
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", 1_000_000);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0,\"max\":100}]}");

            bool dirty = CurrencyPersistenceSeeder.SeedAndClampInPlace(persistence, config);

            Assert.That(dirty, Is.True);
            Assert.That(persistence.TryGet("gold", out long v), Is.True);
            Assert.That(v, Is.EqualTo(100));
        }

        [Test]
        public void SeedAndClampInPlace_InBounds_ReturnsClean()
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", 50);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0,\"min\":0,\"max\":100}]}");

            bool dirty = CurrencyPersistenceSeeder.SeedAndClampInPlace(persistence, config);

            Assert.That(dirty, Is.False);
            Assert.That(persistence.TryGet("gold", out long v), Is.True);
            Assert.That(v, Is.EqualTo(50));
        }
    }
}
