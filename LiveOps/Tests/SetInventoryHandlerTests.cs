using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModule.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Inventory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Unity.Services.CloudCode.Core;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class SetInventoryHandlerTests
    {
        [Fact]
        public async Task HandleAsync_Calls_Player_Set_So_Untracked_Default_Persistence_Is_Written()
        {
            Mock<IExecutionContext> context = new Mock<IExecutionContext>();
            Mock<IPlayerData> player = new Mock<IPlayerData>();
            InventoryPersistence loaded = new InventoryPersistence();
            player.Setup(p => p.Get(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>()))
                .ReturnsAsync(loaded);
            player.Setup(p => p.Set(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
            GameApiRegistry registry = new GameApiRegistry(typeof(SetInventoryHandler).Assembly);
            GameApiSession session = new GameApiSession(
                services,
                registry,
                context.Object,
                player.Object,
                Mock.Of<IGameState>(),
                Mock.Of<IRemoteConfig>());

            SetInventoryHandler handler = new SetInventoryHandler();
            SetInventoryRequest request = new SetInventoryRequest(new[] { "g1", "g2" });

            SetInventoryResponse response = await handler.HandleAsync(session, request).ConfigureAwait(false);

            Assert.Equal(2, response.GearIds.Count);
            Assert.Equal("g1", response.GearIds[0]);
            Assert.Equal("g2", response.GearIds[1]);
            player.Verify(
                p => p.Set(
                    context.Object,
                    InventoryModule.PersistenceKey,
                    It.Is<InventoryPersistence>(pp => pp.GearIds.Count == 2 && pp.GearIds[0] == "g1" && pp.GearIds[1] == "g2"),
                    false),
                Times.Once);
        }
    }
}
