using System.Reflection;
using GameModule.Modules.Cards;
using GameModule.Modules.Currency;
using GameModule.Modules.Inventory;
using GameModule.Modules.Loadout;
using GameModule.Modules.Roguelike;
using GameModule.Modules.Tracks;
using GameModuleDTO.Modules.Cards;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.Modules.Loadout;
using GameModuleDTO.Modules.Roguelike;
using GameModuleDTO.Modules.Tracks;
using Xunit;

namespace LiveOps.Tests
{
    /// <summary>
    /// Ensures Cloud Code module Initialize Remote Config keys stay aligned with DTO type names (and with Unity <c>ConfigBuilderSO</c> defaults).
    /// </summary>
    public sealed class ConfigModuleKeyContractTests
    {
        [Fact]
        public void TracksModule_ConfigKey_matches_TrackConfig()
        {
            Assert.Equal(nameof(TrackConfig), TracksModule.ConfigKey);
        }

        [Fact]
        public void CardsModule_ConfigKey_matches_CardConfig()
        {
            Assert.Equal(nameof(CardConfig), CardsModule.ConfigKey);
        }

        [Fact]
        public void RoguelikeModule_ConfigKey_matches_RoguelikeConfig()
        {
            Assert.Equal(nameof(RoguelikeConfig), RoguelikeModule.ConfigKey);
        }

        [Fact]
        public void InventoryModule_ConfigKey_matches_InventoryConfig()
        {
            Assert.Equal(nameof(InventoryConfig), InventoryModule.ConfigKey);
        }

        [Fact]
        public void LoadoutModule_ConfigKey_matches_LoadoutConfig()
        {
            Assert.Equal(nameof(LoadoutConfig), LoadoutModule.ConfigKey);
        }

        [Fact]
        public void CurrencyModule_ConfigKey_matches_CurrencyConfig()
        {
            FieldInfo? f = typeof(CurrencyModule).GetField("ConfigKey", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(f);
            Assert.Equal(nameof(CurrencyConfig), f!.GetRawConstantValue());
        }
    }
}
