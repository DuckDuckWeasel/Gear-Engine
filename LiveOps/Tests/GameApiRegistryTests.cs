using System;
using GameModule.GameApi;
using GameModule.Modules.Currency;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Currency;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class GameApiRegistryTests
    {
        [Fact]
        public void TryGet_FindsAddCurrencyRequest_WithHandlerType()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddCurrencyHandler).Assembly);
            Assert.True(registry.Contains("AddCurrencyRequest"));
            Assert.True(registry.TryGet("AddCurrencyRequest", out HandlerEntry? entry));
            Assert.NotNull(entry);
            Assert.Equal(typeof(AddCurrencyRequest), entry!.RequestType);
            Assert.Equal(typeof(AddCurrencyResponse), entry.ResponseType);
            Assert.Equal(typeof(AddCurrencyHandler), entry.HandlerType);
        }

        [Fact]
        public void TryResolve_FindsAddCurrencyRequest()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddCurrencyHandler).Assembly);
            Assert.True(registry.TryResolve("AddCurrencyRequest", out Type reqType, out Type resType));
            Assert.Equal(typeof(AddCurrencyRequest), reqType);
            Assert.Equal(typeof(AddCurrencyResponse), resType);
        }

        [Fact]
        public void TryGet_UnknownKey_ReturnsFalse()
        {
            GameApiRegistry registry = new GameApiRegistry(typeof(AddCurrencyHandler).Assembly);
            Assert.False(registry.TryGet("NonExistentRequest", out HandlerEntry? entry));
            Assert.Null(entry);
        }
    }
}
