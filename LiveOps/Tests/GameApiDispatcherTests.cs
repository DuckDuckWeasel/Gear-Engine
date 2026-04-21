using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModule.Signal;
using GameModuleDTO.GameApi;
using GameModuleDTO.ModuleRequests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Core;
using Xunit;

namespace LiveOps.Tests
{
    /// <summary>Test-only request/response for dispatcher smoke tests (same assembly as <see cref="TestEchoHandler"/>).</summary>
    public sealed class TestEchoRequest : ModuleRequest<TestEchoResponse>
    {
        public string Message { get; set; } = string.Empty;
    }

    public sealed class TestEchoResponse : ModuleResponse
    {
        public string Echo { get; set; } = string.Empty;
    }

    public sealed class TestEchoHandler : IGameApiHandler<TestEchoRequest, TestEchoResponse>
    {
        public Task<TestEchoResponse> HandleAsync(GameApiSession session, TestEchoRequest request)
        {
            return Task.FromResult(new TestEchoResponse { Echo = request?.Message ?? string.Empty });
        }
    }

    public sealed class GameApiDispatcherTests
    {
        [Fact]
        public async Task Invoke_DispatchesToConcreteHandler_AndCallsFlushDirty()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILogger<GameApiDispatcher>>(_ => NullLogger<GameApiDispatcher>.Instance);
            GameApiRegistry registry = new GameApiRegistry(typeof(TestEchoHandler).Assembly);
            services.AddSingleton(registry);
            services.AddScoped<TestEchoHandler>();
            services.AddScoped<SignalModule>();
            services.AddScoped<GameApiDispatcher>();

            await using ServiceProvider sp = services.BuildServiceProvider();

            GameApiDispatcher dispatcher = sp.GetRequiredService<GameApiDispatcher>();
            Mock<IExecutionContext> context = new Mock<IExecutionContext>();
            Mock<IPlayerData> player = new Mock<IPlayerData>();
            player.Setup(p => p.FlushDirtyAsync(context.Object)).Returns(Task.CompletedTask).Verifiable();
            Mock<IGameState> gameState = new Mock<IGameState>();
            Mock<IRemoteConfig> remote = new Mock<IRemoteConfig>();

            GameApiEnvelopeRequest envelope = new GameApiEnvelopeRequest
            {
                RequestKey = nameof(TestEchoRequest),
                Payload = JObject.FromObject(
                    new TestEchoRequest { Message = "hi" },
                    JsonSerializer.Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })),
            };

            GameApiEnvelopeResponse resp = await dispatcher.Invoke(
                context.Object,
                player.Object,
                gameState.Object,
                remote.Object,
                envelope).ConfigureAwait(false);

            Assert.Equal(ResponseStatusType.Success, resp.StatusType);
            Assert.NotNull(resp.Result);
            TestEchoResponse typed = Assert.IsType<TestEchoResponse>(resp.Result);
            Assert.Equal("hi", typed.Echo);
            player.Verify(p => p.FlushDirtyAsync(context.Object), Times.Once);
        }
    }
}
