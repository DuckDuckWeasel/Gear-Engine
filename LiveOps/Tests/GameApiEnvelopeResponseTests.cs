using System;
using GameModuleDTO.GameApi;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Gold;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class GameApiEnvelopeResponseTests
    {
        [Fact]
        public void Exception_SetsStatusAndMessage()
        {
            GameApiEnvelopeResponse envelope = GameApiEnvelopeResponse.Exception("AddGoldRequest", new InvalidOperationException("test failure"));
            Assert.Equal("AddGoldRequest", envelope.RequestKey);
            Assert.Equal(ResponseStatusType.Exception, envelope.StatusType);
            Assert.Equal("test failure", envelope.Message);
        }

        [Fact]
        public void Success_CopiesResultAndNested()
        {
            GoldChangedResponse result = new GoldChangedResponse(10, 2);
            GameApiEnvelopeResponse envelope = GameApiEnvelopeResponse.Success("AddGoldRequest", result, null);
            Assert.Equal(ResponseStatusType.Success, envelope.StatusType);
            Assert.Same(result, envelope.Result);
            Assert.NotNull(envelope.NestedResponses);
        }
    }
}
