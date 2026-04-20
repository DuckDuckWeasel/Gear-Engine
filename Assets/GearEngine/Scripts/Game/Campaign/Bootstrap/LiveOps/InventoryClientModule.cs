using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine.Config;
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

        public async Task SaveOwnedGearConfigsAsync(IReadOnlyList<GearConfig> gears, CancellationToken cancellationToken = default)
        {
            if (gears == null)
            {
                throw new ArgumentNullException(nameof(gears));
            }

            try
            {
                List<string> ids = gears.Where(g => g != null && !string.IsNullOrEmpty(g.Id)).Select(g => g.Id).ToList();
                SetInventoryResponse resp = await liveOpsService.CallAsync(new SetInventoryRequest(ids), cancellationToken);
                if (resp != null && data != null)
                {
                    data.GearIds = new List<string>(resp.GearIds);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InventoryClientModule] SaveOwnedGearConfigsAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
