using System;
using GameModule.GameApi;
using GameModule.Modules.Gold;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Gold;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class GameApiRegistryTests
    {
        [Fact]
        public void TryResolve_FindsAddGoldRequest()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddGoldHandler).Assembly);
            Assert.True(registry.Contains("AddGoldRequest"));
            Assert.True(registry.TryResolve("AddGoldRequest", out Type reqType, out Type resType));
            Assert.Equal(typeof(AddGoldRequest), reqType);
            Assert.Equal(typeof(GoldChangedResponse), resType);
        }

        [Fact]
        public void TryResolve_UnknownKey_ReturnsFalse()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddGoldHandler).Assembly);
            Assert.False(registry.TryResolve("NonExistentRequest", out _, out _));
        }
    }
}
