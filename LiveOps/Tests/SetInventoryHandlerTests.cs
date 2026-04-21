using System.Collections.Generic;
using System.Linq;
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
            var request = new SetInventoryRequest(new[]
            {
                new OwnedGearEntry { InstanceId = "i1", GearId = "g1" },
                new OwnedGearEntry { InstanceId = "i2", GearId = "g2" }
            });

            SetInventoryResponse response = await handler.HandleAsync(session, request).ConfigureAwait(false);

            Assert.Equal(2, response.Gears.Count);
            Assert.Equal("i1", response.Gears[0].InstanceId);
            Assert.Equal("g1", response.Gears[0].GearId);
            Assert.Equal("i2", response.Gears[1].InstanceId);
            player.Verify(
                p => p.Set(
                    context.Object,
                    InventoryModule.PersistenceKey,
                    It.Is<InventoryPersistence>(pp => pp.Gears.Count == 2 && pp.Gears[0].GearId == "g1" && pp.Gears[1].GearId == "g2"),
                    false),
                Times.Once);
        }
    }
}
