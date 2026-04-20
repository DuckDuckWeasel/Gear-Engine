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
        public void TryGet_FindsAddGoldRequest_WithHandlerType()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddGoldHandler).Assembly);
            Assert.True(registry.Contains("AddGoldRequest"));
            Assert.True(registry.TryGet("AddGoldRequest", out HandlerEntry? entry));
            Assert.NotNull(entry);
            Assert.Equal(typeof(AddGoldRequest), entry!.RequestType);
            Assert.Equal(typeof(GoldChangedResponse), entry.ResponseType);
            Assert.Equal(typeof(AddGoldHandler), entry.HandlerType);
        }

        [Fact]
        public void TryResolve_FindsAddGoldRequest()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddGoldHandler).Assembly);
            Assert.True(registry.TryResolve("AddGoldRequest", out Type reqType, out Type resType));
            Assert.Equal(typeof(AddGoldRequest), reqType);
            Assert.Equal(typeof(GoldChangedResponse), resType);
        }

        [Fact]
        public void TryGet_UnknownKey_ReturnsFalse()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddGoldHandler).Assembly);
            Assert.False(registry.TryGet("NonExistentRequest", out HandlerEntry? entry));
            Assert.Null(entry);
        }
    }
}
