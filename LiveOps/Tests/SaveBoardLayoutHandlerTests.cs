using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModule.Modules.Inventory;
using GameModule.Modules.Loadout;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.Modules.Loadout;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Unity.Services.CloudCode.Core;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class SaveBoardLayoutHandlerTests
    {
        [Fact]
        public async Task HandleAsync_Calls_Player_Set_So_Untracked_Default_Persistence_Is_Written()
        {
            Mock<IExecutionContext> context = new Mock<IExecutionContext>();
            Mock<IPlayerData> player = new Mock<IPlayerData>();
            Mock<IRemoteConfig> remoteConfig = new Mock<IRemoteConfig>();

            InventoryPersistence inventoryPersistence = new InventoryPersistence
            {
                Gears = new List<OwnedGearEntry>
                {
                    new OwnedGearEntry { InstanceId = "inst-a", GearId = "gear-1" }
                }
            };

            LoadoutPersistence loadoutPersistence = new LoadoutPersistence();

            remoteConfig
                .Setup(r => r.Get(context.Object, InventoryModule.ConfigKey, It.IsAny<InventoryConfig>()))
                .ReturnsAsync((IExecutionContext _, string __, InventoryConfig def) => new InventoryConfig
                {
                    BaseSlots = def.BaseSlots,
                    MotorCogGearId = "gear-1",
                    MotorCogStartX = def.MotorCogStartX,
                    MotorCogStartY = def.MotorCogStartY,
                });

            player.Setup(p => p.Get(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>()))
                .ReturnsAsync(inventoryPersistence);
            player.Setup(p => p.Get(context.Object, LoadoutModule.PersistenceKey, It.IsAny<LoadoutPersistence>()))
                .ReturnsAsync(loadoutPersistence);
            player.Setup(p => p.Set(context.Object, LoadoutModule.PersistenceKey, It.IsAny<LoadoutPersistence>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
            GameApiRegistry registry = new GameApiRegistry(typeof(SaveBoardLayoutHandler).Assembly);
            GameApiSession session = new GameApiSession(
                services,
                registry,
                context.Object,
                player.Object,
                Mock.Of<IGameState>(),
                remoteConfig.Object);

            SaveBoardLayoutHandler handler = new SaveBoardLayoutHandler();
            var request = new SaveBoardLayoutRequest(new[]
            {
                new LoadoutPlacement { InstanceId = "inst-a", GearId = "gear-1", X = 1, Y = 2 }
            });

            SaveBoardLayoutResponse response = await handler.HandleAsync(session, request).ConfigureAwait(false);

            Assert.True(response.SavedAtUtcTicks > 0);
            Assert.False(response.Rejected);
            player.Verify(
                p => p.Set(
                    context.Object,
                    LoadoutModule.PersistenceKey,
                    It.Is<LoadoutPersistence>(lp => lp.Board.Count == 1 && lp.Board[0].X == 1 && lp.Board[0].Y == 2 && lp.Board[0].InstanceId == "inst-a"),
                    false),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Rejects_When_Motor_Cog_Not_In_Placements()
        {
            Mock<IExecutionContext> context = new Mock<IExecutionContext>();
            Mock<IPlayerData> player = new Mock<IPlayerData>();
            Mock<IRemoteConfig> remoteConfig = new Mock<IRemoteConfig>();

            InventoryPersistence inventoryPersistence = new InventoryPersistence
            {
                Gears = new List<OwnedGearEntry>
                {
                    new OwnedGearEntry { InstanceId = "inst-a", GearId = "gear-1" }
                }
            };

            LoadoutPersistence loadoutPersistence = new LoadoutPersistence();

            remoteConfig
                .Setup(r => r.Get(context.Object, InventoryModule.ConfigKey, It.IsAny<InventoryConfig>()))
                .ReturnsAsync(new InventoryConfig { MotorCogGearId = "gear_core", BaseSlots = 8 });

            player.Setup(p => p.Get(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>()))
                .ReturnsAsync(inventoryPersistence);
            player.Setup(p => p.Get(context.Object, LoadoutModule.PersistenceKey, It.IsAny<LoadoutPersistence>()))
                .ReturnsAsync(loadoutPersistence);

            await using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
            GameApiRegistry registry = new GameApiRegistry(typeof(SaveBoardLayoutHandler).Assembly);
            GameApiSession session = new GameApiSession(
                services,
                registry,
                context.Object,
                player.Object,
                Mock.Of<IGameState>(),
                remoteConfig.Object);

            SaveBoardLayoutHandler handler = new SaveBoardLayoutHandler();
            var request = new SaveBoardLayoutRequest(new[]
            {
                new LoadoutPlacement { InstanceId = "inst-a", GearId = "gear-1", X = 0, Y = 0 }
            });

            SaveBoardLayoutResponse response = await handler.HandleAsync(session, request).ConfigureAwait(false);

            Assert.True(response.Rejected);
            Assert.Equal("missing_motor_cog", response.Reason);
            Assert.Equal(0, response.SavedAtUtcTicks);
            player.Verify(
                p => p.Set(context.Object, LoadoutModule.PersistenceKey, It.IsAny<LoadoutPersistence>(), It.IsAny<bool>()),
                Times.Never);
        }
    }
}
