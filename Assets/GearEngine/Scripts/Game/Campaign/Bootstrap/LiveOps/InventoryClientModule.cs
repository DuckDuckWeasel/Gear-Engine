using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class InventoryClientModule : GameClientModuleBase<InventoryGameData>, IInventoryService
    {
        private readonly ILiveOpsService liveOpsService;
        private readonly GearCatalogSO catalog;

        public InventoryClientModule(IObjectResolver resolver, ILiveOpsService liveOps, GearCatalogSO catalog)
            : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public event Action InventoryChanged;

        public bool HasSavedInventory => data != null && data.GearIds.Count > 0;

        public IReadOnlyList<GearConfig> Owned => BuildOwnedList();

        private IReadOnlyList<GearConfig> BuildOwnedList()
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

        public bool TryAdd(GearConfig gear)
        {
            if (gear == null)
            {
                return false;
            }

            if (data == null)
            {
                Debug.LogError("[InventoryClientModule] TryAdd: module data is not initialized.");
                return false;
            }

            if (string.IsNullOrEmpty(gear.Id))
            {
                Debug.LogError("[InventoryClientModule] TryAdd: gear has no Id.");
                return false;
            }

            data.GearIds.Add(gear.Id);
            InventoryChanged?.Invoke();
            _ = SendInventoryAsync(new List<string>(data.GearIds));
            return true;
        }

        public bool TryRemove(GearConfig gear)
        {
            if (gear == null)
            {
                return false;
            }

            if (data?.GearIds == null)
            {
                return false;
            }

            int idx = data.GearIds.FindIndex(id => id == gear.Id);
            if (idx < 0)
            {
                return false;
            }

            data.GearIds.RemoveAt(idx);
            InventoryChanged?.Invoke();
            _ = SendInventoryAsync(new List<string>(data.GearIds));
            return true;
        }

        private async Task SendInventoryAsync(List<string> ids)
        {
            try
            {
                await liveOpsService.CallAsync(new SetInventoryRequest(ids));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InventoryClientModule] SendInventoryAsync failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
