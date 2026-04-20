using System;
using GameModuleDTO.GameApi;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Currency;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class GameApiEnvelopeResponseTests
    {
        [Fact]
        public void Exception_SetsStatusAndMessage()
        {
            GameApiEnvelopeResponse envelope = GameApiEnvelopeResponse.Exception("AddCurrencyRequest", new InvalidOperationException("test failure"));
            Assert.Equal("AddCurrencyRequest", envelope.RequestKey);
            Assert.Equal(ResponseStatusType.Exception, envelope.StatusType);
            Assert.Equal("test failure", envelope.Message);
        }

        [Fact]
        public void Success_CopiesResultAndNested()
        {
            AddCurrencyResponse result = new AddCurrencyResponse("gold", 10, 2);
            GameApiEnvelopeResponse envelope = GameApiEnvelopeResponse.Success("AddCurrencyRequest", result, null);
            Assert.Equal(ResponseStatusType.Success, envelope.StatusType);
            Assert.Same(result, envelope.Result);
            Assert.NotNull(envelope.NestedResponses);
        }
    }
}
