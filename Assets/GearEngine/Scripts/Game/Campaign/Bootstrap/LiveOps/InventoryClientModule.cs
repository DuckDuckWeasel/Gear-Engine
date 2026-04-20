using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class InventoryClientModule : GameClientModuleBase<InventoryGameData>, IOwnedGearInventoryService
    {
        private readonly ILiveOpsService liveOpsService;
        private readonly GearCatalogSO catalog;

        public InventoryClientModule(IObjectResolver resolver, ILiveOpsService liveOps, GearCatalogSO catalog)
            : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public bool HasSavedInventory => data != null && data.GearIds.Count > 0;

        public IReadOnlyList<GearConfig> GetOwnedGearConfigs()
        {
            if (data == null)
            {
                return Array.Empty<GearConfig>();
            }

            var list = new List<GearConfig>(data.GearIds.Count);
            foreach (string id in data.GearIds)
            {
                GearConfig g = catalog.Get(id);
                if (g != null)
                {
                    list.Add(g);
                }
            }

            return list;
        }

        /// <summary>Called when the race inventory tray changes; persists optimistically and fires LiveOps in the background.</summary>
        public void PersistOwnedGearFromRaceInventory(IRaceInventoryService raceInventory)
        {
            if (raceInventory == null)
            {
                throw new ArgumentNullException(nameof(raceInventory));
            }

            List<string> ids = SnapshotGearIds(raceInventory);
            if (data != null)
            {
                data.GearIds = new List<string>(ids);
            }

            _ = SendInventoryAsync(ids);
        }

        private static List<string> SnapshotGearIds(IRaceInventoryService raceInventory)
        {
            var list = new List<string>();
            foreach (IItem item in raceInventory.GetInventory().Items)
            {
                if (item is GearConfigData cfgData && cfgData.SourceGearConfig != null && !string.IsNullOrEmpty(cfgData.SourceGearConfig.Id))
                {
                    list.Add(cfgData.SourceGearConfig.Id);
                }
            }

            return list;
        }

        private async Task SendInventoryAsync(List<string> ids)
        {
            try
            {
                await liveOpsService.CallAsync(new SetInventoryRequest(ids));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InventoryClientModule] PersistOwnedGearFromRaceInventory failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
