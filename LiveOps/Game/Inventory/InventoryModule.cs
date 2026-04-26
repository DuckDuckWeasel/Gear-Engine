using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.Keys;
using LiveOps.Modules.DTO.Inventory;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.Inventory
{
    public class InventoryModule : GameModule<InventoryGameData>
    {
        public static readonly string PersistenceKey = KeyOf<InventoryPersistence>.Module;
        public static readonly string ConfigKey = KeyOf<InventoryConfig>.Module;

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

            if (!persistence.StartingGearsSeeded && config.StartingGearIds != null && config.StartingGearIds.Count > 0)
            {
                for (int i = 0; i < config.StartingGearIds.Count; i++)
                {
                    string gearId = config.StartingGearIds[i];
                    if (string.IsNullOrEmpty(gearId))
                    {
                        continue;
                    }

                    string instanceId = i == 0 ? "motor" : $"start_{Guid.NewGuid():N}";
                    persistence.Gears.Add(new OwnedGearEntry
                    {
                        InstanceId = instanceId,
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
