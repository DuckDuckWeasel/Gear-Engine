using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.Modules.DTO.Inventory;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Inventory
{
    public class InventoryModule : GameModule<InventoryGameData>
    {
        public const string PersistenceKey = nameof(InventoryPersistence);
        public const string ConfigKey = nameof(InventoryConfig);

        public InventoryModule()
        {
        }

        public override async Task<IGameModuleData> InitializeAsync(GameApiSession session, CancellationToken cancellationToken = default)
        {
            IExecutionContext context = session.Context;
            IPlayerData player = session.Player;
            IRemoteConfig remoteConfig = session.RemoteConfig;
            InventoryConfig config = await remoteConfig.Get(context, ConfigKey, new InventoryConfig());
            InventoryPersistence persistence = await player.Get(context, PersistenceKey, new InventoryPersistence());

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
                await player.Set(context, PersistenceKey, persistence, false);
            }

            return new InventoryGameData(persistence, config);
        }
    }
}
