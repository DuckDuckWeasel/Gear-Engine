using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Inventory;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Inventory
{
    public class InventoryModule : GameModule<InventoryGameData>
    {
        public const string PersistenceKey = nameof(InventoryPersistence);
        public const string ConfigKey = nameof(InventoryConfig);

        public InventoryModule()
        {
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            InventoryConfig config = await remoteConfig.Get(context, ConfigKey, new InventoryConfig()).ConfigureAwait(false);
            InventoryPersistence persistence = await player.Get(context, PersistenceKey, new InventoryPersistence()).ConfigureAwait(false);

            if (persistence.Gears == null)
            {
                persistence.Gears = new List<OwnedGearEntry>();
            }

            bool mutated = false;

            bool hasMotorGear = persistence.Gears.Any(g => g != null && g.GearId == config.MotorCogGearId);
            if (!string.IsNullOrEmpty(config.MotorCogGearId) && !hasMotorGear)
            {
                persistence.Gears.Insert(0, new OwnedGearEntry
                {
                    InstanceId = "motor",
                    GearId = config.MotorCogGearId,
                });
                mutated = true;
            }

            if (!persistence.StartingGearsSeeded && config.StartingGearIds != null && config.StartingGearIds.Count > 0)
            {
                foreach (string gearId in config.StartingGearIds)
                {
                    if (string.IsNullOrEmpty(gearId))
                    {
                        continue;
                    }

                    persistence.Gears.Add(new OwnedGearEntry
                    {
                        InstanceId = $"start_{Guid.NewGuid():N}",
                        GearId = gearId,
                    });
                }

                persistence.StartingGearsSeeded = true;
                mutated = true;
            }

            if (mutated)
            {
                await player.Set(context, PersistenceKey, persistence, false).ConfigureAwait(false);
            }

            return new InventoryGameData(persistence, config);
        }
    }
}
