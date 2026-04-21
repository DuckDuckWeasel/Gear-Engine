using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModule.Modules.Inventory;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Inventory;
using Moq;
using Unity.Services.CloudCode.Core;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class InventoryModuleInitializeTests
    {
        [Fact]
        public async Task Initialize_Inserts_Motor_Gear_When_Absent_And_Persists()
        {
            Mock<IExecutionContext> context = new Mock<IExecutionContext>();
            Mock<IPlayerData> player = new Mock<IPlayerData>();
            Mock<IRemoteConfig> remote = new Mock<IRemoteConfig>();

            remote.Setup(r => r.Get(context.Object, InventoryModule.ConfigKey, It.IsAny<InventoryConfig>()))
                .ReturnsAsync(new InventoryConfig { MotorCogGearId = "gear_core", BaseSlots = 8 });

            InventoryPersistence persistence = new InventoryPersistence { Gears = new List<OwnedGearEntry>() };
            player.Setup(p => p.Get(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>()))
                .ReturnsAsync(persistence);
            player.Setup(p => p.Set(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>(), false))
                .Returns(Task.CompletedTask)
                .Verifiable();

            InventoryModule module = new InventoryModule();
            IGameModuleData result = await module.Initialize(
                context.Object,
                player.Object,
                Mock.Of<IGameState>(),
                remote.Object).ConfigureAwait(false);

            InventoryGameData data = Assert.IsType<InventoryGameData>(result);
            Assert.Single(data.Gears);
            Assert.Equal("motor", data.Gears[0].InstanceId);
            Assert.Equal("gear_core", data.Gears[0].GearId);
            player.Verify(
                p => p.Set(
                    context.Object,
                    InventoryModule.PersistenceKey,
                    It.Is<InventoryPersistence>(ip =>
                        ip.Gears.Count == 1 &&
                        ip.Gears[0].InstanceId == "motor" &&
                        ip.Gears[0].GearId == "gear_core"),
                    false),
                Times.Once);
        }

        [Fact]
        public async Task Initialize_Does_Not_Insert_When_Motor_Gear_Already_Present()
        {
            Mock<IExecutionContext> context = new Mock<IExecutionContext>();
            Mock<IPlayerData> player = new Mock<IPlayerData>();
            Mock<IRemoteConfig> remote = new Mock<IRemoteConfig>();

            remote.Setup(r => r.Get(context.Object, InventoryModule.ConfigKey, It.IsAny<InventoryConfig>()))
                .ReturnsAsync(new InventoryConfig { MotorCogGearId = "gear_core", BaseSlots = 8 });

            InventoryPersistence persistence = new InventoryPersistence
            {
                Gears = new List<OwnedGearEntry>
                {
                    new OwnedGearEntry { InstanceId = "existing", GearId = "gear_core" },
                },
            };
            player.Setup(p => p.Get(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>()))
                .ReturnsAsync(persistence);

            InventoryModule module = new InventoryModule();
            IGameModuleData result = await module.Initialize(
                context.Object,
                player.Object,
                Mock.Of<IGameState>(),
                remote.Object).ConfigureAwait(false);

            InventoryGameData data = Assert.IsType<InventoryGameData>(result);
            Assert.Single(data.Gears);
            Assert.Equal("existing", data.Gears[0].InstanceId);
            player.Verify(
                p => p.Set(context.Object, InventoryModule.PersistenceKey, It.IsAny<InventoryPersistence>(), It.IsAny<bool>()),
                Times.Never);
        }
    }
}
